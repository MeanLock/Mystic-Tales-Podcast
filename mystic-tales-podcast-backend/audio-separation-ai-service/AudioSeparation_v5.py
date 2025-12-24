import os
import uuid
import shutil
import logging
import zipfile
import subprocess
import time
import asyncio
import gc
from pathlib import Path
from typing import List, Set, Tuple, Optional, Dict, Any, Union
from concurrent.futures import ThreadPoolExecutor
from contextlib import contextmanager

from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import FileResponse

# Core frameworks with proper error handling
try:
    import torch
    import torch.cuda.amp
    TORCH_AVAILABLE = True
except Exception:
    torch = None
    TORCH_AVAILABLE = False
    
try:
    import onnxruntime as ort
    ORT_AVAILABLE = True
except Exception:
    ort = None
    ORT_AVAILABLE = False

# audio separator library
from audio_separator.separator import Separator
import imageio_ffmpeg as ffmpeg

# CUDA optimization environment setup (similar to working example)
os.environ.setdefault("OMP_NUM_THREADS", "1")
os.environ.setdefault("MKL_NUM_THREADS", "1")
os.environ.setdefault("NUMEXPR_NUM_THREADS", "1")

# CUDA/ONNX optimization environment variables
os.environ.setdefault("CUDA_LAUNCH_BLOCKING", "0")
os.environ.setdefault("TORCH_CUDNN_V8_API_ENABLED", "1")
os.environ.setdefault("CUDA_CACHE_DISABLE", "0")
os.environ.setdefault("ORT_CUDA_UNAVAILABLE", "0")
os.environ.setdefault("CUDA_MODULE_LOADING", "LAZY")

app = FastAPI(title="Audio Separator GPU API v5 - CUDA Optimized with Concurrent Processing", version="5.0")

# Setup logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s | %(levelname)s | pid=%(process)d | %(message)s")
logger = logging.getLogger(__name__)

# Paths
MODELS_DIR = "model"
OUTPUT_DIR = "outputs"
os.makedirs(OUTPUT_DIR, exist_ok=True)

MODEL_FILE = "UVR_MDXNET_Main.onnx"

# Global variables for GPU management and concurrent processing
_GPU_SEP: Optional[Separator] = None
_GPU_PROCESSING_POOL: Optional['GPUProcessingPool'] = None

# Concurrent processing configuration
MAX_CONCURRENT_SEGMENTS = 3  # Adjust based on GPU memory (8GB RTX 4060)
SEMAPHORE_LIMIT = 2  # Conservative limit to avoid OOM

# Utilities

def _ensure_ffmpeg():
    explicit_ffmpeg = os.environ.get("EXPLICIT_FFMPEG") or r"D:\ffmpeg\ffmpeg.exe"
    explicit_ffprobe = os.environ.get("EXPLICIT_FFPROBE") or r"D:\ffmpeg\ffprobe.exe"
    candidates = []
    if os.path.isfile(explicit_ffmpeg):
        candidates.append(explicit_ffmpeg)
    which_ff = shutil.which("ffmpeg")
    if which_ff:
        candidates.append(which_ff)
    try:
        candidates.append(ffmpeg.get_ffmpeg_exe())
    except Exception:
        pass
    ff = next((p for p in candidates if p and os.path.isfile(p)), None)
    if not ff:
        logging.warning("FFmpeg not found; split/merge may fail")
        return
    ffdir = os.path.dirname(ff)
    os.environ["PATH"] = ffdir + os.pathsep + os.environ.get("PATH", "")
    os.environ["FFMPEG_BINARY"] = ff
    os.environ["IMAGEIO_FFMPEG_EXE"] = ff
    if os.path.isfile(explicit_ffprobe):
        os.environ["FFPROBE_BINARY"] = explicit_ffprobe
    else:
        which_probe = shutil.which("ffprobe")
        if which_probe:
            os.environ["FFPROBE_BINARY"] = which_probe

_ensure_ffmpeg()


def has_cuda_gpu() -> bool:
    """Detect NVIDIA CUDA GPU or ONNX CUDA execution provider."""
    try:
        if torch is not None and getattr(torch, "cuda", None) and torch.cuda.is_available():
            return True
    except Exception:
        pass
    try:
        if ort is not None:
            prov = ort.get_available_providers()
            if "CUDAExecutionProvider" in prov or "TensorrtExecutionProvider" in prov:
                return True
    except Exception:
        pass
    # nvidia-smi fallback
    try:
        subprocess.run(["nvidia-smi"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=1)
        return True
    except Exception:
        pass
    return False


# Import GPUManager
from GPUManager import GPUManager

# Initialize GPU Manager
gpu_manager = GPUManager()

# GPU Processing Pool for concurrent segment processing
class GPUProcessingPool:
    """Pool of Separator instances for concurrent processing"""
    
    def __init__(self, pool_size: int = MAX_CONCURRENT_SEGMENTS):
        self.pool_size = pool_size
        self.separators: List[Separator] = []
        self.in_use: List[bool] = []
        self.semaphore = asyncio.Semaphore(SEMAPHORE_LIMIT)
        self._lock = asyncio.Lock()
        
    async def get_separator(self) -> Tuple[int, Separator]:
        """Get an available Separator instance"""
        async with self._lock:
            # Find available separator
            for i, in_use in enumerate(self.in_use):
                if not in_use:
                    self.in_use[i] = True
                    return i, self.separators[i]
            
            # Create new separator if pool not full
            if len(self.separators) < self.pool_size:
                idx = len(self.separators)
                logger.info(f"Creating new optimized Separator instance {idx} for concurrent processing")
                
                # Check GPU memory before creating new instance
                if not gpu_manager.check_memory_usage():
                    logger.warning("High GPU memory usage detected, cleaning up...")
                    with gpu_manager.gpu_memory_cleanup():
                        pass
                
                # Create separator with GPU optimization similar to working example
                with gpu_manager.gpu_memory_cleanup():
                    if TORCH_AVAILABLE and torch.cuda.is_available():
                        with torch.no_grad():
                            separator = Separator(
                                model_file_dir=MODELS_DIR,
                                output_dir=OUTPUT_DIR,
                                output_format="wav",
                                log_level=logging.INFO,
                                use_autocast=True,
                            )
                            
                            # Set ONNX providers if available
                            if ORT_AVAILABLE and hasattr(separator, 'onnx_execution_providers'):
                                try:
                                    available_providers = ort.get_available_providers()
                                    preferred_providers = []
                                    if "TensorrtExecutionProvider" in available_providers:
                                        preferred_providers.append("TensorrtExecutionProvider")
                                    if "CUDAExecutionProvider" in available_providers:
                                        preferred_providers.append("CUDAExecutionProvider")
                                    preferred_providers.append("CPUExecutionProvider")
                                    separator.onnx_execution_providers = preferred_providers
                                    logger.info(f"Set ONNX providers for instance {idx}: {preferred_providers}")
                                except Exception as e:
                                    logger.warning(f"Could not set ONNX providers: {e}")
                            
                            separator.load_model(model_filename=MODEL_FILE)
                    else:
                        raise Exception("CUDA not available for concurrent processing")
                
                self.separators.append(separator)
                self.in_use.append(True)
                
                memory_info = gpu_manager.get_memory_info()
                logger.info(f"Separator instance {idx} created. GPU Memory: {memory_info['allocated_memory_gb']:.2f}GB / {memory_info['total_memory_gb']:.2f}GB")
                
                return idx, separator
            
            # All instances busy, wait for available one
            logger.warning("All separator instances busy, waiting...")
            await asyncio.sleep(0.1)
            return await self.get_separator()
    
    async def release_separator(self, idx: int):
        """Release a Separator instance back to pool"""
        async with self._lock:
            if 0 <= idx < len(self.in_use):
                self.in_use[idx] = False
                
                # Cleanup GPU memory after use
                if TORCH_AVAILABLE and torch.cuda.is_available():
                    with gpu_manager.gpu_memory_cleanup():
                        pass


@app.get("/gpu/health")
def gpu_health():
    """Enhanced GPU health check similar to working example"""
    cuda = False
    torch_cuda = False
    device_name = None
    providers = []
    onnx_version = None
    cuda_version = None
    recommendations = []
    
    try:
        if TORCH_AVAILABLE and torch.cuda.is_available():
            torch_cuda = True
            cuda = True
            try:
                device_name = torch.cuda.get_device_name(0)
                cuda_version = torch.version.cuda
            except Exception:
                device_name = None
    except Exception:
        pass
        
    try:
        if ORT_AVAILABLE:
            providers = ort.get_available_providers()
            onnx_version = ort.__version__
            if "CUDAExecutionProvider" in providers or "TensorrtExecutionProvider" in providers:
                cuda = True
            else:
                recommendations.append("Install onnxruntime-gpu and ensure CUDA/cuDNN are properly installed")
    except Exception:
        recommendations.append("ONNX Runtime not available - install onnxruntime-gpu")
    
    # Add GPU Manager info
    memory_info = gpu_manager.get_memory_info() if TORCH_AVAILABLE and torch.cuda.is_available() else None
    
    # GPU Processing Pool status
    pool_status = None
    if _GPU_PROCESSING_POOL:
        pool_status = {
            "pool_size": _GPU_PROCESSING_POOL.pool_size,
            "active_instances": len(_GPU_PROCESSING_POOL.separators),
            "busy_instances": sum(_GPU_PROCESSING_POOL.in_use),
            "semaphore_limit": SEMAPHORE_LIMIT
        }
    
    result = {
        "cudaAvailable": cuda,
        "torchCuda": torch_cuda,
        "deviceName": device_name,
        "onnxProviders": providers,
        "onnxVersion": onnx_version,
        "cudaVersion": cuda_version,
        "device": gpu_manager.device,
        "memoryInfo": memory_info,
        "poolStatus": pool_status,
        "recommendations": recommendations
    }
    
    return result


def get_gpu_sep() -> Separator:
    """Create/load a single Separator instance strictly on CUDA GPU (for backward compatibility).
    Raises if CUDA is not available or the model cannot be loaded on GPU.
    """
    global _GPU_SEP
    if _GPU_SEP is None:
        logger.info("Loading primary model on CUDA GPU (v5 with concurrent processing)")
        
        # Check GPU memory before loading
        if not gpu_manager.check_memory_usage():
            logger.warning("High GPU memory usage detected, cleaning up...")
            with gpu_manager.gpu_memory_cleanup():
                pass
        
        try:
            # Set CUDA environment using GPUManager device info (similar to working example)
            device_info = gpu_manager.device
            if "cuda" in device_info:
                device_num = device_info.split(":")[-1] if ":" in device_info else "0"
                os.environ["CUDA_VISIBLE_DEVICES"] = device_num
            
            # Load model with GPU optimization similar to working example
            with gpu_manager.gpu_memory_cleanup():
                if TORCH_AVAILABLE and torch.cuda.is_available():
                    # Use torch.no_grad() to save memory during model loading
                    with torch.no_grad():
                        _GPU_SEP = Separator(
                            model_file_dir=MODELS_DIR,
                            output_dir=OUTPUT_DIR,
                            output_format="wav",
                            log_level=logging.INFO,
                            use_autocast=True,
                        )
                        
                        # Set ONNX providers if available (similar to working example)
                        if ORT_AVAILABLE and hasattr(_GPU_SEP, 'onnx_execution_providers'):
                            try:
                                available_providers = ort.get_available_providers()
                                preferred_providers = []
                                if "TensorrtExecutionProvider" in available_providers:
                                    preferred_providers.append("TensorrtExecutionProvider")
                                if "CUDAExecutionProvider" in available_providers:
                                    preferred_providers.append("CUDAExecutionProvider")
                                preferred_providers.append("CPUExecutionProvider")
                                _GPU_SEP.onnx_execution_providers = preferred_providers
                                logger.info(f"Set ONNX providers: {preferred_providers}")
                            except Exception as e:
                                logger.warning(f"Could not set ONNX providers: {e}")
                        
                        _GPU_SEP.load_model(model_filename=MODEL_FILE)
                else:
                    raise Exception("CUDA not available")
                    
            memory_info = gpu_manager.get_memory_info()
            logger.info(f"Model loaded on CUDA successfully. Device: {gpu_manager.device}")
            logger.info(f"GPU Memory: {memory_info['allocated_memory_gb']:.2f}GB / {memory_info['total_memory_gb']:.2f}GB")
            
        except Exception as e:
            logger.exception("Failed to initialize Separator on CUDA")
            raise HTTPException(status_code=500, detail=f"Failed to load model on CUDA: {e}")
    return _GPU_SEP

def get_gpu_processing_pool() -> 'GPUProcessingPool':
    """Get or create GPU processing pool for concurrent segment processing"""
    global _GPU_PROCESSING_POOL
    if _GPU_PROCESSING_POOL is None:
        logger.info(f"Initializing GPU Processing Pool (max_workers={MAX_CONCURRENT_SEGMENTS}, semaphore={SEMAPHORE_LIMIT})")
        _GPU_PROCESSING_POOL = GPUProcessingPool(MAX_CONCURRENT_SEGMENTS)
    return _GPU_PROCESSING_POOL


# Reuse some helpers from v3

def get_audio_duration(file_path: str) -> float:
    try:
        ffprobe = os.environ.get("FFPROBE_BINARY") or ffmpeg.get_ffmpeg_exe().replace("ffmpeg.exe", "ffprobe.exe")
        cmd = [ffprobe, "-v", "quiet", "-show_entries", "format=duration", "-of", "csv=p=0", file_path]
        out = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        if out.returncode == 0:
            return float(out.stdout.strip())
    except Exception as e:
        logging.warning(f"ffprobe failed: {e}")
    return 0.0


def compute_segment_time(duration_seconds: int) -> Tuple[bool, int, int]:
    try:
        d = int(duration_seconds)
    except Exception:
        d = 0
    if d <= 240:
        return False, 0, 0
    if d <= 20 * 60:
        return True, 120, 3
    return True, 180, 3


def split_into_segments(input_path: str, duration_seconds: int, segment_seconds: int, overlap_seconds: int, req_id: str) -> List[Tuple[int, str, float, float]]:
    seg_dir = os.path.abspath(os.path.join(OUTPUT_DIR, f"segments_{req_id}"))
    os.makedirs(seg_dir, exist_ok=True)
    step = max(1, segment_seconds - overlap_seconds)
    segments: List[Tuple[int, str, float, float]] = []
    start = 0
    idx = 0
    ffexe = ffmpeg.get_ffmpeg_exe()
    while start < duration_seconds:
        end = min(start + segment_seconds, duration_seconds)
        t = end - start
        out_path = os.path.join(seg_dir, f"seg_{idx:03d}.wav")
        cmd = [ffexe, "-hide_banner", "-loglevel", "error", "-ss", str(start), "-i", input_path, "-t", str(t), "-acodec", "pcm_s16le", "-ar", "44100", "-ac", "2", "-threads", "1", "-y", out_path]
        try:
            subprocess.run(cmd, check=True)
            segments.append((idx, out_path, float(start), float(end)))
        except subprocess.CalledProcessError as e:
            logging.error(f"Failed to cut segment {idx}: {e}")
        idx += 1
        start += step
    return segments


def merge_audio_pairwise(segments: List[str], output_path: str, overlap_seconds: int) -> Optional[str]:
    if not segments:
        return None
    if len(segments) == 1:
        try:
            shutil.copyfile(segments[0], output_path)
            return output_path
        except Exception as e:
            logging.error(f"Copy failed: {e}")
            return None
    ffexe = ffmpeg.get_ffmpeg_exe()
    base = segments[0]
    for i in range(1, len(segments)):
        nxt = segments[i]
        tmp_out = output_path + f".tmp_{i}.wav"
        try:
            cmd = [ffexe, "-hide_banner", "-loglevel", "error", "-i", base, "-i", nxt, "-filter_complex", f"[0:a][1:a]acrossfade=d={overlap_seconds}:c1=tri:c2=tri", "-c:a", "pcm_s16le", "-threads", "1", "-y", tmp_out]
            subprocess.run(cmd, check=True)
            if base != segments[0] and os.path.exists(base):
                try:
                    os.remove(base)
                except Exception:
                    pass
            base = tmp_out
        except subprocess.CalledProcessError as e:
            logging.error(f"Acrossfade failed: {e}")
            try:
                concat_file = output_path + ".concat.txt"
                with open(concat_file, "w", encoding="utf-8") as cf:
                    cf.write(f"file '{base}'\n")
                    cf.write(f"file '{nxt}'\n")
                    for rest in segments[i+1:]:
                        cf.write(f"file '{rest}'\n")
                cmd2 = [ffexe, "-hide_banner", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", concat_file, "-c", "copy", "-y", output_path]
                subprocess.run(cmd2, check=True)
                try:
                    os.remove(concat_file)
                except Exception:
                    pass
                base = output_path
                break
            except Exception as e2:
                logging.error(f"Concat fallback failed: {e2}")
                return None
    if base != output_path:
        try:
            shutil.move(base, output_path)
        except Exception as e:
            logging.error(f"Final move failed: {e}")
            return None
    return output_path
# Helpers missing in original v4
def list_files_recursive(root: str) -> Set[str]:
    files: Set[str] = set()
    root = os.path.abspath(root)
    for dirpath, _, filenames in os.walk(root):
        for fn in filenames:
            try:
                files.add(os.path.abspath(os.path.join(dirpath, fn)))
            except Exception:
                pass
    return files


def zip_files(files: List[str], zip_path: str, arcname_resolver=None) -> str:
    os.makedirs(os.path.dirname(zip_path), exist_ok=True)
    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        for p in files:
            if not p or not os.path.isfile(p):
                continue
            arcname = arcname_resolver(p) if arcname_resolver else os.path.basename(p)
            try:
                zf.write(p, arcname)
            except Exception as e:
                logging.warning(f"Failed to add {p} to zip: {e}")
    return zip_path



# Re-encode helper
def reencode_file(input_p: str, target_fmt: str = "mp3", bitrate: str = "192k", mono_for_vocals: bool = True, sample_rate: Optional[int] = None) -> str:
    ffexe = ffmpeg.get_ffmpeg_exe()
    p = Path(input_p)
    out_ext = ".mp3" if target_fmt == "mp3" else ".flac"
    out_path = str(p.with_suffix(out_ext))
    cmd = [ffexe, "-hide_banner", "-loglevel", "error", "-i", str(input_p), "-threads", "1"]
    if mono_for_vocals and "(vocals)" in p.name.lower():
        cmd += ["-ac", "1"]
    if sample_rate:
        cmd += ["-ar", str(sample_rate)]
    if target_fmt == "mp3":
        cmd += ["-c:a", "libmp3lame", "-b:a", bitrate]
    else:
        cmd += ["-c:a", "flac"]
    cmd += ["-y", out_path]
    try:
        subprocess.run(cmd, check=True)
        return out_path
    except Exception as e:
        logger.warning(f"Re-encode failed for {input_p}: {e}")
        return input_p

# Concurrent processing functions
async def process_segment_concurrent(
    pool: GPUProcessingPool,
    segment_info: Tuple[int, str, float, float]
) -> Tuple[int, List[str]]:
    """Process a single segment using GPU pool with memory optimization"""
    idx, seg_path, start, end = segment_info
    
    # Use semaphore to limit concurrent operations
    async with pool.semaphore:
        # Get separator instance from pool
        sep_idx, separator = await pool.get_separator()
        
        try:
            logger.info(f"GPU processing segment {idx} [{start:.1f}-{end:.1f}s] using instance {sep_idx}")
            
            # Process with GPU optimization (similar to working example)
            with gpu_manager.gpu_memory_cleanup():
                if TORCH_AVAILABLE and torch.cuda.is_available():
                    with torch.no_grad():
                        before = set(list_files_recursive(OUTPUT_DIR))
                        await asyncio.get_event_loop().run_in_executor(
                            None, separator.separate, seg_path
                        )
                        after = set(list_files_recursive(OUTPUT_DIR))
                else:
                    before = set(list_files_recursive(OUTPUT_DIR))
                    await asyncio.get_event_loop().run_in_executor(
                        None, separator.separate, seg_path
                    )
                    after = set(list_files_recursive(OUTPUT_DIR))
            
            created_files = [p for p in (after - before) if os.path.isfile(p) and os.path.abspath(p) != seg_path]
            
            memory_info = gpu_manager.get_memory_info()
            logger.info(f"Segment {idx} completed using instance {sep_idx}. GPU Memory: {memory_info['allocated_memory_gb']:.2f}GB / {memory_info['total_memory_gb']:.2f}GB")
            
            return idx, created_files
            
        except Exception as e:
            logger.error(f"Error processing segment {idx}: {e}")
            raise
        finally:
            # Release separator back to pool
            await pool.release_separator(sep_idx)

async def process_segments_concurrent(segments: List[Tuple[int, str, float, float]]) -> Tuple[List[str], List[str]]:
    """Process all segments concurrently using GPU pool"""
    pool = get_gpu_processing_pool()
    
    logger.info(f"Starting concurrent processing of {len(segments)} segments (max_concurrent={SEMAPHORE_LIMIT})")
    
    # Create tasks for concurrent processing
    tasks = [
        process_segment_concurrent(pool, segment_info)
        for segment_info in segments
    ]
    
    # Execute all tasks concurrently
    results = await asyncio.gather(*tasks, return_exceptions=True)
    
    # Process results
    vocal_parts: List[Tuple[str, float, float]] = []
    instr_parts: List[Tuple[str, float, float]] = []
    
    for i, result in enumerate(results):
        if isinstance(result, Exception):
            logger.error(f"Segment {i} failed: {result}")
            continue
            
        idx, created_files = result
        _, _, start, end = segments[idx]
        
        for p in created_files:
            low = Path(p).name.lower()
            if "vocals" in low:
                vocal_parts.append((p, start, end))
            elif "instrumental" in low or "music" in low:
                instr_parts.append((p, start, end))
    
    # Sort by start time and extract paths
    vocal_parts.sort(key=lambda x: x[1])
    instr_parts.sort(key=lambda x: x[1])
    
    vocal_paths = [p for p, _, _ in vocal_parts]
    instr_paths = [p for p, _, _ in instr_parts]
    
    logger.info(f"Concurrent processing completed. Vocal parts: {len(vocal_paths)}, Instrumental parts: {len(instr_paths)}")
    
    return vocal_paths, instr_paths


@app.post("/separate")
async def separate(
    file: UploadFile = File(...),
):
    """GPU-only: requires CUDA. Uses concurrent processing for segments to maximize GPU utilization.
    Short files are processed in single pass, long files are chunked and processed concurrently.
    """
    input_path = None
    seg_dir = None
    try:
        req_id = uuid.uuid4().hex
        saved_name = f"{req_id}_{file.filename}"
        content = await file.read()
        input_path = os.path.abspath(os.path.join(OUTPUT_DIR, saved_name))
        with open(input_path, "wb") as f:
            f.write(content)

        # Detect duration automatically
        duration = int(get_audio_duration(input_path))
        should_segment, seg_seconds, ovl_seconds = compute_segment_time(duration)

        created: List[str] = []

        gpu = has_cuda_gpu()
        logger.info(f"GPU available: {gpu}")
        if not gpu:
            raise HTTPException(status_code=400, detail="CUDA GPU not detected. v5 requires CUDA. Please ensure an NVIDIA CUDA-capable GPU and drivers are installed.")

        if not should_segment:
            logger.info("Single-pass on GPU (short audio)")
            # Use primary separator for single pass
            sep = get_gpu_sep()
            
            # GPU-optimized processing with memory management (similar to working example)
            with gpu_manager.gpu_memory_cleanup():
                if TORCH_AVAILABLE and torch.cuda.is_available():
                    with torch.no_grad():
                        before = set(list_files_recursive(OUTPUT_DIR))
                        _ = sep.separate(input_path)
                        after = set(list_files_recursive(OUTPUT_DIR))
                else:
                    before = set(list_files_recursive(OUTPUT_DIR))
                    _ = sep.separate(input_path)
                    after = set(list_files_recursive(OUTPUT_DIR))
            created = [p for p in (after - before) if os.path.isfile(p) and os.path.abspath(p) != input_path]
            segs = []
            processing_mode = "gpu-single"
        else:
            # Concurrent processing for long audio
            segs = split_into_segments(input_path, duration, seg_seconds, ovl_seconds, req_id)
            seg_dir = os.path.dirname(segs[0][1]) if segs else None
            if not segs:
                raise HTTPException(status_code=500, detail="Failed to create segments")
            
            logger.info(f"Concurrent GPU processing: {len(segs)} segments, max_concurrent={SEMAPHORE_LIMIT}")
            
            # Process segments concurrently using GPU pool
            vocal_paths, instr_paths = await process_segments_concurrent(segs)
            
            # Merge results
            base = os.path.join(OUTPUT_DIR, Path(saved_name).stem)
            vocal_out = merge_audio_pairwise(vocal_paths, f"{base} (Vocals).wav", ovl_seconds) if vocal_paths else None
            instr_out = merge_audio_pairwise(instr_paths, f"{base} (Instrumental).wav", ovl_seconds) if instr_paths else None
            
            if vocal_out:
                created.append(vocal_out)
            if instr_out:
                created.append(instr_out)
                
            processing_mode = "gpu-concurrent"

        if not created:
            raise HTTPException(status_code=500, detail="No output files were produced")

        # Re-encode final files to reduce size; no request params, use env/defaults
        final_format = os.environ.get("FINAL_FORMAT", "mp3").lower()
        vocal_mono = os.environ.get("VOCAL_MONO", "true").lower() in ("1", "true", "yes", "y")
        try:
            sample_rate = int(os.environ.get("SAMPLE_RATE", "44100"))
        except Exception:
            sample_rate = 44100

        final_files: List[str] = []
        for p in created:
            if not str(p).lower().endswith('.wav'):
                final_files.append(p)
                continue
            outp = reencode_file(p, target_fmt=final_format, bitrate=os.environ.get('MP3_BITRATE', '192k'), mono_for_vocals=vocal_mono, sample_rate=sample_rate)
            final_files.append(outp)
            try:
                if outp != p and os.path.exists(p):
                    os.remove(p)
            except Exception:
                pass

        zip_name = f"{Path(saved_name).stem}_stems.zip"
        zip_path = os.path.abspath(os.path.join(OUTPUT_DIR, zip_name))

        def resolve_name(p: str) -> str:
            name_lower = Path(p).name.lower()
            ext = Path(p).suffix
            base = f"{Path(saved_name).stem}"
            if "(vocals)" in name_lower:
                return f"{base}_Voice{ext}"
            if "(instrumental)" in name_lower:
                return f"{base}_BackgroundSound{ext}"
            return os.path.basename(p)

        zip_files(final_files, zip_path, arcname_resolver=resolve_name)

        # Enhanced headers with concurrent processing info
        headers = {
            "X-GPU": "True",
            "X-Mode": processing_mode,
            "X-Workers": str(SEMAPHORE_LIMIT if should_segment else 1),
            "X-Segments": str(len(segs) if should_segment else 1),
            "X-Pool-Size": str(MAX_CONCURRENT_SEGMENTS),
            "X-Concurrent": "True" if should_segment else "False",
        }
        return FileResponse(zip_path, media_type="application/zip", filename=zip_name, headers=headers)
    except HTTPException:
        raise
    except Exception as e:
        logging.exception("Separation failed")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        try:
            if input_path and os.path.exists(input_path):
                os.remove(input_path)
        except Exception:
            pass
        if seg_dir and os.path.isdir(seg_dir):
            try:
                shutil.rmtree(seg_dir, ignore_errors=True)
            except Exception:
                pass


# Additional endpoints similar to working example

@app.get("/gpu-info")
async def gpu_info():
    """Comprehensive GPU information similar to working example"""
    base_info = {
        "cuda_available": TORCH_AVAILABLE and torch.cuda.is_available(),
        "cuda_version": torch.version.cuda if TORCH_AVAILABLE else None,
        "torch_version": torch.__version__ if TORCH_AVAILABLE else None,
        "onnx_version": ort.__version__ if ORT_AVAILABLE else None,
        "device": gpu_manager.device
    }
    
    if TORCH_AVAILABLE and torch.cuda.is_available():
        memory_info = gpu_manager.get_memory_info()
        base_info.update(memory_info)
        
        # Additional GPU info similar to working example
        base_info.update({
            "gpu_count": torch.cuda.device_count(),
            "current_device": torch.cuda.current_device(),
            "gpu_name": torch.cuda.get_device_name(0),
            "compute_capability": torch.cuda.get_device_capability(0),
            "is_fp16_supported": torch.cuda.is_available() and torch.cuda.get_device_capability(0)[0] >= 7
        })
        
        # Processing pool status
        if _GPU_PROCESSING_POOL:
            base_info.update({
                "pool_size": _GPU_PROCESSING_POOL.pool_size,
                "active_instances": len(_GPU_PROCESSING_POOL.separators),
                "busy_instances": sum(_GPU_PROCESSING_POOL.in_use),
                "semaphore_limit": SEMAPHORE_LIMIT,
                "max_concurrent_segments": MAX_CONCURRENT_SEGMENTS
            })
    
    return base_info

@app.get("/health")
async def health_check():
    """Enhanced health check similar to working example"""
    model_info = "Not loaded"
    if _GPU_SEP is not None:
        try:
            model_info = f"Primary separator loaded ({MODEL_FILE})"
        except:
            model_info = "Primary separator loaded (unknown model)"
    
    pool_info = "Not initialized"
    if _GPU_PROCESSING_POOL is not None:
        pool_info = f"Pool initialized ({len(_GPU_PROCESSING_POOL.separators)} instances)"
    
    memory_info = gpu_manager.get_memory_info() if TORCH_AVAILABLE and torch.cuda.is_available() else None
    
    return {
        "status": "healthy",
        "version": "v5-cuda-concurrent-optimized",
        "primary_model_loaded": _GPU_SEP is not None,
        "processing_pool": pool_info,
        "model_info": model_info,
        "device": gpu_manager.device,
        "cuda_available": TORCH_AVAILABLE and torch.cuda.is_available(),
        "onnx_available": ORT_AVAILABLE,
        "memory_info": memory_info,
        "concurrent_processing": {
            "max_segments": MAX_CONCURRENT_SEGMENTS,
            "semaphore_limit": SEMAPHORE_LIMIT,
            "enabled": True
        },
        "supported_formats": [".wav", ".mp3", ".m4a", ".flac", ".ogg", ".aac"],
        "output_dir": OUTPUT_DIR,
        "models_dir": MODELS_DIR
    }

@app.get("/model-info")
async def model_info():
    """Model information endpoint similar to working example"""
    if _GPU_SEP is None:
        return {"error": "Primary model not loaded"}
    
    try:
        info = {
            "primary_model": {
                "loaded": True,
                "model_file": MODEL_FILE,
                "device": gpu_manager.device,
                "memory_info": gpu_manager.get_memory_info() if TORCH_AVAILABLE and torch.cuda.is_available() else None
            }
        }
        
        # Pool information
        if _GPU_PROCESSING_POOL:
            info["processing_pool"] = {
                "pool_size": _GPU_PROCESSING_POOL.pool_size,
                "active_instances": len(_GPU_PROCESSING_POOL.separators),
                "busy_instances": sum(_GPU_PROCESSING_POOL.in_use),
                "semaphore_limit": SEMAPHORE_LIMIT
            }
        
        # ONNX provider information
        if ORT_AVAILABLE:
            info["onnx_providers"] = ort.get_available_providers()
        
        return info
    except Exception as e:
        return {"error": f"Could not get model info: {str(e)}"}

@app.get("/benchmark")
async def benchmark():
    """Simple benchmark to test GPU performance (similar to working example)"""
    if not has_cuda_gpu():
        return {"error": "CUDA GPU not available"}
    
    # Create dummy short audio for benchmark
    import numpy as np
    import soundfile as sf
    
    dummy_audio = np.random.randn(16000 * 5).astype(np.float32)  # 5 seconds
    temp_path = os.path.join(OUTPUT_DIR, "benchmark_test.wav")
    
    try:
        sf.write(temp_path, dummy_audio, 16000)
        
        start_time = time.time()
        
        # Use primary separator for benchmark
        sep = get_gpu_sep()
        with gpu_manager.gpu_memory_cleanup():
            if TORCH_AVAILABLE and torch.cuda.is_available():
                with torch.no_grad():
                    before = set(list_files_recursive(OUTPUT_DIR))
                    _ = sep.separate(temp_path)
                    after = set(list_files_recursive(OUTPUT_DIR))
            else:
                before = set(list_files_recursive(OUTPUT_DIR))
                _ = sep.separate(temp_path)
                after = set(list_files_recursive(OUTPUT_DIR))
        
        end_time = time.time()
        processing_time = end_time - start_time
        
        # Clean up benchmark files
        created_files = [p for p in (after - before) if os.path.isfile(p) and os.path.abspath(p) != temp_path]
        for f in created_files:
            try:
                os.remove(f)
            except:
                pass
        
        return {
            "processing_time_seconds": processing_time,
            "audio_duration_seconds": 5.0,
            "real_time_factor": processing_time / 5.0,
            "device": gpu_manager.device,
            "memory_info": gpu_manager.get_memory_info() if TORCH_AVAILABLE and torch.cuda.is_available() else None,
            "performance_rating": "excellent" if processing_time < 2.0 else "good" if processing_time < 5.0 else "acceptable"
        }
        
    finally:
        if os.path.exists(temp_path):
            try:
                os.remove(temp_path)
            except:
                pass

# Startup and shutdown events similar to working example
@app.on_event("startup")
async def startup_event():
    """Initialize system on startup"""
    logger.info("Starting Audio Separation API v5 with concurrent processing...")
    logger.info(f"GPU Manager initialized with device: {gpu_manager.device}")
    logger.info(f"Concurrent processing: max_segments={MAX_CONCURRENT_SEGMENTS}, semaphore={SEMAPHORE_LIMIT}")
    
    # Pre-load primary model for better first request performance
    try:
        await asyncio.get_event_loop().run_in_executor(None, get_gpu_sep)
        logger.info("Primary model pre-loaded successfully")
    except Exception as e:
        logger.warning(f"Could not pre-load primary model: {e}")

@app.on_event("shutdown")
async def shutdown_event():
    """Enhanced cleanup on shutdown similar to working example"""
    global _GPU_SEP, _GPU_PROCESSING_POOL
    
    logger.info("Shutting down Audio Separation API v5...")
    
    # Clean up processing pool
    if _GPU_PROCESSING_POOL is not None:
        try:
            # Clean up all separator instances in pool
            for separator in _GPU_PROCESSING_POOL.separators:
                try:
                    if hasattr(separator, 'model') and TORCH_AVAILABLE and torch.cuda.is_available():
                        # Move models to CPU before deletion
                        pass  # audio_separator handles this internally
                    del separator
                except Exception as e:
                    logger.warning(f"Error during pool separator cleanup: {e}")
            del _GPU_PROCESSING_POOL
        except Exception as e:
            logger.warning(f"Error during processing pool cleanup: {e}")
        _GPU_PROCESSING_POOL = None
    
    # Clean up primary separator
    if _GPU_SEP is not None:
        try:
            del _GPU_SEP
        except Exception as e:
            logger.warning(f"Error during primary separator cleanup: {e}")
        _GPU_SEP = None
    
    # Comprehensive GPU cleanup similar to working example
    if TORCH_AVAILABLE and torch.cuda.is_available():
        try:
            torch.cuda.empty_cache()
            torch.cuda.synchronize()  # Wait for all operations to complete
        except Exception as e:
            logger.warning(f"GPU cleanup error: {e}")
    
    gc.collect()
    
    # Clean temp files
    try:
        temp_files = [f for f in os.listdir(OUTPUT_DIR) if f.startswith("benchmark") or "temp" in f]
        for temp_file in temp_files:
            try:
                os.remove(os.path.join(OUTPUT_DIR, temp_file))
            except:
                pass
        logger.info("Cleaned up temporary files")
    except Exception as e:
        logger.warning(f"Could not clean up temp files: {e}")
