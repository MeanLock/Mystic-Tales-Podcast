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

app = FastAPI(title="PhoWhisper Transcription API v4 - Proper Long-form")

# Global variables for GPU management
transcriber = None
model = None
processor = None
device = None

model_config = {
    "torch_dtype": torch.float16,  # Use half precision for better GPU memory usage
    "use_safetensors": True,
    "low_cpu_mem_usage": True,
}

# Initialize GPU Manager
gpu_manager = GPUManager()


def load_model_optimized():
    """Load model và processor riêng biệt để support proper long-form transcription"""
    global transcriber, model, processor, device
    
    if transcriber is not None and model is not None:
        return transcriber, model, processor
    
    device = gpu_manager.device
    logger.info(f"Loading model on device: {device}")
    
    try:
        # Sử dụng local model hoặc specific revision
        local_model_path = "./PhoWhisper-large"
        
        if os.path.exists(local_model_path):
            logger.info(f"Loading local model from {local_model_path}")
            model_path = local_model_path
        else:
            logger.info("Loading model from Hugging Face...")
            model_path = "vinai/PhoWhisper-large"
        
        # Load model và processor với GPU optimization
        with gpu_manager.gpu_memory_cleanup():
            if torch.cuda.is_available():
                try:
                    # Load model
                    model = AutoModelForSpeechSeq2Seq.from_pretrained(
                        model_path,
                        torch_dtype=model_config["torch_dtype"],
                        low_cpu_mem_usage=model_config["low_cpu_mem_usage"],
                        use_safetensors=model_config["use_safetensors"],
                        device_map="auto",
                        local_files_only=False,
                        trust_remote_code=False,
                        token=None,
                        force_download=False,
                        resume_download=True
                    )
                    
                    # Load processor
                    processor = AutoProcessor.from_pretrained(
                        model_path,
                        local_files_only=False,
                        use_auth_token=False
                    )
                    
                    # Create pipeline for short audio
                    transcriber = pipeline(
                        "automatic-speech-recognition",
                        model=model,
                        tokenizer=processor.tokenizer,
                        feature_extractor=processor.feature_extractor,
                        torch_dtype=model_config["torch_dtype"],
                        device=device
                    )
                    
                except Exception as hf_error:
                    logger.warning(f"Hugging Face load failed: {hf_error}")
                    # Fallback to simple pipeline
                    transcriber = pipeline(
                        "automatic-speech-recognition",
                        model=model_path,
                        torch_dtype=model_config["torch_dtype"],
                        device=device,
                        trust_remote_code=False
                    )
                    model = transcriber.model
                    processor = transcriber.tokenizer
                    
            else:
                # CPU fallback
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model=model_path,
                    torch_dtype=torch.float32,
                    trust_remote_code=False
                )
                model = transcriber.model
                processor = transcriber.tokenizer
        
        logger.info("Model loaded successfully!")
        return transcriber, model, processor
        
    except Exception as e:
        logger.error(f"Error loading PhoWhisper model: {e}")
        raise


async def _transcribe_short_audio(transcriber, audio_data: Union[np.ndarray, str], language: Optional[str] = None):
    """Transcribe short audio (<30s) với pipeline - fastest method"""
    try:
        result = transcriber(
            audio_data,
            generate_kwargs={
                "language": language,
                "task": "transcribe",
                "use_cache": True,
                "num_beams": 1,
                "do_sample": False,
            }
        )
        return result
    except (TypeError, KeyError):
        logger.info("Using simple transcription mode...")
        return transcriber(audio_data)


async def _transcribe_long_audio_proper(model, processor, audio_data: np.ndarray, language: Optional[str] = None):
    """
    Proper long-form transcription theo Whisper paper (Section 3.8)
    Sử dụng model.generate() thay vì pipeline chunking
    Tránh warning và có độ chính xác cao hơn
    """
    try:
        logger.info("Using proper long-form transcription with model.generate()...")
        
        # Prepare input features
        input_features = processor(
            audio_data, 
            sampling_rate=16000, 
            return_tensors="pt"
        ).input_features
        
        # Move to GPU if available
        if torch.cuda.is_available():
            input_features = input_features.to(device)
            model_device = next(model.parameters()).device
            logger.info(f"Input on: {input_features.device}, Model on: {model_device}")
        
        # Set language token if specified
        forced_decoder_ids = None
        if language:
            # Get language token
            try:
                lang_token = processor.tokenizer.convert_tokens_to_ids(f"<|{language}|>")
                forced_decoder_ids = [[1, lang_token], [2, processor.tokenizer.convert_tokens_to_ids("<|transcribe|>")]]
            except:
                logger.warning(f"Could not set language to {language}, using auto-detect")
        
        # Generate với proper Whisper parameters
        with torch.no_grad():
            predicted_ids = model.generate(
                input_features,
                forced_decoder_ids=forced_decoder_ids,
                max_new_tokens=448,  # Whisper default
                num_beams=1,  # Greedy decoding for speed
                do_sample=False,
                return_timestamps=False,  # Set to True if you need timestamps
            )
        
        # Decode
        transcription = processor.batch_decode(
            predicted_ids, 
            skip_special_tokens=True
        )[0]
        
        return {"text": transcription.strip()}
        
    except Exception as e:
        logger.error(f"Proper long-form transcription failed: {e}")
        import traceback
        logger.error(f"Traceback: {traceback.format_exc()}")
        raise


async def _transcribe_long_audio_chunked(transcriber, model, processor, audio: np.ndarray, language: Optional[str] = None):
    """
    Fallback method: Manual chunking nếu model.generate() fail
    Chia audio thành chunks 30s với overlap 5s
    """
    try:
        logger.info("Using manual chunking fallback method...")
        
        # Chunking parameters
        chunk_length = 30  # seconds
        overlap = 5  # seconds
        sample_rate = 16000
        
        chunk_samples = chunk_length * sample_rate
        overlap_samples = overlap * sample_rate
        step = chunk_samples - overlap_samples
        
        # Create chunks
        chunks = []
        for start in range(0, len(audio), step):
            end = min(start + chunk_samples, len(audio))
            chunks.append(audio[start:end])
            if end >= len(audio):
                break
        
        logger.info(f"Processing {len(chunks)} chunks...")
        
        all_transcripts = []
        
        for i, chunk in enumerate(chunks):
            logger.info(f"Processing chunk {i+1}/{len(chunks)}")
            
            # Use short audio method for each chunk
            result = await _transcribe_short_audio(transcriber, chunk, language)
            
            text = result.get('text', '') if isinstance(result, dict) else str(result)
            all_transcripts.append(text.strip())
            
            # Clear GPU cache after each chunk
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
        
        # Combine transcripts
        full_transcript = ' '.join(all_transcripts)
        
        return {"text": full_transcript}
        
    except Exception as e:
        logger.error(f"Manual chunking failed: {e}")
        raise


@app.post("/transcribe")
async def transcribe(AudioFile: UploadFile = File(...), language: Optional[str] = None):
    """
    Transcribe audio với proper long-form support
    - Short audio (<30s): Dùng pipeline (fastest)
    - Long audio (≥30s): Dùng model.generate() (proper method, no warnings)
    """
    
    # Check memory before processing
    if not gpu_manager.check_memory_usage():
        logger.warning("High GPU memory usage detected, cleaning up...")
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
            gc.collect()
    
    # Load model
    try:
        current_transcriber, current_model, current_processor = load_model_optimized()
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"Model loading failed: {str(e)}")
    
    if current_transcriber is None:
        raise HTTPException(status_code=503, detail="Model not available")
    
    # Validate file
    if not AudioFile.filename:
        raise HTTPException(status_code=400, detail="No filename provided")
    
    allowed_extensions = {'.wav', '.mp3', '.m4a', '.flac', '.ogg', '.aac'}
    file_ext = os.path.splitext(AudioFile.filename)[1].lower()
    if file_ext not in allowed_extensions:
        raise HTTPException(
            status_code=400, 
            detail=f"Unsupported file format. Allowed: {', '.join(allowed_extensions)}"
        )
    
    try:
        # Read file content into memory
        content = await AudioFile.read()
        if len(content) == 0:
            raise HTTPException(status_code=400, detail="Empty file uploaded")
        
        logger.info(f"Processing file: {AudioFile.filename} ({len(content)} bytes)")
        
        # Process audio directly from memory
        try:
            audio_buffer = io.BytesIO(content)
            audio, sr = librosa.load(audio_buffer, sr=16000, dtype=np.float32)
            
            if len(audio) == 0:
                raise HTTPException(status_code=400, detail="Invalid or corrupted audio file")
            
            # Normalize audio
            audio = librosa.util.normalize(audio)
            audio_duration = len(audio) / sr
            
            logger.info(f"Audio processed: {audio_duration:.2f}s, shape: {audio.shape}, GPU: {torch.cuda.is_available()}")
            
        except Exception as audio_error:
            logger.error(f"Audio processing error: {audio_error}")
            raise HTTPException(
                status_code=400, 
                detail=f"Audio processing failed: {str(audio_error)}"
            )
        
        # GPU-optimized transcription
        with gpu_manager.gpu_memory_cleanup():
            try:
                with torch.no_grad():
                    if audio_duration > 30:
                        logger.info(f"Long audio detected ({audio_duration:.2f}s), using proper long-form transcription...")
                        
                        # Try proper method first
                        try:
                            result = await _transcribe_long_audio_proper(
                                current_model,
                                current_processor,
                                audio,
                                language
                            )
                            processing_method = "model.generate() - proper long-form"
                            
                        except Exception as proper_error:
                            logger.warning(f"Proper method failed: {proper_error}, falling back to chunking...")
                            
                            # Fallback to manual chunking
                            result = await _transcribe_long_audio_chunked(
                                current_transcriber,
                                current_model,
                                current_processor,
                                audio,
                                language
                            )
                            processing_method = "manual chunking fallback"
                    else:
                        logger.info(f"Short audio detected ({audio_duration:.2f}s), using fast pipeline...")
                        result = await _transcribe_short_audio(current_transcriber, audio, language)
                        processing_method = "pipeline - fast mode"

                # Extract text from result
                if isinstance(result, dict):
                    if 'chunks' in result and result['chunks']:
                        output_text = ' '.join([chunk.get('text', '') for chunk in result['chunks'] if chunk.get('text')])
                    elif 'text' in result:
                        output_text = result['text']
                    else:
                        output_text = str(result)
                else:
                    output_text = str(result)
                
                # Ensure we have output
                if not output_text.strip():
                    output_text = "No transcription generated"
                
            except Exception as transcribe_error:
                logger.error(f"Transcription error: {transcribe_error}")
                import traceback
                logger.error(f"Traceback: {traceback.format_exc()}")
                
                # Final fallback
                try:
                    logger.info("Attempting final simple fallback...")
                    with torch.no_grad():
                        simple_result = current_transcriber(audio)
                        output_text = simple_result.get('text', 'Transcription failed') if isinstance(simple_result, dict) else str(simple_result)
                        processing_method = "simple fallback"
                except Exception as fallback_error:
                    logger.error(f"All transcription methods failed: {fallback_error}")
                    raise HTTPException(
                        status_code=500, 
                        detail=f"Transcription failed: {str(transcribe_error)}"
                    )
        
        # Memory usage after transcription
        memory_info = gpu_manager.get_memory_info()
        
        return {
            "Transcript": output_text.strip(),
            "DurationSeconds": audio_duration,
            "FileSizeBytes": len(content),
            "AudioShape": list(audio.shape),
            "SampleRate": sr,
            "ModelUsed": getattr(current_model, 'name_or_path', 'unknown'),
            "DeviceUsed": str(device),
            "ProcessingMethod": processing_method,
            "MemoryInfo": memory_info if torch.cuda.is_available() else None,
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Unexpected error in transcribe endpoint: {e}")
        import traceback
        logger.error(f"Traceback: {traceback.format_exc()}")
        raise HTTPException(
            status_code=500, 
            detail=f"Internal server error: {str(e)}"
        )


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
        
        base_info.update({
            "gpu_count": torch.cuda.device_count(),
            "current_device": torch.cuda.current_device(),
            "gpu_name": torch.cuda.get_device_name(0),
            "compute_capability": torch.cuda.get_device_capability(0),
            "is_fp16_supported": torch.cuda.is_available() and torch.cuda.get_device_capability(0)[0] >= 7
        })
    
    return base_info


@app.get("/health")
async def health_check():
    """Enhanced health check với GPU status"""
    model_info = "Not loaded"
    if transcriber is not None:
        try:
            model_info = getattr(transcriber.model, 'name_or_path', 'Loaded')
        except:
            model_info = "Loaded"
    
    memory_info = gpu_manager.get_memory_info() if torch.cuda.is_available() else None
    
    return {
        "status": "healthy",
        "version": "v4-proper-longform",
        "model_loaded": transcriber is not None,
        "model_info": model_info,
        "device": gpu_manager.device,
        "cuda_available": torch.cuda.is_available(),
        "memory_info": memory_info,
        "supported_formats": [".wav", ".mp3", ".m4a", ".flac", ".ogg", ".aac"],
        "features": [
            "Short audio: Fast pipeline mode",
            "Long audio: Proper model.generate() (no warnings)",
            "Fallback: Manual chunking with overlap"
        ]
    }


@app.get("/model-info")
async def model_info():
    """Thông tin chi tiết về model và GPU usage"""
    if model is None:
        return {"error": "Model not loaded"}
    
    try:
        info = {
            "model_name": getattr(model, 'name_or_path', 'Unknown'),
            "model_type": type(model).__name__,
            "device": str(device),
            "torch_dtype": str(model_config["torch_dtype"]),
            "memory_info": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
        
        if hasattr(model, 'config'):
            config = model.config
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
    """Benchmark để test GPU performance với cả short và long audio"""
    if transcriber is None or model is None:
        return {"error": "Model not loaded"}
    
    results = {}
    
    # Benchmark 1: Short audio (5s)
    try:
        dummy_audio_short = np.random.randn(16000 * 5).astype(np.float32)
        
        start_time = datetime.now()
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = await _transcribe_short_audio(transcriber, dummy_audio_short)
        end_time = datetime.now()
        
        results["short_audio_5s"] = {
            "processing_time_seconds": (end_time - start_time).total_seconds(),
            "audio_duration_seconds": 5.0,
            "real_time_factor": (end_time - start_time).total_seconds() / 5.0,
            "method": "pipeline"
        }
    except Exception as e:
        results["short_audio_5s"] = {"error": str(e)}
    
    # Benchmark 2: Long audio (60s)
    try:
        dummy_audio_long = np.random.randn(16000 * 60).astype(np.float32)
        
        start_time = datetime.now()
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = await _transcribe_long_audio_proper(model, processor, dummy_audio_long)
        end_time = datetime.now()
        
        results["long_audio_60s"] = {
            "processing_time_seconds": (end_time - start_time).total_seconds(),
            "audio_duration_seconds": 60.0,
            "real_time_factor": (end_time - start_time).total_seconds() / 60.0,
            "method": "model.generate()"
        }
    except Exception as e:
        results["long_audio_60s"] = {"error": str(e)}
    
    results["device"] = device
    results["memory_info"] = gpu_manager.get_memory_info() if torch.cuda.is_available() else None
    
    return results


@app.on_event("startup")
async def startup_event():
    """Initialize model on startup"""
    logger.info("Starting up API v4...")
    logger.info(f"GPU Manager initialized with device: {gpu_manager.device}")
    
    # Pre-load model
    try:
        await asyncio.get_event_loop().run_in_executor(None, load_model_optimized)
        logger.info("Model pre-loaded successfully")
    except Exception as e:
        logger.warning(f"Could not pre-load model: {e}")


@app.on_event("shutdown")
async def shutdown_event():
    """Enhanced cleanup on shutdown"""
    global transcriber, model, processor
    
    logger.info("Shutting down API...")
    
    # Cleanup model
    if model is not None:
        try:
            if torch.cuda.is_available():
                model.cpu()
            del model
        except Exception as e:
            logger.warning(f"Error during model cleanup: {e}")
        model = None
    
    # Cleanup processor
    if processor is not None:
        del processor
        processor = None
    
    # Cleanup transcriber
    if transcriber is not None:
        try:
            if hasattr(transcriber, 'model') and torch.cuda.is_available():
                transcriber.model.cpu()
            del transcriber
        except Exception as e:
            logger.warning(f"Error during transcriber cleanup: {e}")
        transcriber = None
    
    # GPU cleanup
    if torch.cuda.is_available():
        try:
            torch.cuda.empty_cache()
            torch.cuda.synchronize()
        except Exception as e:
            logger.warning(f"GPU cleanup error: {e}")
    
    gc.collect()
    logger.info("Shutdown complete")