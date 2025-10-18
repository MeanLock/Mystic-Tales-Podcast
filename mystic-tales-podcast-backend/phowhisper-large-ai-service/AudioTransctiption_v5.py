from fastapi import FastAPI, File, UploadFile, HTTPException
from transformers import pipeline, AutoModelForSpeechSeq2Seq, AutoProcessor
from datetime import datetime
import tempfile
import os
import librosa
import soundfile as sf
import gc
import torch
import asyncio
from contextlib import contextmanager
from typing import Optional, Dict, Any, Union
import logging
from GPUManager import GPUManager
import io
import numpy as np
# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="PhoWhisper Transcription API v3 - CUDA Optimized")

# Global variables for GPU management
transcriber = None
device = None
model_config = {
    "torch_dtype": torch.float16,  # Use half precision for better GPU memory usage
    "use_safetensors": True,
    "low_cpu_mem_usage": True,
}


# Initialize GPU Manager
gpu_manager = GPUManager()


def load_model_optimized():
    global transcriber, device
    
    if transcriber is not None:
        return transcriber
    
    device = gpu_manager.device
    logger.info(f"Loading model on device: {device}")
    
    try:
        # Sử dụng local model hoặc specific revision
        local_model_path = "./PhoWhisper-large"
        
        if os.path.exists(local_model_path):
            logger.info(f"Loading local model from {local_model_path}")
            model_path = local_model_path
        else:
            logger.info("Loading model from Hugging Face with offline tolerance...")
            model_path = "vinai/PhoWhisper-large"
        
        # Load với options để bypass discussions
        with gpu_manager.gpu_memory_cleanup():
            if torch.cuda.is_available():
                try:
                    # Try với local_files_only nếu có cache (nếu không có trong cache sẽ failover sang download)
                    model = AutoModelForSpeechSeq2Seq.from_pretrained(
                        model_path,
                        torch_dtype=model_config["torch_dtype"],
                        low_cpu_mem_usage=model_config["low_cpu_mem_usage"],
                        use_safetensors=model_config["use_safetensors"],
                        device_map="auto",
                        local_files_only=False,   # Allow download
                        trust_remote_code=False,  # Security
                        use_auth_token=False,     # No authentication needed
                        force_download=False,     # Use cache if available
                        resume_download=True      # Resume interrupted downloads
                    )
                    
                    processor = AutoProcessor.from_pretrained(
                        model_path,
                        local_files_only=False,
                        use_auth_token=False
                    )
                except Exception as hf_error:
                    logger.warning(f"Hugging Face load failed: {hf_error}")
                    # Fallback to pipeline method
                    transcriber = pipeline(
                        "automatic-speech-recognition",
                        model=model_path,
                        torch_dtype=model_config["torch_dtype"],
                        device=device,
                        trust_remote_code=False
                    )
                    return transcriber
                
                # Create pipeline nếu model load thành công
                transcriber = pipeline(
                    "automatic-speech-recognition",
                    model=model,
                    tokenizer=processor.tokenizer,
                    feature_extractor=processor.feature_extractor,
                    torch_dtype=model_config["torch_dtype"],
                    device=device
                )
            else:
                # CPU fallback
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model=model_path,
                    torch_dtype=torch.float32,
                    trust_remote_code=False
                )
        
        logger.info("Model loaded successfully!")
        return transcriber
        
    except Exception as e:
        logger.error(f"Error loading PhoWhisper model: {e}")
        return _load_fallback_model()

@app.get("/gpu-info")
async def gpu_info():
    """Thông tin chi tiết về GPU và performance"""
    base_info = {
        "cuda_available": torch.cuda.is_available(),
        "cuda_version": torch.version.cuda,
        "torch_version": torch.__version__,
        "device": gpu_manager.device
    }
    
    if torch.cuda.is_available():
        memory_info = gpu_manager.get_memory_info()
        base_info.update(memory_info)
        
        # Additional GPU info
        base_info.update({
            "gpu_count": torch.cuda.device_count(),
            "current_device": torch.cuda.current_device(),
            "gpu_name": torch.cuda.get_device_name(0),
            "compute_capability": torch.cuda.get_device_capability(0),
            "is_fp16_supported": torch.cuda.is_available() and torch.cuda.get_device_capability(0)[0] >= 7
        })
    
    return base_info


async def _transcribe_short_audio(transcriber, audio_data: Union[np.ndarray, str], language: Optional[str] = None):
    """Transcribe short audio với GPU optimization - nhận trực tiếp audio data"""
    try:
        # audio_data có thể là numpy array hoặc file path
        result = transcriber(
            audio_data,
            generate_kwargs={
                "language": language,  # None for auto-detect, "vi" for Vietnamese, "en" for English
                "task": "transcribe",
                "use_cache": True,  # Enable caching
                "num_beams": 1,     # Faster inference 1=fast, 5=quality
                "do_sample": False,  # Deterministic output
            }
        )
        return result
    except (TypeError, KeyError):
        # Fallback nếu generate_kwargs không support
        logger.info("Using simple transcription mode...")
        return transcriber(audio_data)

async def _transcribe_long_audio(transcriber, audio_data: Union[np.ndarray, str], duration: float, language: Optional[str] = None):
    """Transcribe long audio với chunking strategy - nhận trực tiếp audio data"""
    try:
        # For long audio, use return_timestamps để process efficiently
        result = transcriber(
            audio_data,
            return_timestamps=True,
            chunk_length_s=30,  # Process in 30-second chunks
            generate_kwargs={
                "language": language,  # None for auto-detect, "vi" for Vietnamese, "en" for English
                "task": "transcribe",
                "use_cache": True,  # Enable caching
                "num_beams": 1,     # Faster inference 1=fast, 5=quality
                "do_sample": False  # Deterministic output
            }
        )
        return result
    except (TypeError, KeyError):
        # Fallback
        logger.info("Using simple long audio transcription...")
        return transcriber(audio_data, return_timestamps=True)

@app.post("/transcribe")
async def transcribe(file: UploadFile = File(...), language: Optional[str] = None):
    """Transcribe audio với GPU optimization - xử lý trực tiếp từ memory"""
    
    # Check memory before processing
    if not gpu_manager.check_memory_usage():
        logger.warning("High GPU memory usage detected, cleaning up...")
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
            gc.collect()
    
    # Load model khi cần thiết
    current_transcriber = load_model_optimized()
    
    if current_transcriber is None:
        raise HTTPException(status_code=503, detail="Model not available")
    
    # Validate file
    if not file.filename:
        raise HTTPException(status_code=400, detail="No filename provided")
    
    allowed_extensions = {'.wav', '.mp3', '.m4a', '.flac', '.ogg', '.aac'}
    file_ext = os.path.splitext(file.filename)[1].lower()
    if file_ext not in allowed_extensions:
        raise HTTPException(
            status_code=400, 
            detail=f"Unsupported file format. Allowed: {', '.join(allowed_extensions)}"
        )
    
    try:
        # Read file content vào memory
        content = await file.read()
        if len(content) == 0:
            raise HTTPException(status_code=400, detail="Empty file uploaded")
        
        logger.info(f"Processing file: {file.filename} ({len(content)} bytes)")
        
        # Xử lý audio trực tiếp từ memory
        try:
            # Tạo BytesIO object từ file content
            audio_buffer = io.BytesIO(content)
            
            # Load audio trực tiếp từ memory buffer
            audio, sr = librosa.load(audio_buffer, sr=16000, dtype=np.float32)
            
            if len(audio) == 0:
                raise HTTPException(status_code=400, detail="Invalid or corrupted audio file")
            
            # Normalize audio để tối ưu model performance
            audio = librosa.util.normalize(audio)
            audio_duration = len(audio) / sr
            
            logger.info(f"Audio processed: {audio_duration:.2f}s, shape: {audio.shape}, GPU: {torch.cuda.is_available()}")
            
        except Exception as audio_error:
            logger.error(f"Audio processing error: {audio_error}")
            raise HTTPException(
                status_code=400, 
                detail=f"Audio processing failed: {str(audio_error)}"
            )
        
        # GPU-optimized transcription với audio array trực tiếp
        with gpu_manager.gpu_memory_cleanup():
            try:
                # Use torch.no_grad() để tiết kiệm memory
                with torch.no_grad():
                    if audio_duration > 30:
                        # Long audio với chunk processing
                        logger.info("Processing long audio with chunking...")
                        result = await _transcribe_long_audio(current_transcriber, audio, audio_duration, language)
                    else:
                        # Short audio
                        logger.info("Processing short audio...")
                        result = await _transcribe_short_audio(current_transcriber, audio, language)

                # Extract text từ result
                if isinstance(result, dict):
                    if 'chunks' in result and result['chunks']:
                        output_text = ' '.join([chunk.get('text', '') for chunk in result['chunks'] if chunk.get('text')])
                    elif 'text' in result:
                        output_text = result['text']
                    else:
                        output_text = str(result)
                else:
                    output_text = str(result)
                
                # Ensure we have some output
                if not output_text.strip():
                    output_text = "No transcription generated"
                
            except Exception as transcribe_error:
                logger.error(f"Transcription error: {transcribe_error}")
                import traceback
                logger.error(f"Traceback: {traceback.format_exc()}")
                
                # Try simple fallback với audio array
                try:
                    logger.info("Attempting simple fallback transcription...")
                    with torch.no_grad():
                        simple_result = current_transcriber(audio)
                        output_text = simple_result.get('text', 'Transcription failed') if isinstance(simple_result, dict) else str(simple_result)
                except Exception as fallback_error:
                    logger.error(f"Fallback transcription also failed: {fallback_error}")
                    raise HTTPException(
                        status_code=500, 
                        detail=f"Transcription failed: {str(transcribe_error)}"
                    )
        
        # Memory usage sau khi transcribe
        memory_info = gpu_manager.get_memory_info()
        
        return {
            "transcript": output_text.strip(),
            "duration_seconds": audio_duration,
            "file_size_bytes": len(content),
            "audio_shape": list(audio.shape),
            "sample_rate": sr,
            "model_used": getattr(current_transcriber.model, 'name_or_path', 'unknown'),
            "device_used": str(device),
            "memory_info": memory_info if torch.cuda.is_available() else None,
            "processing_mode": "memory_direct"  # Indicator that we processed directly from memory
        }
        
    except HTTPException:
        # Re-raise HTTP exceptions as-is
        raise
    except Exception as e:
        # Catch any other unexpected errors
        logger.error(f"Unexpected error in transcribe endpoint: {e}")
        import traceback
        logger.error(f"Traceback: {traceback.format_exc()}")
        raise HTTPException(
            status_code=500, 
            detail=f"Internal server error: {str(e)}"
        )

@app.get("/health")
async def health_check():
    """Enhanced health check với GPU status"""
    model_info = "Not loaded"
    if transcriber is not None:
        try:
            model_info = getattr(transcriber.model, 'name_or_path', 'Loaded (unknown name)')
        except:
            model_info = "Loaded (unknown name)"
    
    memory_info = gpu_manager.get_memory_info() if torch.cuda.is_available() else None
    
    return {
        "status": "healthy",
        "version": "v3-cuda-optimized",
        "model_loaded": transcriber is not None,
        "model_info": model_info,
        "device": gpu_manager.device,
        "cuda_available": torch.cuda.is_available(),
        "memory_info": memory_info,
        "temp_dir": "./temp_audio_v3",
        "supported_formats": [".wav", ".mp3", ".m4a", ".flac", ".ogg", ".aac"]
    }

@app.get("/model-info")
async def model_info():
    """Thông tin chi tiết về model và GPU usage"""
    if transcriber is None:
        return {"error": "Model not loaded"}
    
    try:
        info = {
            "model_name": getattr(transcriber.model, 'name_or_path', 'Unknown'),
            "model_type": type(transcriber.model).__name__,
            "device": str(device),
            "torch_dtype": str(model_config["torch_dtype"]),
            "memory_info": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
        
        # Model specific info
        if hasattr(transcriber.model, 'config'):
            config = transcriber.model.config
            info.update({
                "model_size": getattr(config, 'd_model', 'unknown'),
                "vocab_size": getattr(config, 'vocab_size', 'unknown'),
                "max_length": getattr(config, 'max_length', 'unknown')
            })
        
        return info
    except Exception as e:
        return {"error": f"Could not get model info: {str(e)}"}

@app.get("/benchmark")
async def benchmark():
    """Simple benchmark để test GPU performance"""
    if transcriber is None:
        return {"error": "Model not loaded"}
    
    # Create dummy audio for benchmark
    import numpy as np
    dummy_audio = np.random.randn(16000 * 5).astype(np.float32)  # 5 seconds
    temp_path = "./temp_audio_v3/benchmark.wav"
    
    os.makedirs("./temp_audio_v3", exist_ok=True)
    sf.write(temp_path, dummy_audio, 16000)
    
    try:
        start_time = datetime.now()
        
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = transcriber(temp_path)
        
        end_time = datetime.now()
        processing_time = (end_time - start_time).total_seconds()
        
        return {
            "processing_time_seconds": processing_time,
            "audio_duration_seconds": 5.0,
            "real_time_factor": processing_time / 5.0,
            "device": device,
            "memory_info": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
        
    finally:
        if os.path.exists(temp_path):
            os.unlink(temp_path)

@app.on_event("startup")
async def startup_event():
    """Initialize model on startup"""
    logger.info("Starting up API...")
    logger.info(f"GPU Manager initialized with device: {gpu_manager.device}")
    
    # Pre-load model for better first request performance
    try:
        await asyncio.get_event_loop().run_in_executor(None, load_model_optimized)
        logger.info("Model pre-loaded successfully")
    except Exception as e:
        logger.warning(f"Could not pre-load model: {e}")

@app.on_event("shutdown")
async def shutdown_event():
    """Enhanced cleanup khi shutdown"""
    global transcriber
    
    logger.info("Shutting down API...")
    
    if transcriber is not None:
        try:
            # Move model to CPU before deletion để free GPU memory
            if hasattr(transcriber, 'model') and torch.cuda.is_available():
                transcriber.model.cpu()
            del transcriber
        except Exception as e:
            logger.warning(f"Error during model cleanup: {e}")
        transcriber = None
    
    # Comprehensive GPU cleanup
    if torch.cuda.is_available():
        try:
            torch.cuda.empty_cache()
            torch.cuda.synchronize()  # Wait for all operations to complete
        except Exception as e:
            logger.warning(f"GPU cleanup error: {e}")
    
    gc.collect()
    
    # Clean temp directories
    for temp_dir in ["./temp_audio_v3", "./temp_audio_v2", "./temp_audio"]:
        if os.path.exists(temp_dir):
            try:
                import shutil
                shutil.rmtree(temp_dir)
                logger.info(f"Cleaned up temp directory: {temp_dir}")
            except Exception as e:
                logger.warning(f"Could not clean up temp directory {temp_dir}: {e}")

