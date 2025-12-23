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
from typing import Optional, Dict, Any
import logging
from GPUManager import GPUManager
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
        # return _load_fallback_model()

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

# async def _transcribe_dual_pass(transcriber, audio_path: str, duration: float):
#     """Dual pass: chạy Vietnamese và English, merge results - works for both short and long audio"""
    
#     # Determine if we need chunking based on duration
#     use_chunks = duration > 30
    
#     # Pass 1: Vietnamese
#     try:
#         with torch.no_grad():
#             if use_chunks:
#                 vi_result = transcriber(
#                     audio_path,
#                     return_timestamps=True,
#                     chunk_length_s=30,
#                     generate_kwargs={
#                         "language": "vi",
#                         "task": "transcribe",
#                         "num_beams": 2,
#                         "do_sample": False,
#                         "use_cache": True
#                     }
#                 )
#             else:
#                 # Short audio - no chunking, but still get timestamps for merging
#                 vi_result = transcriber(
#                     audio_path,
#                     return_timestamps=True,
#                     generate_kwargs={
#                         "language": "vi",
#                         "task": "transcribe",
#                         "num_beams": 2,
#                         "do_sample": False,
#                         "use_cache": True
#                     }
#                 )
#     except Exception as e:
#         logger.warning(f"Vietnamese pass failed: {e}")
#         vi_result = {"text": "", "chunks": []}
    
#     # Pass 2: English
#     try:
#         with torch.no_grad():
#             if use_chunks:
#                 en_result = transcriber(
#                     audio_path,
#                     return_timestamps=True,
#                     chunk_length_s=30,
#                     generate_kwargs={
#                         "language": "en",
#                         "task": "transcribe",
#                         "num_beams": 2,
#                         "do_sample": False,
#                         "use_cache": True
#                     }
#                 )
#             else:
#                 # Short audio - no chunking, but still get timestamps for merging
#                 en_result = transcriber(
#                     audio_path,
#                     return_timestamps=True,
#                     generate_kwargs={
#                         "language": "en",
#                         "task": "transcribe",
#                         "num_beams": 2,
#                         "do_sample": False,
#                         "use_cache": True
#                     }
#                 )
#     except Exception as e:
#         logger.warning(f"English pass failed: {e}")
#         en_result = {"text": "", "chunks": []}
    
#     # Simple merge: choose best chunk based on language detection
#     vi_chunks = vi_result.get("chunks", [])
#     en_chunks = en_result.get("chunks", [])
    
#     merged_chunks = []
#     merged_text_parts = []
    
#     # Get all timestamps
#     all_times = set()
#     for chunk in vi_chunks + en_chunks:
#         if "timestamp" in chunk:
#             all_times.add(chunk["timestamp"])
    
#     for timestamp in sorted(all_times):
#         vi_chunk = next((c for c in vi_chunks if c.get("timestamp") == timestamp), None)
#         en_chunk = next((c for c in en_chunks if c.get("timestamp") == timestamp), None)
        
#         if vi_chunk and en_chunk:
#             vi_text = vi_chunk.get("text", "").strip()
#             en_text = en_chunk.get("text", "").strip()
            
#             # Simple language detection: check for Vietnamese diacritics
#             import re
#             has_vietnamese = bool(re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', vi_text.lower()))
            
#             if has_vietnamese:
#                 chosen_text = vi_text
#                 chosen_lang = "vi"
#             else:
#                 chosen_text = en_text
#                 chosen_lang = "en"
            
#             if chosen_text:
#                 merged_chunks.append({
#                     "text": chosen_text,
#                     "timestamp": timestamp,
#                     "language": chosen_lang
#                 })
#                 merged_text_parts.append(chosen_text)
        
#         elif vi_chunk and vi_chunk.get("text", "").strip():
#             merged_chunks.append({
#                 "text": vi_chunk["text"],
#                 "timestamp": timestamp,
#                 "language": "vi"
#             })
#             merged_text_parts.append(vi_chunk["text"])
        
#         elif en_chunk and en_chunk.get("text", "").strip():
#             merged_chunks.append({
#                 "text": en_chunk["text"],
#                 "timestamp": timestamp,
#                 "language": "en"
#             })
#             merged_text_parts.append(en_chunk["text"])
    
#     return {
#         "text": " ".join(merged_text_parts),
#         "chunks": merged_chunks,
#         "vietnamese_text": vi_result.get("text", ""),
#         "english_text": en_result.get("text", ""),
#         "dual_pass": True
#     }

async def _transcribe_dual_pass(transcriber, audio_path: str, duration: float):
    """Dual pass: chạy Vietnamese và English, merge results - works for both short and long audio"""
    
    # Determine if we need chunking based on duration
    use_chunks = duration > 30
    
    # Pass 1: Vietnamese
    try:
        with torch.no_grad():
            if use_chunks:
                vi_result = transcriber(
                    audio_path,
                    return_timestamps=True,
                    chunk_length_s=30,
                    generate_kwargs={
                        "language": "vi",
                        "task": "transcribe",
                        "num_beams": 2,
                        "do_sample": False,
                        "use_cache": True
                    }
                )
            else:
                # Short audio - no chunking, but still get timestamps for merging
                vi_result = transcriber(
                    audio_path,
                    return_timestamps=True,
                    generate_kwargs={
                        "language": "vi",
                        "task": "transcribe",
                        "num_beams": 2,
                        "do_sample": False,
                        "use_cache": True
                    }
                )
    except Exception as e:
        logger.warning(f"Vietnamese pass failed: {e}")
        vi_result = {"text": "", "chunks": []}
    
    # Pass 2: English
    try:
        with torch.no_grad():
            if use_chunks:
                en_result = transcriber(
                    audio_path,
                    return_timestamps=True,
                    chunk_length_s=30,
                    generate_kwargs={
                        "language": "en",
                        "task": "transcribe",
                        "num_beams": 2,
                        "do_sample": False,
                        "use_cache": True
                    }
                )
            else:
                # Short audio - no chunking, but still get timestamps for merging
                en_result = transcriber(
                    audio_path,
                    return_timestamps=True,
                    generate_kwargs={
                        "language": "en",
                        "task": "transcribe",
                        "num_beams": 2,
                        "do_sample": False,
                        "use_cache": True
                    }
                )
    except Exception as e:
        logger.warning(f"English pass failed: {e}")
        en_result = {"text": "", "chunks": []}
    
    # Simple merge: choose best chunk based on language detection
    vi_chunks = vi_result.get("chunks", [])
    en_chunks = en_result.get("chunks", [])
    
    merged_chunks = []
    merged_text_parts = []
    
    # FIX: Handle timestamp parsing more safely
    def extract_timestamp(chunk):
        """Safely extract timestamp from chunk"""
        if not isinstance(chunk, dict):
            return None
        
        timestamp = chunk.get("timestamp")
        if timestamp is None:
            return None
        
        # Handle different timestamp formats
        if isinstance(timestamp, (list, tuple)) and len(timestamp) >= 1:
            # Format: [start, end] or (start, end)
            start_time = timestamp[0]
            return start_time if start_time is not None else 0.0
        elif isinstance(timestamp, (int, float)):
            # Format: single number
            return float(timestamp)
        else:
            return None
    
    # Get all valid timestamps
    all_timestamps = []
    
    for chunk in vi_chunks:
        ts = extract_timestamp(chunk)
        if ts is not None:
            all_timestamps.append(ts)
    
    for chunk in en_chunks:
        ts = extract_timestamp(chunk)
        if ts is not None:
            all_timestamps.append(ts)
    
    # Remove duplicates and sort
    unique_timestamps = sorted(set(all_timestamps))
    
    logger.info(f"Processing {len(unique_timestamps)} unique timestamps")
    
    # Process each timestamp
    for timestamp in unique_timestamps:
        # Find matching chunks với tolerance
        tolerance = 0.1  # 100ms tolerance
        
        vi_chunk = None
        en_chunk = None
        
        for chunk in vi_chunks:
            chunk_ts = extract_timestamp(chunk)
            if chunk_ts is not None and abs(chunk_ts - timestamp) <= tolerance:
                vi_chunk = chunk
                break
        
        for chunk in en_chunks:
            chunk_ts = extract_timestamp(chunk)
            if chunk_ts is not None and abs(chunk_ts - timestamp) <= tolerance:
                en_chunk = chunk
                break
        
        # Choose best text
        chosen_text = ""
        chosen_lang = "unknown"
        
        if vi_chunk and en_chunk:
            vi_text = vi_chunk.get("text", "").strip()
            en_text = en_chunk.get("text", "").strip()
            
            # Simple language detection: check for Vietnamese diacritics
            import re
            has_vietnamese = bool(re.search(r'[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]', vi_text.lower()))
            
            if has_vietnamese:
                chosen_text = vi_text
                chosen_lang = "vi"
            else:
                chosen_text = en_text
                chosen_lang = "en"
        
        elif vi_chunk:
            chosen_text = vi_chunk.get("text", "").strip()
            chosen_lang = "vi"
        
        elif en_chunk:
            chosen_text = en_chunk.get("text", "").strip()
            chosen_lang = "en"
        
        # Add to results if we have text
        if chosen_text:
            merged_chunks.append({
                "text": chosen_text,
                "timestamp": timestamp,
                "language": chosen_lang
            })
            merged_text_parts.append(chosen_text)
    
    # Fallback: if no chunks found, use the full text from results
    if not merged_text_parts:
        vi_text = vi_result.get("text", "").strip()
        en_text = en_result.get("text", "").strip()
        
        if vi_text:
            merged_text_parts.append(vi_text)
        if en_text:
            merged_text_parts.append(en_text)
    
    return {
        "text": " ".join(merged_text_parts),
        "chunks": merged_chunks,
        "vietnamese_text": vi_result.get("text", ""),
        "english_text": en_result.get("text", ""),
        "dual_pass": True,
        "total_chunks_processed": len(merged_chunks)
    }

# Modify existing transcribe endpoint - thêm parameter
@app.post("/transcribe")
async def transcribe(
    file: UploadFile = File(...),
    mode: str = "auto"  # auto, dual_pass
):
    """Transcribe audio với GPU optimization"""
    
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
    
    # Validate mode
    if mode not in ["auto", "dual_pass"]:
        raise HTTPException(status_code=400, detail="Mode must be 'auto' or 'dual_pass'")
    
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
    
    # Setup temp directory
    temp_dir = "./temp_audio_v3"
    os.makedirs(temp_dir, exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S_%f")
    input_filename = f"input_{timestamp}{file_ext}"
    output_filename = f"output_{timestamp}.wav"
    
    input_path = os.path.join(temp_dir, input_filename)
    output_path = os.path.join(temp_dir, output_filename)
    
    try:
        # Read and validate file
        content = await file.read()
        if len(content) == 0:
            raise HTTPException(status_code=400, detail="Empty file uploaded")
            
        with open(input_path, 'wb') as f:
            f.write(content)
        
        # Audio processing với error handling
        try:
            audio, sr = librosa.load(input_path, sr=16000)
            if len(audio) == 0:
                raise HTTPException(status_code=400, detail="Invalid or corrupted audio file")
            
            sf.write(output_path, audio, 16000)
            audio_duration = len(audio) / sr
            
            logger.info(f"Processing audio: {audio_duration:.2f}s, Mode: {mode}, GPU: {torch.cuda.is_available()}")
            
        except Exception as audio_error:
            raise HTTPException(
                status_code=400, 
                detail=f"Audio processing failed: {str(audio_error)}"
            )
        
        # GPU-optimized transcription
        with gpu_manager.gpu_memory_cleanup():
            try:
                # Use torch.no_grad() để tiết kiệm memory
                with torch.no_grad():
                    if mode == "dual_pass":
                        # Dual pass mode
                        logger.info("Processing with dual pass mode...")
                        result = await _transcribe_dual_pass(current_transcriber, output_path, audio_duration)
                    elif audio_duration > 30:
                        # Long audio với chunk processing
                        logger.info("Processing long audio with chunking...")
                        result = await _transcribe_long_audio(current_transcriber, output_path, audio_duration)
                    else:
                        # Short audio
                        logger.info("Processing short audio...")
                        result = await _transcribe_short_audio(current_transcriber, output_path)
                
                # Extract text từ result
                if isinstance(result, dict):
                    if 'chunks' in result:
                        output_text = ' '.join([chunk['text'] for chunk in result['chunks']])
                    elif 'text' in result:
                        output_text = result['text']
                    else:
                        output_text = str(result)
                else:
                    output_text = str(result)
                
            except Exception as transcribe_error:
                logger.error(f"Transcription error: {transcribe_error}")
                # in ra dòng lỗi chi tiết
                logger.error(f"Transcription error details: {transcribe_error}")
                raise HTTPException(
                    status_code=500, 
                    detail=f"Transcription failed: {str(transcribe_error)}"
                )
        
        # Memory usage sau khi transcribe
        memory_info = gpu_manager.get_memory_info()
        
        # Build response
        response = {
            "transcript": output_text.strip(),
            "duration_seconds": audio_duration,
            "file_size_bytes": len(content),
            "model_used": getattr(current_transcriber.model, 'name_or_path', 'unknown'),
            "device_used": device,
            "mode": mode,
            "memory_info": memory_info if torch.cuda.is_available() else None
        }
        
        # Add dual pass specific info
        if mode == "dual_pass" and isinstance(result, dict):
            response.update({
                "chunks": result.get("chunks", []),
                "vietnamese_text": result.get("vietnamese_text", ""),
                "english_text": result.get("english_text", ""),
                "dual_pass_info": "Merged from Vietnamese and English passes"
            })
        
        return response
        
    finally:
        # Cleanup temp files
        for temp_file in [input_path, output_path]:
            if os.path.exists(temp_file):
                try:
                    os.unlink(temp_file)
                except Exception as e:
                    logger.warning(f"Could not delete temp file {temp_file}: {e}")

async def _transcribe_short_audio(transcriber, audio_path: str):
    """Transcribe short audio với GPU optimization"""
    try:
        result = transcriber(
            audio_path,
            generate_kwargs={
                "language": None, # None for auto-detect, "vi" for Vietnamese, "en" for English
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
        return transcriber(audio_path)

async def _transcribe_long_audio(transcriber, audio_path: str, duration: float):
    """Transcribe long audio với chunking strategy"""
    try:
        # For long audio, use return_timestamps để process efficiently
        result = transcriber(
            audio_path,
            return_timestamps=True,
            chunk_length_s=30,  # Process in 30-second chunks
            generate_kwargs={
                "language": None, # None for auto-detect, "vi" for Vietnamese, "en" for English
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
        return transcriber(audio_path, return_timestamps=True)

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

