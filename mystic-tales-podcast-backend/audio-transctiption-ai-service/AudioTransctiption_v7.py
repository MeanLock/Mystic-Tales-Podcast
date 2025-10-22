from fastapi import FastAPI, File, UploadFile, HTTPException, Query
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
from typing import Optional, Dict, Any, Union, List
import logging
from GPUManager import GPUManager
import io
import numpy as np
import time
import uuid
from collections import deque

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="PhoWhisper Transcription API v8 - Simple Lock")

# Global variables
transcriber = None
model = None
processor = None
device = None

model_config = {
    "torch_dtype": torch.float16,
    "use_safetensors": True,
    "low_cpu_mem_usage": True,
}

# Initialize GPU Manager
gpu_manager = GPUManager()

# Parallel config
PARALLEL_CONFIG = {
    "max_parallel_chunks": 2,
    "chunk_length": 30,
    "overlap": 5,
}

# ============================================
# SIMPLE LOCK QUEUE SYSTEM
# ============================================

class TranscriptionQueue:
    """
    Simple lock để đảm bảo chỉ 1 transcription chạy tại 1 thời điểm
    Prevents GPU OOM khi có multiple concurrent requests
    """
    def __init__(self):
        self.current_job_id = None
        self.current_filename = None
        self.lock = asyncio.Lock()
        self.waiting_count = 0
        
    async def acquire(self, job_id: str, filename: str):
        """
        Wait until it's this job's turn to process
        Returns True when acquired
        """
        # Increment waiting count
        self.waiting_count += 1
        wait_position = self.waiting_count
        
        logger.info(f"🔵 Job {job_id} ({filename}) - Queue position: {wait_position}")
        
        # Wait for lock
        await self.lock.acquire()
        
        # We got the lock!
        self.current_job_id = job_id
        self.current_filename = filename
        self.waiting_count -= 1
        
        logger.info(f"✅ Job {job_id} ({filename}) - Started processing (waited for lock)")
        return True
    
    def release(self, job_id: str):
        """Release lock after job completes"""
        if self.current_job_id == job_id:
            self.current_job_id = None
            self.current_filename = None
            self.lock.release()
            logger.info(f"🔓 Job {job_id} - Lock released, next job can proceed")
        else:
            logger.warning(f"⚠️ Job {job_id} tried to release lock but doesn't own it")
    
    def get_status(self) -> Dict:
        """Get current queue status"""
        return {
            "current_job": self.current_job_id,
            "current_filename": self.current_filename,
            "waiting_jobs": self.waiting_count,
            "is_busy": self.current_job_id is not None
        }

# Global queue instance
transcription_queue = TranscriptionQueue()

# ============================================
# ORIGINAL TRANSCRIPTION FUNCTIONS (unchanged)
# ============================================

def chunk_audio(audio: np.ndarray, chunk_length: int = 30, overlap: int = 5, sample_rate: int = 16000):
    """Chia audio thành chunks với overlap"""
    chunk_samples = chunk_length * sample_rate
    overlap_samples = overlap * sample_rate
    step = chunk_samples - overlap_samples
    
    chunks = []
    for start in range(0, len(audio), step):
        end = min(start + chunk_samples, len(audio))
        chunks.append(audio[start:end])
        if end >= len(audio):
            break
    
    return chunks


def load_model_optimized():
    """Load model và processor - FINAL FIXED VERSION"""
    global transcriber, model, processor, device
    
    if transcriber is not None and model is not None:
        return transcriber, model, processor
    
    device = gpu_manager.device
    logger.info(f"Loading model on device: {device}")
    
    try:
        local_model_path = "./PhoWhisper-large"
        
        if os.path.exists(local_model_path):
            logger.info(f"Loading local model from {local_model_path}")
            model_path = local_model_path
        else:
            logger.info("Loading model from Hugging Face...")
            model_path = "vinai/PhoWhisper-large"
        
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
                    
                    logger.info(f"✓ Processor type: {type(processor).__name__}")
                    logger.info(f"✓ Has feature_extractor: {hasattr(processor, 'feature_extractor')}")
                    logger.info(f"✓ Has tokenizer: {hasattr(processor, 'tokenizer')}")
                    
                    if hasattr(processor, 'feature_extractor'):
                        logger.info(f"✓ Feature extractor type: {type(processor.feature_extractor).__name__}")
                    else:
                        logger.error("✗ CRITICAL: Processor missing feature_extractor!")
                    
                    # Create pipeline
                    transcriber = pipeline(
                        "automatic-speech-recognition",
                        model=model,
                        tokenizer=processor.tokenizer,
                        feature_extractor=processor.feature_extractor,
                        torch_dtype=model_config["torch_dtype"],
                        device=device
                    )
                    
                except Exception as hf_error:
                    logger.error(f"Primary load failed: {hf_error}")
                    logger.info("Attempting fallback pipeline loading...")
                    
                    transcriber = pipeline(
                        "automatic-speech-recognition",
                        model=model_path,
                        torch_dtype=model_config["torch_dtype"],
                        device=device,
                        trust_remote_code=False
                    )
                    
                    model = transcriber.model
                    
                    logger.info("Loading processor separately to ensure completeness...")
                    processor = AutoProcessor.from_pretrained(
                        model_path,
                        local_files_only=False,
                        use_auth_token=False
                    )
                    
                    logger.info(f"✓ Fallback processor type: {type(processor).__name__}")
                    logger.info(f"✓ Fallback has feature_extractor: {hasattr(processor, 'feature_extractor')}")
                    
            else:
                logger.info("Loading on CPU...")
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model=model_path,
                    torch_dtype=torch.float32,
                    trust_remote_code=False
                )
                model = transcriber.model
                processor = AutoProcessor.from_pretrained(model_path)
        
        if not hasattr(processor, 'feature_extractor'):
            logger.error("❌ FATAL: Processor missing feature_extractor after all attempts!")
            raise ValueError("Processor is missing feature_extractor.")
        
        logger.info("✓ Model loaded successfully!")
        logger.info(f"✓ Final verification - Processor has feature_extractor: {hasattr(processor, 'feature_extractor')}")
        
        return transcriber, model, processor
        
    except Exception as e:
        logger.error(f"❌ Error loading model: {e}")
        import traceback
        logger.error(traceback.format_exc())
        raise


async def _transcribe_chunk(transcriber, chunk: np.ndarray, chunk_id: int, language: Optional[str] = None):
    """Transcribe single chunk"""
    start_time = time.time()
    
    try:
        result = transcriber(
            chunk,
            generate_kwargs={
                "language": language,
                "task": "transcribe",
                "use_cache": True,
                "num_beams": 1,
                "do_sample": False,
            }
        )
        
        processing_time = time.time() - start_time
        text = result.get('text', '') if isinstance(result, dict) else str(result)
        
        logger.info(f"Chunk {chunk_id} completed in {processing_time:.2f}s")
        
        return {
            "chunk_id": chunk_id,
            "text": text.strip(),
            "processing_time": processing_time
        }
        
    except Exception as e:
        logger.error(f"Chunk {chunk_id} failed: {e}")
        return {
            "chunk_id": chunk_id,
            "text": "",
            "processing_time": 0,
            "error": str(e)
        }


async def _transcribe_short_audio(transcriber, audio_data: Union[np.ndarray, str], language: Optional[str] = None):
    """Transcribe short audio (<30s)"""
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
        return transcriber(audio_data)


async def _transcribe_long_audio_proper(model, processor, audio_data: np.ndarray, language: Optional[str] = None):
    """Proper long-form transcription"""
    try:
        logger.info("Using proper long-form transcription with model.generate()...")
        
        if not hasattr(processor, 'feature_extractor'):
            error_msg = f"FATAL ERROR: Processor missing feature_extractor!"
            logger.error(error_msg)
            raise ValueError(error_msg)
        
        logger.info(f"Using processor.feature_extractor (type: {type(processor.feature_extractor).__name__})")
        
        input_features = processor.feature_extractor(
            audio_data, 
            sampling_rate=16000, 
            return_tensors="pt"
        ).input_features
        
        logger.info(f"✓ Input features shape: {input_features.shape}")
        
        if torch.cuda.is_available():
            input_features = input_features.to(device)
            logger.info(f"✓ Input on device: {input_features.device}")
        
        forced_decoder_ids = None
        if language:
            try:
                if hasattr(processor, 'tokenizer'):
                    lang_token = processor.tokenizer.convert_tokens_to_ids(f"<|{language}|>")
                    task_token = processor.tokenizer.convert_tokens_to_ids("<|transcribe|>")
                    forced_decoder_ids = [[1, lang_token], [2, task_token]]
                    logger.info(f"✓ Language: {language}")
            except Exception as lang_error:
                logger.warning(f"Could not set language: {lang_error}")
        
        logger.info("Generating transcription...")
        with torch.no_grad():
            predicted_ids = model.generate(
                input_features,
                forced_decoder_ids=forced_decoder_ids,
                max_new_tokens=448,
                num_beams=1,
                do_sample=False,
                return_timestamps=False,
            )
        
        logger.info(f"✓ Generated: {predicted_ids.shape}")
        
        if hasattr(processor, 'tokenizer'):
            transcription = processor.tokenizer.batch_decode(
                predicted_ids, 
                skip_special_tokens=True
            )[0]
        else:
            transcription = processor.batch_decode(
                predicted_ids, 
                skip_special_tokens=True
            )[0]
        
        logger.info(f"✓ Transcription done: {len(transcription)} chars")
        return {"text": transcription.strip()}
        
    except Exception as e:
        logger.error(f"Proper method failed: {e}")
        import traceback
        logger.error(f"Traceback:\n{traceback.format_exc()}")
        raise


async def _transcribe_parallel_chunks(transcriber, audio: np.ndarray, language: Optional[str] = None, 
                                      max_parallel: int = 2):
    """PARALLEL CHUNKING"""
    try:
        logger.info(f"Using PARALLEL chunking (max {max_parallel} chunks at once)...")
        
        chunks = chunk_audio(
            audio, 
            chunk_length=PARALLEL_CONFIG["chunk_length"],
            overlap=PARALLEL_CONFIG["overlap"]
        )
        
        total_chunks = len(chunks)
        logger.info(f"Created {total_chunks} chunks for parallel processing")
        
        all_results = []
        
        for i in range(0, total_chunks, max_parallel):
            batch_end = min(i + max_parallel, total_chunks)
            batch_chunks = chunks[i:batch_end]
            batch_size = len(batch_chunks)
            
            logger.info(f"Processing batch: chunks {i+1}-{batch_end}/{total_chunks} ({batch_size} parallel)")
            
            if torch.cuda.is_available():
                memory_before = torch.cuda.memory_allocated() / 1024**3
                logger.info(f"GPU memory before batch: {memory_before:.2f}GB")
            
            tasks = [
                _transcribe_chunk(transcriber, chunk, i + idx, language)
                for idx, chunk in enumerate(batch_chunks)
            ]
            
            batch_results = await asyncio.gather(*tasks)
            all_results.extend(batch_results)
            
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
                memory_after = torch.cuda.memory_allocated() / 1024**3
                logger.info(f"GPU memory after batch: {memory_after:.2f}GB")
            
            await asyncio.sleep(0.1)
        
        all_results.sort(key=lambda x: x["chunk_id"])
        full_transcript = ' '.join([r["text"] for r in all_results if r["text"]])
        
        total_processing_time = sum([r["processing_time"] for r in all_results])
        
        return {
            "text": full_transcript,
            "chunks_processed": total_chunks,
            "total_processing_time": total_processing_time,
            "parallel_batches": (total_chunks + max_parallel - 1) // max_parallel
        }
        
    except Exception as e:
        logger.error(f"Parallel chunking failed: {e}")
        raise


async def _transcribe_sequential_chunks(transcriber, audio: np.ndarray, language: Optional[str] = None):
    """SEQUENTIAL CHUNKING"""
    try:
        logger.info("Using SEQUENTIAL chunking...")
        
        chunks = chunk_audio(audio, chunk_length=30, overlap=5)
        logger.info(f"Processing {len(chunks)} chunks sequentially...")
        
        all_transcripts = []
        
        for i, chunk in enumerate(chunks):
            logger.info(f"Processing chunk {i+1}/{len(chunks)}")
            
            result = await _transcribe_short_audio(transcriber, chunk, language)
            text = result.get('text', '') if isinstance(result, dict) else str(result)
            all_transcripts.append(text.strip())
            
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
        
        full_transcript = ' '.join(all_transcripts)
        return {"text": full_transcript}
        
    except Exception as e:
        logger.error(f"Sequential chunking failed: {e}")
        raise


# ============================================
# MODIFIED /transcribe ENDPOINT WITH LOCK
# ============================================

@app.post("/transcribe")
async def transcribe(
    AudioFile: UploadFile = File(...), 
    language: Optional[str] = None,
    method: str = Query("auto", description="auto|proper|parallel|sequential"),
    max_parallel: int = Query(2, ge=1, le=4, description="Max parallel chunks (1-4)")
):
    """
    Transcribe audio với queue lock
    Chỉ 1 transcription chạy tại 1 thời điểm để tránh GPU OOM
    """
    
    # Generate unique job ID
    job_id = str(uuid.uuid4())[:8]
    
    # Validate file
    if not AudioFile.filename:
        raise HTTPException(status_code=400, detail="No filename")
    
    allowed_extensions = {'.wav', '.mp3', '.m4a', '.flac', '.ogg', '.aac'}
    file_ext = os.path.splitext(AudioFile.filename)[1].lower()
    if file_ext not in allowed_extensions:
        raise HTTPException(status_code=400, detail=f"Unsupported format")
    
    # Read file
    content = await AudioFile.read()
    if len(content) == 0:
        raise HTTPException(status_code=400, detail="Empty file")
    
    # ============================================
    # ACQUIRE LOCK - Wait for our turn
    # ============================================
    await transcription_queue.acquire(job_id, AudioFile.filename)
    
    try:
        # Memory check
        if not gpu_manager.check_memory_usage():
            logger.warning("High GPU memory, cleaning...")
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
        
        logger.info(f"Processing: {AudioFile.filename} ({len(content)} bytes), method={method}")
        
        # Load audio
        try:
            audio_buffer = io.BytesIO(content)
            audio, sr = librosa.load(audio_buffer, sr=16000, dtype=np.float32)
            
            if len(audio) == 0:
                raise HTTPException(status_code=400, detail="Invalid audio")
            
            audio = librosa.util.normalize(audio)
            audio_duration = len(audio) / sr
            
            logger.info(f"Audio: {audio_duration:.2f}s, GPU: {torch.cuda.is_available()}")
            
        except Exception as audio_error:
            logger.error(f"Audio processing error: {audio_error}")
            raise HTTPException(status_code=400, detail=f"Audio failed: {str(audio_error)}")
        
        # START TRANSCRIPTION
        start_time = time.time()
        
        with gpu_manager.gpu_memory_cleanup():
            try:
                with torch.no_grad():
                    # Method selection
                    if method == "auto":
                        if audio_duration > 30:
                            try:
                                result = await _transcribe_long_audio_proper(
                                    current_model, current_processor, audio, language
                                )
                                processing_method = "auto → proper (model.generate)"
                            except Exception as proper_error:
                                logger.warning(f"Proper failed, trying parallel: {proper_error}")
                                result = await _transcribe_parallel_chunks(
                                    current_transcriber, audio, language, max_parallel
                                )
                                processing_method = "auto → parallel (fallback)"
                        else:
                            result = await _transcribe_short_audio(current_transcriber, audio, language)
                            processing_method = "auto → pipeline (short)"
                    
                    elif method == "proper":
                        result = await _transcribe_long_audio_proper(
                            current_model, current_processor, audio, language
                        )
                        processing_method = "proper (model.generate - no chunks)"
                    
                    elif method == "parallel":
                        result = await _transcribe_parallel_chunks(
                            current_transcriber, audio, language, max_parallel
                        )
                        processing_method = f"parallel ({max_parallel} chunks at once)"
                    
                    elif method == "sequential":
                        result = await _transcribe_sequential_chunks(
                            current_transcriber, audio, language
                        )
                        processing_method = "sequential (1 chunk at a time)"
                    
                    else:
                        raise HTTPException(status_code=400, detail=f"Unknown method: {method}")
                    
                    # Extract text
                    if isinstance(result, dict):
                        if 'chunks' in result and result['chunks']:
                            output_text = ' '.join([c.get('text', '') for c in result['chunks']])
                        elif 'text' in result:
                            output_text = result['text']
                        else:
                            output_text = str(result)
                    else:
                        output_text = str(result)
                    
                    if not output_text.strip():
                        output_text = "No transcription"
                
            except Exception as transcribe_error:
                logger.error(f"Transcription error: {transcribe_error}")
                import traceback
                logger.error(f"Traceback: {traceback.format_exc()}")
                
                # Final fallback
                try:
                    logger.info("Final fallback...")
                    with torch.no_grad():
                        simple_result = current_transcriber(audio)
                        output_text = simple_result.get('text', 'Failed') if isinstance(simple_result, dict) else str(simple_result)
                        processing_method = "simple fallback"
                except Exception as fallback_error:
                    logger.error(f"All methods failed: {fallback_error}")
                    raise HTTPException(status_code=500, detail=f"Transcription failed: {str(transcribe_error)}")
        
        end_time = time.time()
        total_time = end_time - start_time
        
        # Response
        memory_info = gpu_manager.get_memory_info()
        
        response_data = {
            "JobID": job_id,
            "Transcript": output_text.strip(),
            "DurationSeconds": audio_duration,
            "ProcessingTimeSeconds": total_time,
            "RealTimeFactor": total_time / audio_duration if audio_duration > 0 else 0,
            "FileSizeBytes": len(content),
            "AudioShape": list(audio.shape),
            "SampleRate": sr,
            "ModelUsed": getattr(current_model, 'name_or_path', 'unknown'),
            "DeviceUsed": str(device),
            "ProcessingMethod": processing_method,
            "MemoryInfo": memory_info if torch.cuda.is_available() else None,
        }
        
        # Add extra info
        if isinstance(result, dict):
            if "chunks_processed" in result:
                response_data["ChunksProcessed"] = result["chunks_processed"]
            if "parallel_batches" in result:
                response_data["ParallelBatches"] = result["parallel_batches"]
        
        return response_data
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        import traceback
        logger.error(f"Traceback: {traceback.format_exc()}")
        raise HTTPException(status_code=500, detail=f"Internal error: {str(e)}")
    
    finally:
        # ============================================
        # RELEASE LOCK - Always release when done
        # ============================================
        transcription_queue.release(job_id)


# ============================================
# OTHER ENDPOINTS (unchanged)
# ============================================

@app.get("/queue-status")
async def queue_status():
    """Check current queue status"""
    return transcription_queue.get_status()


@app.get("/compare-methods")
async def compare_methods(duration: int = Query(60, ge=30, le=300, description="Test audio duration (30-300s)")):
    """Benchmark methods"""
    if transcriber is None or model is None:
        return {"error": "Model not loaded"}
    
    dummy_audio = np.random.randn(16000 * duration).astype(np.float32)
    logger.info(f"Benchmarking with {duration}s audio...")
    
    results = {}
    
    # Test proper method
    try:
        logger.info("Testing PROPER method...")
        start = time.time()
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = await _transcribe_long_audio_proper(model, processor, dummy_audio)
        elapsed = time.time() - start
        
        results["proper"] = {
            "method": "model.generate() - no chunks",
            "processing_time": elapsed,
            "real_time_factor": elapsed / duration,
            "chunks": 0,
            "memory_peak": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
    except Exception as e:
        results["proper"] = {"error": str(e)}
    
    if torch.cuda.is_available():
        torch.cuda.empty_cache()
        gc.collect()
    
    # Test parallel method
    try:
        logger.info("Testing PARALLEL method (2 chunks)...")
        start = time.time()
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = await _transcribe_parallel_chunks(transcriber, dummy_audio, max_parallel=2)
        elapsed = time.time() - start
        
        results["parallel_2x"] = {
            "method": "parallel chunks (2 at once)",
            "processing_time": elapsed,
            "real_time_factor": elapsed / duration,
            "chunks": result.get("chunks_processed", 0),
            "parallel_batches": result.get("parallel_batches", 0),
            "memory_peak": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
    except Exception as e:
        results["parallel_2x"] = {"error": str(e)}
    
    if torch.cuda.is_available():
        torch.cuda.empty_cache()
        gc.collect()
    
    # Test sequential method
    try:
        logger.info("Testing SEQUENTIAL method...")
        start = time.time()
        with gpu_manager.gpu_memory_cleanup():
            with torch.no_grad():
                result = await _transcribe_sequential_chunks(transcriber, dummy_audio)
        elapsed = time.time() - start
        
        chunks_count = len(chunk_audio(dummy_audio))
        
        results["sequential"] = {
            "method": "sequential chunks (1 at a time)",
            "processing_time": elapsed,
            "real_time_factor": elapsed / duration,
            "chunks": chunks_count,
            "memory_peak": gpu_manager.get_memory_info() if torch.cuda.is_available() else None
        }
    except Exception as e:
        results["sequential"] = {"error": str(e)}
    
    results["summary"] = {
        "test_duration_seconds": duration,
        "gpu_device": str(device),
    }
    
    return results


@app.get("/health")
async def health_check():
    """Health check"""
    model_info = "Not loaded"
    if transcriber is not None:
        try:
            model_info = getattr(transcriber.model, 'name_or_path', 'Loaded')
        except:
            model_info = "Loaded"
    
    memory_info = gpu_manager.get_memory_info() if torch.cuda.is_available() else None
    queue_status = transcription_queue.get_status()
    
    return {
        "status": "healthy",
        "version": "v8-simple-lock",
        "model_loaded": transcriber is not None,
        "model_info": model_info,
        "device": gpu_manager.device,
        "cuda_available": torch.cuda.is_available(),
        "memory_info": memory_info,
        "parallel_config": PARALLEL_CONFIG,
        "queue_status": queue_status,
        "supported_methods": ["auto", "proper", "parallel", "sequential"],
        "supported_formats": [".wav", ".mp3", ".m4a", ".flac", ".ogg", ".aac"]
    }


@app.get("/gpu-info")
async def gpu_info():
    """GPU info"""
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
        })
    
    return base_info


@app.on_event("startup")
async def startup_event():
    """Initialize model on startup"""
    logger.info("Starting API v8 - Simple Lock Edition...")
    logger.info(f"GPU Manager: {gpu_manager.device}")
    logger.info(f"Parallel config: {PARALLEL_CONFIG}")
    logger.info("🔒 Simple Lock Queue enabled - Only 1 transcription at a time")
    
    try:
        await asyncio.get_event_loop().run_in_executor(None, load_model_optimized)
        logger.info("Model pre-loaded successfully")
    except Exception as e:
        logger.warning(f"Could not pre-load model: {e}")


@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    global transcriber, model, processor
    
    logger.info("Shutting down...")
    
    if model is not None:
        try:
            if torch.cuda.is_available():
                model.cpu()
            del model
        except Exception as e:
            logger.warning(f"Model cleanup error: {e}")
        model = None
    
    if processor is not None:
        del processor
        processor = None
    
    if transcriber is not None:
        try:
            if hasattr(transcriber, 'model') and torch.cuda.is_available():
                transcriber.model.cpu()
            del transcriber
        except Exception as e:
            logger.warning(f"Transcriber cleanup error: {e}")
        transcriber = None
    
    if torch.cuda.is_available():
        try:
            torch.cuda.empty_cache()
            torch.cuda.synchronize()
        except Exception as e:
            logger.warning(f"GPU cleanup error: {e}")
    
    gc.collect()
    logger.info("Shutdown complete")