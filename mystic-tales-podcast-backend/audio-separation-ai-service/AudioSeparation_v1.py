import os
import uuid
import logging
import zipfile
from pathlib import Path
from typing import List, Set
from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import FileResponse
from audio_separator.separator import Separator

app = FastAPI(title="Audio Separator API", version="1.0")

# --- FFmpeg setup ---
FFMPEG_EXE = r"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffmpeg.exe"   # chỉnh đường dẫn này cho đúng máy bạn
FFPROBE_EXE = r"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffprobe.exe"

if os.path.isfile(FFMPEG_EXE):
    ffdir = os.path.dirname(FFMPEG_EXE)
    os.environ["PATH"] = ffdir + os.pathsep + os.environ.get("PATH", "")
    os.environ["FFMPEG_BINARY"] = FFMPEG_EXE
    os.environ["IMAGEIO_FFMPEG_EXE"] = FFMPEG_EXE
    if os.path.isfile(FFPROBE_EXE):
        os.environ["FFPROBE_BINARY"] = FFPROBE_EXE
else:
    logging.warning("FFmpeg not found! Only WAV may work properly.")

# --- Model setup ---
# Trỏ đúng vào thư mục models bạn đang có
MODELS_DIR = "model"
OUTPUT_DIR = "outputs"
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Đặt đúng tên model bạn có trong thư mục models
MODEL_FILE = "UVR_MDXNET_Main.onnx"

# Init separator once
sep = Separator(
    model_file_dir=MODELS_DIR,
    output_dir=OUTPUT_DIR,
    output_format="wav",
    log_level=logging.INFO,
    use_autocast=True
)

try:
    sep.load_model(model_filename=MODEL_FILE)
except Exception as e:
    logging.error(f"Failed to load model {MODEL_FILE}: {e}")
    sep = None

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

# --- API Endpoints ---
@app.get("/health")
def health():
    return {"ok": True, "model_loaded": sep is not None}

@app.post("/separate")
async def separate(file: UploadFile = File(...)):
    if sep is None:
        raise HTTPException(status_code=500, detail="Separator not initialized")

    try:
        # Save uploaded file
        req_id = uuid.uuid4().hex
        saved_name = f"{req_id}_{file.filename}"
        input_path = os.path.abspath(os.path.join(OUTPUT_DIR, saved_name))
        with open(input_path, "wb") as f:
            f.write(await file.read())

        before = list_files_recursive(OUTPUT_DIR)

        # Run separation
        result = sep.separate(input_path)

        after = list_files_recursive(OUTPUT_DIR)
        created = [p for p in (after - before) if os.path.abspath(p) != input_path and os.path.isfile(p)]

        # Nếu thư viện trả list đường dẫn tương đối, hợp nhất với created
        if isinstance(result, list) and result:
            normalized = []
            for n in result:
                p = n if os.path.isabs(n) else os.path.abspath(os.path.join(OUTPUT_DIR, n))
                if os.path.isfile(p):
                    normalized.append(p)
            # Ưu tiên file thực sự mới tạo
            if created:
                # Giữ những file có trong created
                created_set = set(created)
                merged = [p for p in normalized if p in created_set]
                if merged:
                    created = merged
            elif normalized:
                created = normalized

        if not created:
            raise HTTPException(status_code=500, detail="No output files were produced")

        # Nếu có đúng 2 file => zip 2 file, nếu >2 vẫn zip tất cả để client xử lý
        zip_name = f"{Path(saved_name).stem}_stems.zip"
        zip_path = os.path.abspath(os.path.join(OUTPUT_DIR, zip_name))

        # Map tên (Vocals) -> Voice, (Instrumental) -> BackgroundSound trong ZIP
        def resolve_name(p: str) -> str:
            name_lower = Path(p).name.lower()
            ext = Path(p).suffix
            base = f"{Path(saved_name).stem}"
            if "(vocals)" in name_lower:
                return f"{base}_Voice{ext}"
            if "(instrumental)" in name_lower:
                return f"{base}_BackgroundSound{ext}"
            return os.path.basename(p)

        zip_files(created, zip_path, arcname_resolver=resolve_name)

        return FileResponse(
            zip_path,
            media_type="application/zip",
            filename=zip_name
        )
    except HTTPException:
        raise
    except Exception as e:
        logging.exception("Separation failed")
        raise HTTPException(status_code=500, detail=str(e))