# main.py
from fastapi import FastAPI, File, UploadFile, HTTPException
from transformers import pipeline
import tempfile
import os
import librosa
import soundfile as sf
import gc
import torch
from datetime import datetime

app = FastAPI(title="PhoWhisper Transcription API")

# Global variable để lưu transcriber
transcriber = None

def load_model_lazy():
    """Lazy load model khi cần thiết"""
    global transcriber
    
    if transcriber is not None:
        return transcriber
    
    try:
        print("Loading PhoWhisper model...")
        
        # Thử load model local trước
        local_model_path = "./PhoWhisper-large"
        if os.path.exists(local_model_path) and os.path.exists(os.path.join(local_model_path, "config.json")):
            print(f"Loading local model from {local_model_path}")
            transcriber = pipeline(
                "automatic-speech-recognition", 
                model=local_model_path,
                torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
                device=0 if torch.cuda.is_available() else -1
            )
        else:
            print("Loading model from Hugging Face...")
            transcriber = pipeline(
                "automatic-speech-recognition", 
                model="vinai/PhoWhisper-large",
                torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
                device=0 if torch.cuda.is_available() else -1
            )
        
        print("Model loaded successfully!")
        return transcriber
        
    except Exception as e:
        print(f"Error loading PhoWhisper model: {e}")
        print("Trying fallback to Whisper base model...")
        try:
            transcriber = pipeline(
                "automatic-speech-recognition", 
                model="openai/whisper-base"
            )
            print("Fallback model loaded successfully!")
            return transcriber
        except Exception as fallback_error:
            print(f"All models failed to load: {fallback_error}")
            return None

current_transcriber = load_model_lazy()


@app.post("/transcribe")
async def transcribe(file: UploadFile = File(...)):
    """Upload audio file và nhận transcript"""
    
    # Load model khi cần thiết
    
    if current_transcriber is None:
        raise HTTPException(status_code=503, detail="Model not available")
    
    # Tạo thư mục temp trong project nếu chưa có
    temp_dir = "./temp_audio"
    if not os.path.exists(temp_dir):
        os.makedirs(temp_dir)
    
    # Tạo tên file tạm với timestamp
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S_%f")
    input_filename = f"input_{timestamp}{os.path.splitext(file.filename)[1]}"
    output_filename = f"output_{timestamp}.wav"
    
    input_path = os.path.join(temp_dir, input_filename)
    output_path = os.path.join(temp_dir, output_filename)
    
    try:
        # Ghi file upload vào thư mục temp
        content = await file.read()
        with open(input_path, 'wb') as f:
            f.write(content)
        
        # Convert to 16kHz như yêu cầu của model
        audio, sr = librosa.load(input_path, sr=16000)
        sf.write(output_path, audio, 16000)
        
        # Kiểm tra độ dài audio
        audio_duration = len(audio) / sr
        print(f"Audio duration: {audio_duration:.2f} seconds")
        
        # Transcribe với xử lý audio dài
        try:
            if audio_duration > 30:
                # Audio dài hơn 30s, cần return_timestamps=True
                print("Long audio detected, using timestamp mode...")
                result = current_transcriber(output_path, return_timestamps=True)
                
                # Kết hợp tất cả chunks thành một text duy nhất
                if isinstance(result, dict) and 'chunks' in result:
                    output = ' '.join([chunk['text'] for chunk in result['chunks']])
                else:
                    output = result['text'] if isinstance(result, dict) else str(result)
            else:
                # Audio ngắn, xử lý bình thường
                print("Short audio, using normal mode...")
                result = current_transcriber(output_path)
                output = result['text'] if isinstance(result, dict) else str(result)
                
        except Exception as transcribe_error:
            print(f"Transcription error: {transcribe_error}")
            # Fallback: thử với return_timestamps=True
            try:
                print("Retrying with timestamp mode as fallback...")
                result = current_transcriber(output_path, return_timestamps=True, language='vi')
                if isinstance(result, dict) and 'chunks' in result:
                    output = ' '.join([chunk['text'] for chunk in result['chunks']])
                else:
                    output = result['text'] if isinstance(result, dict) else str(result)
            except Exception as fallback_error:
                raise HTTPException(
                    status_code=500, 
                    detail=f"Transcription failed: {str(fallback_error)}"
                )
        
        # Clear cache để giải phóng memory (nếu có torch)
        try:
            import torch
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
        except ImportError:
            pass
        
        gc.collect()
        
        return {"transcript": output}
        
    finally:
        # Xóa file tạm ngay lập tức
        for temp_file in [input_path, output_path]:
            if os.path.exists(temp_file):
                try:
                    os.unlink(temp_file)
                except Exception as e:
                    print(f"Warning: Could not delete temp file {temp_file}: {e}")

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "model_loaded": transcriber is not None,
        "temp_dir": "./temp_audio"
    }

@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup khi shutdown"""
    global transcriber
    if transcriber is not None:
        del transcriber
        transcriber = None
    
    # Clear cache
    try:
        import torch
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
    except ImportError:
        pass
    
    gc.collect()
    
    # Xóa thư mục temp nếu còn file nào
    temp_dir = "./temp_audio"
    if os.path.exists(temp_dir):
        try:
            import shutil
            shutil.rmtree(temp_dir)
        except Exception as e:
            print(f"Warning: Could not clean temp directory: {e}")