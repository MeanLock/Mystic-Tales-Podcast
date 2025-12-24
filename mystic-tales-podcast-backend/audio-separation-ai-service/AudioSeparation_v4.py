import os
import uuid
import shutil
import logging
import zipfile
import subprocess
import time
from pathlib import Path
from typing import List, Set, Tuple, Optional

from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import FileResponse

# try to detect frameworks
try:
    import torch
    import gc
except Exception:
    torch = None
    gc = None
try:
    import onnxruntime as ort
except Exception:
    ort = None

# audio separator library (same as v3)
from audio_separator.separator import Separator
import imageio_ffmpeg as ffmpeg

# Limit OpenMP/MKL threads early
os.environ.setdefault("OMP_NUM_THREADS", "1")
os.environ.setdefault("MKL_NUM_THREADS", "1")
os.environ.setdefault("NUMEXPR_NUM_THREADS", "1")

app = FastAPI(title="Audio Separator GPU API (v4)", version="1.0")

# Logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s | %(levelname)s | pid=%(process)d | %(message)s")

# Paths
MODELS_DIR = "model"
OUTPUT_DIR = "outputs"
os.makedirs(OUTPUT_DIR, exist_ok=True)

MODEL_FILE = "UVR_MDXNET_Main.onnx"

# Global variables for GPU management
_GPU_SEP: Optional[Separator] = None

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


@app.get("/gpu/health")
def gpu_health():
    cuda = False
    torch_cuda = False
    device_name = None
    providers = []
    try:
        if torch is not None and getattr(torch, "cuda", None):
            torch_cuda = torch.cuda.is_available()
            if torch_cuda:
                cuda = True
                try:
                    device_name = torch.cuda.get_device_name(0)
                except Exception:
                    device_name = None
    except Exception:
        pass
    try:
        if ort is not None:
            providers = ort.get_available_providers()
            if "CUDAExecutionProvider" in providers or "TensorrtExecutionProvider" in providers:
                cuda = True
    except Exception:
        pass
    
    # Add GPU Manager info
    memory_info = gpu_manager.get_memory_info() if torch.cuda.is_available() else None
    
    return {
        "cudaAvailable": cuda,
        "torchCuda": torch_cuda,
        "deviceName": device_name,
        "onnxProviders": providers,
        "device": gpu_manager.device,
        "memoryInfo": memory_info,
    }


def get_gpu_sep() -> Separator:
    """Create/load a single Separator instance strictly on CUDA GPU.
    Raises if CUDA is not available or the model cannot be loaded on GPU.
    """
    global _GPU_SEP
    if _GPU_SEP is None:
        logging.info("Loading model on CUDA GPU (v4 is GPU-only)")
        
        # Check GPU memory before loading
        if not gpu_manager.check_memory_usage():
            logging.warning("High GPU memory usage detected, cleaning up...")
            with gpu_manager.gpu_memory_cleanup():
                pass
        
        try:
            # Set CUDA environment using GPUManager device info
            device_info = gpu_manager.device
            if "cuda" in device_info:
                device_num = device_info.split(":")[-1] if ":" in device_info else "0"
                os.environ["CUDA_VISIBLE_DEVICES"] = device_num
            
            # Load model with GPU optimization similar to working example
            with gpu_manager.gpu_memory_cleanup():
                if torch is not None and torch.cuda.is_available():
                    # Use torch.no_grad() to save memory during model loading
                    with torch.no_grad():
                        _GPU_SEP = Separator(
                            model_file_dir=MODELS_DIR,
                            output_dir=OUTPUT_DIR,
                            output_format="wav",
                            log_level=logging.INFO,
                            use_autocast=True,
                            # Remove device parameter - not supported by audio_separator
                        )
                        _GPU_SEP.load_model(model_filename=MODEL_FILE)
                else:
                    raise Exception("CUDA not available")
                    
            memory_info = gpu_manager.get_memory_info()
            logging.info(f"Model loaded on CUDA successfully. Device: {gpu_manager.device}")
            logging.info(f"GPU Memory: {memory_info['allocated_memory_gb']:.2f}GB / {memory_info['total_memory_gb']:.2f}GB")
            
        except Exception as e:
            logging.exception("Failed to initialize Separator on CUDA")
            raise HTTPException(status_code=500, detail=f"Failed to load model on CUDA: {e}")
    return _GPU_SEP


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
        logging.warning(f"Re-encode failed for {input_p}: {e}")
        return input_p


@app.post("/separate")
async def separate(
    file: UploadFile = File(...),
):
    """GPU-only: requires CUDA. If no CUDA GPU, returns 400 with explanation.
    The uploaded file is processed on a single GPU instance. Long files are chunked
    and processed sequentially; short files are processed in a single pass.
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
        logging.info(f"GPU available: {gpu}")
        if not gpu:
            raise HTTPException(status_code=400, detail="CUDA GPU not detected. v4 is GPU-only. Please ensure an NVIDIA CUDA-capable GPU and drivers are installed.")

        # Load GPU model once
        sep = get_gpu_sep()
        if not should_segment:
            logging.info("Single-pass on GPU")
            # GPU-optimized processing with memory management
            with gpu_manager.gpu_memory_cleanup():
                if torch is not None and torch.cuda.is_available():
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
        else:
            # chunk then feed sequentially to same GPU model instance
            segs = split_into_segments(input_path, duration, seg_seconds, ovl_seconds, req_id)
            seg_dir = os.path.dirname(segs[0][1]) if segs else None
            if not segs:
                raise HTTPException(status_code=500, detail="Failed to create segments")
            logging.info(f"GPU path: processing {len(segs)} chunks sequentially on one GPU instance")
            vocal_parts: List[Tuple[str, float, float]] = []
            instr_parts: List[Tuple[str, float, float]] = []
            for idx, seg_path, start, end in segs:
                logging.info(f"GPU processing chunk {idx} [{start}-{end}]")
                
                # GPU-optimized chunk processing with memory management
                with gpu_manager.gpu_memory_cleanup():
                    if torch is not None and torch.cuda.is_available():
                        with torch.no_grad():
                            before = set(list_files_recursive(OUTPUT_DIR))
                            _ = sep.separate(seg_path)
                            after = set(list_files_recursive(OUTPUT_DIR))
                    else:
                        before = set(list_files_recursive(OUTPUT_DIR))
                        _ = sep.separate(seg_path)
                        after = set(list_files_recursive(OUTPUT_DIR))
                        
                created_now = [p for p in (after - before) if os.path.isfile(p) and os.path.abspath(p) != seg_path]
                for p in created_now:
                    low = Path(p).name.lower()
                    if "vocals" in low:
                        vocal_parts.append((p, start, end))
                    elif "instrumental" in low or "music" in low:
                        instr_parts.append((p, start, end))
            # merge
            vocal_parts.sort(key=lambda x: x[1])
            instr_parts.sort(key=lambda x: x[1])
            vocal_paths = [p for p, _, _ in vocal_parts]
            instr_paths = [p for p, _, _ in instr_parts]
            base = os.path.join(OUTPUT_DIR, Path(saved_name).stem)
            vocal_out = merge_audio_pairwise(vocal_paths, f"{base} (Vocals).wav", ovl_seconds) if vocal_paths else None
            instr_out = merge_audio_pairwise(instr_paths, f"{base} (Instrumental).wav", ovl_seconds) if instr_paths else None
            if vocal_out:
                created.append(vocal_out)
            if instr_out:
                created.append(instr_out)

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

        headers = {
            "X-GPU": "True",
            "X-Mode": "gpu-seq-chunks" if should_segment else "gpu-single",
            "X-Workers": "1",
            "X-Segments": str(len(segs) if should_segment else 1),
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


# No CPU worker helpers in GPU-only v4
