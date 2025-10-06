import os
# Giới hạn thread trong mỗi process để tránh oversubscription (đặt sớm)
os.environ.setdefault("OMP_NUM_THREADS", "1")
os.environ.setdefault("MKL_NUM_THREADS", "1")
os.environ.setdefault("NUMEXPR_NUM_THREADS", "1")

import uuid
import math
import shutil
import logging
import zipfile
import subprocess
from pathlib import Path
from typing import List, Set, Tuple, Optional
from concurrent.futures import ProcessPoolExecutor, as_completed
import multiprocessing as mp
from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import FileResponse
from audio_separator.separator import Separator
import imageio_ffmpeg as ffmpeg
import time

app = FastAPI(title="Audio Separator API", version="1.0")

# Ensure logs are visible with process id
if not logging.getLogger().handlers:
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s | %(levelname)s | pid=%(process)d | %(message)s"
    )

# --- FFmpeg auto-detection setup ---
def _ensure_ffmpeg():
    # Adjust these if you have a fixed install
    explicit_ffmpeg =  r"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffmpeg.exe"
    explicit_ffprobe = r"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffprobe.exe"

    candidates = []
    if os.path.isfile(explicit_ffmpeg):
        candidates.append(explicit_ffmpeg)

    # Try system PATH
    which_ff = shutil.which("ffmpeg")
    if which_ff:
        candidates.append(which_ff)

    # Try imageio-ffmpeg bundled binary
    try:
        import imageio_ffmpeg
        candidates.append(imageio_ffmpeg.get_ffmpeg_exe())
    except Exception:
        pass

    ff = next((p for p in candidates if p and os.path.isfile(p)), None)
    if not ff:
        logging.error("FFmpeg not found. Install FFmpeg or update explicit_ffmpeg path.")
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

    try:
        ver = os.popen(f'"{ff}" -version').read().splitlines()[0]
        logging.info(f"Using FFmpeg: {ff} | {ver}")
    except Exception:
        logging.info(f"Using FFmpeg: {ff}")

_ensure_ffmpeg()

# --- Model setup ---
# Trỏ đúng vào thư mục models bạn đang có
MODELS_DIR = "model"
OUTPUT_DIR = "outputs"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Đặt đúng tên model bạn có trong thư mục models
MODEL_FILE = "UVR_MDXNET_Main.onnx"

# Lazy model load (to avoid loading in every worker import)
_SEP: Optional[Separator] = None

def get_sep() -> Separator:
    global _SEP
    if _SEP is None:
        s = Separator(
            model_file_dir=MODELS_DIR,
            output_dir=OUTPUT_DIR,
            output_format="wav",
            log_level=logging.INFO,
            use_autocast=True
        )
        s.load_model(model_filename=MODEL_FILE)
        _SEP = s
        logging.info("Main process loaded model")
    return _SEP

def list_files_recursive(root: str) -> Set[str]:
    files: Set[str] = set()
    for base, _, names in os.walk(root):
        for n in names:
            files.add(os.path.abspath(os.path.join(base, n)))
    return files


def zip_files(files: List[str], zip_path: str, arcname_resolver=None) -> str:
    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
        for f in files:
            if os.path.isfile(f):
                arcname = arcname_resolver(f) if arcname_resolver else os.path.basename(f)
                zf.write(f, arcname=arcname)
    return zip_path


# --- Utilities for duration, segmentation, merging ---
def get_audio_duration(file_path: str) -> float:
    try:
        ffprobe = os.environ.get("FFPROBE_BINARY") or ffmpeg.get_ffmpeg_exe().replace("ffmpeg.exe", "ffprobe.exe")
        cmd = [ffprobe, "-v", "quiet", "-show_entries", "format=duration", "-of", "csv=p=0", file_path]
        out = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        if out.returncode == 0:
            return float(out.stdout.strip())
    except Exception as e:
        logging.warning(f"ffprobe failed to get duration: {e}")
    return 0.0


def compute_segment_time(duration_seconds: int) -> Tuple[bool, int, int]:
    """Compute segmentation strategy.
    Returns: (should_segment, segment_seconds, overlap_seconds)
    """
    try:
        d = int(duration_seconds)
    except Exception:
        d = 0
    # <= 240s: single pass
    if d <= 240:
        return False, 0, 0
    # 4–20 phút: 120s, >20 phút: 180s (overlap 3s)
    if d <= 20 * 60:
        return True, 120, 3
    return True, 180, 3


def split_into_segments(input_path: str, duration_seconds: int, segment_seconds: int, overlap_seconds: int, req_id: str) -> List[Tuple[int, str, float, float]]:
    """Use ffmpeg to cut overlapping segments as WAV files.
    Returns list of (index, segment_path, start, end)
    """
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
        cmd = [
            ffexe,
            "-hide_banner", "-loglevel", "error",
            "-ss", str(start),
            "-i", input_path,
            "-t", str(t),
            "-acodec", "pcm_s16le", "-ar", "44100", "-ac", "2",
            "-threads", "1",
            "-y", out_path
        ]
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
            cmd = [
                ffexe, "-hide_banner", "-loglevel", "error",
                "-i", base, "-i", nxt,
                "-filter_complex", f"[0:a][1:a]acrossfade=d={overlap_seconds}:c1=tri:c2=tri",
                "-c:a", "pcm_s16le",
                "-threads", "1",
                "-y", tmp_out
            ]
            subprocess.run(cmd, check=True)
            # move tmp_out to be new base
            if base != segments[0] and os.path.exists(base):
                try:
                    os.remove(base)
                except Exception:
                    pass
            base = tmp_out
        except subprocess.CalledProcessError as e:
            logging.error(f"Acrossfade failed at step {i}: {e}")
            # fallback to simple concat for remaining
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

    # final move/rename
    if base != output_path:
        try:
            shutil.move(base, output_path)
        except Exception as e:
            logging.error(f"Final move failed: {e}")
            return None
    return output_path


# Worker-side lazy model
_WSEP: Optional[Separator] = None

def _get_worker_sep() -> Separator:
    global _WSEP
    if _WSEP is None:
        s = Separator(
            model_file_dir=MODELS_DIR,
            output_dir=OUTPUT_DIR,
            output_format="wav",
            log_level=logging.WARNING,
            use_autocast=True
        )
        s.load_model(model_filename=MODEL_FILE)
        _WSEP = s
        logging.info(f"Worker {os.getpid()} loaded model")
    return _WSEP


def process_segment_worker(args: Tuple[int, str, float, float]):
    idx, seg_path, start, end = args
    try:
        logging.info(f"[PID {os.getpid()}] start seg {idx} [{start}-{end}]")
        sep = _get_worker_sep()

        # Tách trực tiếp
        result = sep.separate(seg_path)

        vocal_file = None
        instrumental_file = None

        # Ưu tiên danh sách trả về từ model
        if isinstance(result, list):
            for n in result:
                p = n if os.path.isabs(n) else os.path.abspath(os.path.join(OUTPUT_DIR, n))
                if os.path.isfile(p):
                    name = Path(p).name.lower()
                    if "(vocals)" in name or "vocals" in name:
                        vocal_file = p
                    elif "(instrumental)" in name or "instrumental" in name:
                        instrumental_file = p

        # Fallback: chỉ quét thư mục chứa segment (không quét cả outputs)
        if not (vocal_file or instrumental_file):
            base_dir = os.path.dirname(seg_path)
            for root, _, files in os.walk(base_dir):
                for f in files:
                    p = os.path.abspath(os.path.join(root, f))
                    name = Path(p).name.lower()
                    if "(vocals)" in name or "vocals" in name:
                        vocal_file = p
                    elif "(instrumental)" in name or "instrumental" in name:
                        instrumental_file = p

        logging.info(f"[PID {os.getpid()}] done seg {idx}")
        return idx, vocal_file, instrumental_file, start, end
    except Exception as e:
        logging.error(f"Worker failed on seg {idx}: {e}")
        return idx, None, None, start, end
def bench_worker(simulate_model: bool = True) -> dict:
    """Worker benchmark: optionally load model once, then report PID."""
    try:
        if simulate_model:
            sep = _get_worker_sep()  # lazy load model in this process
            _ = sep is not None
        return {"pid": os.getpid(), "ok": True}
    except Exception as e:
        return {"pid": os.getpid(), "ok": False, "err": str(e)}

@app.get("/workers/check")
def check_workers(try_workers: int = 0, simulate_model: bool = True, timeout_seconds: int = 180):
    """
    Benchmark how many workers can run concurrently.
    - try_workers: số worker muốn thử (0 = auto dùng số CPU)
    - simulate_model: True => mỗi worker sẽ load model (thực tế hơn)
    - timeout_seconds: thời gian tối đa chờ benchmark
    """
    # Decide target worker count
    cpu_cnt = os.cpu_count() or 1
    n = int(try_workers) if try_workers and try_workers > 0 else cpu_cnt

    # Safety bounds
    n = max(1, min(n, cpu_cnt * 2))  # không vượt quá 2x CPU
    start = time.time()

    # Windows-safe start method
    try:
        mp.set_start_method('spawn', force=True)
    except RuntimeError:
        pass

    results = []
    ok_count = 0
    pids = []

    # Use context manager to ensure pool terminates
    with ProcessPoolExecutor(max_workers=n) as pool:
        futures = [pool.submit(bench_worker, simulate_model) for _ in range(n)]
        for fut in as_completed(futures, timeout=timeout_seconds):
            try:
                res = fut.result(timeout=timeout_seconds)
                results.append(res)
                if res.get("ok"):
                    ok_count += 1
                pid = res.get("pid")
                if pid:
                    pids.append(pid)
            except Exception as e:
                results.append({"ok": False, "err": str(e)})

    elapsed = round(time.time() - start, 3)
    return {
        "requested_workers": int(try_workers) if try_workers else cpu_cnt,
        "actual_workers_used": n,
        "ok_workers": ok_count,
        "failed_workers": n - ok_count,
        "pids": pids,
        "elapsed_seconds": elapsed,
        "simulate_model": bool(simulate_model),
        "cpu_count": cpu_cnt,
    }
# --- API Endpoints ---
@app.post("/separate")
async def separate(
    file: UploadFile = File(...),
    AudioDurationSeconds: int = 0,
    workers: int = 1,
):
    input_path = None
    seg_dir = None
    try:
        req_id = uuid.uuid4().hex
        saved_name = f"{req_id}_{file.filename}"
        file_content = await file.read()
        input_path = os.path.abspath(os.path.join(OUTPUT_DIR, saved_name))
        with open(input_path, "wb") as f:
            f.write(file_content)

        duration = int(AudioDurationSeconds) if AudioDurationSeconds else int(get_audio_duration(input_path))
        should_segment, seg_seconds, ovl_seconds = compute_segment_time(duration)

        created: List[str] = []

        if not should_segment or workers <= 1:
            # Single pass
            sep = get_sep()
            before = list_files_recursive(OUTPUT_DIR)
            result = sep.separate(input_path)
            after = list_files_recursive(OUTPUT_DIR)
            created = [p for p in (after - before) if os.path.abspath(p) != input_path and os.path.isfile(p)]

            if isinstance(result, list) and result:
                normalized = []
                for n in result:
                    p = n if os.path.isabs(n) else os.path.abspath(os.path.join(OUTPUT_DIR, n))
                    if os.path.isfile(p):
                        normalized.append(p)
                if normalized:
                    # prefer files that were just created
                    created_set = set(created)
                    merged = [p for p in normalized if p in created_set] or normalized
                    created = merged
        else:
            # Segmented, parallel
            segs = split_into_segments(input_path, duration, seg_seconds, ovl_seconds, req_id)
            if not segs:
                raise HTTPException(status_code=500, detail="Failed to create audio segments")
            seg_dir = os.path.dirname(segs[0][1])

            cpu_cnt = os.cpu_count() or 1
            actual_workers = max(1, min(int(workers), len(segs), cpu_cnt))
            logging.info(
                f"POOL plan: cpu={cpu_cnt}, requested_workers={workers}, actual_workers={actual_workers}, "
                f"segments={len(segs)}, seg={seg_seconds}s, ovl={ovl_seconds}s"
            )
            try:
                mp.set_start_method('spawn', force=True)
            except RuntimeError:
                pass

            vocal_parts: List[Tuple[str, float, float]] = []
            instr_parts: List[Tuple[str, float, float]] = []

            # Dùng context manager để tự đóng pool, tránh tích lũy process
            logging.info("POOL create -> submitting tasks")
            with ProcessPoolExecutor(max_workers=actual_workers) as pool:
                futures = {pool.submit(process_segment_worker, seg): seg for seg in segs}

                completed = 0
                for fut in as_completed(futures):
                    try:
                        idx, vfile, ifile, start, end = fut.result()
                    except Exception as e:
                        logging.error(f"Segment task failed: {e}")
                        continue

                    completed += 1
                    running_est = max(0, min(actual_workers, len(segs) - completed))
                    logging.info(f"Segment {idx} done ({completed}/{len(segs)}), ~inflight~≈{running_est}")
                    if vfile:
                        vocal_parts.append((vfile, start, end))
                    if ifile:
                        instr_parts.append((ifile, start, end))

            # sort by start
            vocal_parts.sort(key=lambda x: x[1])
            instr_parts.sort(key=lambda x: x[1])
            vocal_paths = [p for p, _, _ in vocal_parts]
            instr_paths = [p for p, _, _ in instr_parts]

            base = os.path.join(OUTPUT_DIR, Path(saved_name).stem)
            vocal_out = merge_audio_pairwise(vocal_paths, f"{base} (Vocals).wav", ovl_seconds) if vocal_paths else None
            instr_out = merge_audio_pairwise(instr_paths, f"{base} (Instrumental).wav", ovl_seconds) if instr_paths else None

            created = []
            if vocal_out and os.path.exists(vocal_out):
                created.append(vocal_out)
            if instr_out and os.path.exists(instr_out):
                created.append(instr_out)

        if not created:
            raise HTTPException(status_code=500, detail="No output files were produced")

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

        # Re-encode large WAVs to compressed format before zipping to reduce size
        def reencode_file(input_p: str, target_fmt: str = "mp3", bitrate: str = "192k", mono_for_vocals: bool = True) -> str:
            ffexe = ffmpeg.get_ffmpeg_exe()
            p = Path(input_p)
            name = p.stem
            out_ext = ".mp3" if target_fmt == "mp3" else ".flac"
            out_path = str(p.with_suffix(out_ext))
            cmd = [ffexe, "-hide_banner", "-loglevel", "error", "-i", str(input_p), "-threads", "1"]
            # If vocals and requested mono
            if mono_for_vocals and "(vocals)" in p.name.lower():
                cmd += ["-ac", "1"]
            # codec selection
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

        # Choose target format: mp3 by default for small size. You can change to 'flac' if lossless needed.
        target_fmt = os.environ.get("OUTPUT_FORMAT", "mp3")
        mp3_bitrate = os.environ.get("MP3_BITRATE", "192k")

        final_files: List[str] = []
        for p in created:
            # If already small (non-wav), keep
            if not str(p).lower().endswith('.wav'):
                final_files.append(p)
                continue
            # re-encode
            outp = reencode_file(p, target_fmt, mp3_bitrate)
            final_files.append(outp)
            # remove original wav if re-encoded
            try:
                if outp != p and os.path.exists(p):
                    os.remove(p)
            except Exception:
                pass

        zip_files(final_files, zip_path, arcname_resolver=resolve_name)

        # Concurrency plan in headers
        headers = {
            "X-Mode": "segmented" if (should_segment and workers > 1) else "single",
            "X-Workers": str(actual_workers if (should_segment and workers > 1) else 1),
            "X-Segments": str(len(segs) if (should_segment and workers > 1) else 1),
            "X-CPU": str(os.cpu_count() or 1),
            "X-SegSeconds": str(seg_seconds if should_segment else 0),
            "X-Overlap": str(ovl_seconds if should_segment else 0),
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

