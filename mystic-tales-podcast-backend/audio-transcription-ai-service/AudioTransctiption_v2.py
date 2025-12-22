from fastapi import FastAPI, File, UploadFile, HTTPException
from transformers import pipeline
from datetime import datetime
import tempfile
import os
import librosa
import soundfile as sf
import gc
import torch

app = FastAPI(title="PhoWhisper Transcription API v2")

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
            try:
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model=local_model_path,
                    torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
                    device=0 if torch.cuda.is_available() else -1
                )
            except Exception as e:
                print(f"Failed to load local model with advanced settings: {e}")
                # Thử load đơn giản hơn
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model=local_model_path
                )
        else:
            print("Loading model from Hugging Face...")
            try:
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model="vinai/PhoWhisper-medium",
                    torch_dtype=torch.float16 if torch.cuda.is_available() else torch.float32,
                    device=0 if torch.cuda.is_available() else -1
                )
            except Exception as e:
                print(f"Failed to load HF model with advanced settings: {e}")
                # Thử load đơn giản hơn
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model="vinai/PhoWhisper-large"
                )
        
        print("Model loaded successfully!")
        return transcriber
        
    except Exception as e:
        print(f"Error loading PhoWhisper model: {e}")
        print("Trying fallback to Whisper base model...")
        try:
            # Thử load Whisper với các cài đặt đơn giản nhất
            transcriber = pipeline(
                "automatic-speech-recognition", 
                model="openai/whisper-base"
            )
            print("Fallback model loaded successfully!")
            return transcriber
        except Exception as fallback_error:
            print(f"Whisper base also failed: {fallback_error}")
            
            # Thử model nhỏ hơn nữa
            try:
                print("Trying smaller Whisper tiny model...")
                transcriber = pipeline(
                    "automatic-speech-recognition", 
                    model="openai/whisper-tiny"
                )
                print("Whisper tiny model loaded successfully!")
                return transcriber
            except Exception as tiny_error:
                print(f"All models failed to load: {tiny_error}")
                return None
            
@app.get("/gpu-info")
async def gpu_info():
    """Kiểm tra thông tin GPU và performance"""
    info = {
        "cuda_available": torch.cuda.is_available(),
        # "cuda_version": torch.version.cuda if torch.cuda.is_available() else None,
        "cuda_version": torch.version.cuda,

        "torch_version": torch.__version__
    }
    
    if torch.cuda.is_available():
        info.update({
            "gpu_count": torch.cuda.device_count(),
            "current_device": torch.cuda.current_device(),
            "gpu_name": torch.cuda.get_device_name(0),
            "gpu_memory_total": f"{torch.cuda.get_device_properties(0).total_memory / 1024**3:.2f} GB",
            "gpu_memory_allocated": f"{torch.cuda.memory_allocated(0) / 1024**3:.2f} GB",
            "gpu_memory_reserved": f"{torch.cuda.memory_reserved(0) / 1024**3:.2f} GB"
        })
    
    return info

@app.post("/transcribe")
async def transcribe(file: UploadFile = File(...)):
    """Upload audio file và nhận transcript"""
    
    # Load model khi cần thiết
    current_transcriber = load_model_lazy()
            
    if current_transcriber is None:
        raise HTTPException(status_code=503, detail="Model not available")
    
    # Validate file type
    if not file.filename:
        raise HTTPException(status_code=400, detail="No filename provided")
    
    allowed_extensions = {'.wav', '.mp3', '.m4a', '.flac', '.ogg', '.aac'}
    file_ext = os.path.splitext(file.filename)[1].lower()
    if file_ext not in allowed_extensions:
        raise HTTPException(
            status_code=400, 
            detail=f"Unsupported file format. Allowed: {', '.join(allowed_extensions)}"
        )
    
    # Tạo thư mục temp trong project nếu chưa có
    temp_dir = "./temp_audio_v2"
    if not os.path.exists(temp_dir):
        os.makedirs(temp_dir)
    
    # Tạo tên file tạm với timestamp
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S_%f")
    input_filename = f"input_{timestamp}{file_ext}"
    output_filename = f"output_{timestamp}.wav"
    
    input_path = os.path.join(temp_dir, input_filename)
    output_path = os.path.join(temp_dir, output_filename)
    
    try:
        # Ghi file upload vào thư mục temp
        content = await file.read()
        if len(content) == 0:
            raise HTTPException(status_code=400, detail="Empty file uploaded")
            
        with open(input_path, 'wb') as f:
            f.write(content)
        
        # Convert to 16kHz như yêu cầu của model
        try:
            audio, sr = librosa.load(input_path, sr=16000)
            if len(audio) == 0:
                raise HTTPException(status_code=400, detail="Invalid or corrupted audio file")
            
            sf.write(output_path, audio, 16000)
        except Exception as audio_error:
            raise HTTPException(
                status_code=400, 
                detail=f"Audio processing failed: {str(audio_error)}"
            )
        
        # Kiểm tra độ dài audio
        audio_duration = len(audio) / sr
        print(f"Audio duration: {audio_duration:.2f} seconds")
        
        # Transcribe với xử lý tương thích các phiên bản
        try:
            if audio_duration > 30:
                # Audio dài hơn 30s, cần return_timestamps=True
                print("Long audio detected, using timestamp mode...")
                try:
                    # Thử với generate_kwargs trước
                    result = current_transcriber(
                        output_path, 
                        return_timestamps=True,
                        generate_kwargs={
                            "language": "vi",
                            "task": "transcribe"
                        }
                    )
                except TypeError:
                    # Nếu không hỗ trợ generate_kwargs, thử cách khác
                    print("generate_kwargs not supported, trying alternative...")
                    result = current_transcriber(output_path, return_timestamps=True)
                
                # Xử lý kết quả
                if isinstance(result, dict) and 'chunks' in result:
                    output = ' '.join([chunk['text'] for chunk in result['chunks']])
                elif isinstance(result, dict) and 'text' in result:
                    output = result['text']
                else:
                    output = str(result)
            else:
                # Audio ngắn, xử lý bình thường
                print("Short audio, using normal mode...")
                try:
                    # Thử với generate_kwargs trước
                    result = current_transcriber(
                        output_path,
                        generate_kwargs={
                            "language": "vi",
                            "task": "transcribe"
                        }
                    )
                except TypeError:
                    # Nếu không hỗ trợ generate_kwargs, dùng cách đơn giản
                    print("generate_kwargs not supported, using simple mode...")
                    result = current_transcriber(output_path)
                
                output = result['text'] if isinstance(result, dict) else str(result)
                
        except Exception as transcribe_error:
            print(f"Transcription error: {transcribe_error}")
            # Fallback: thử với cách đơn giản nhất
            try:
                print("Retrying with simplest mode...")
                result = current_transcriber(output_path)
                output = result['text'] if isinstance(result, dict) else str(result)
            except Exception as fallback_error:
                raise HTTPException(
                    status_code=500, 
                    detail=f"Transcription failed: {str(fallback_error)}"
                )
        
        # Clear cache để giải phóng memory
        try:
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
        except:
            pass
        
        gc.collect()
        
        return {
            "transcript": output,
            "duration_seconds": audio_duration,
            "file_size_bytes": len(content),
            "model_used": getattr(current_transcriber.model, 'name_or_path', 'unknown')
        }
        
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
    model_info = "Not loaded"
    if transcriber is not None:
        try:
            model_info = getattr(transcriber.model, 'name_or_path', 'Unknown model')
        except:
            model_info = "Loaded (unknown name)"
    
    return {
        "status": "healthy",
        "version": "v2",
        "model_loaded": transcriber is not None,
        "model_info": model_info,
        "cuda_available": torch.cuda.is_available(),
        "temp_dir": "./temp_audio_v2",
        "supported_formats": [".wav", ".mp3", ".m4a", ".flac", ".ogg", ".aac"]
    }

@app.get("/model-info")
async def model_info():
    """Thông tin chi tiết về model"""
    if transcriber is None:
        return {"error": "Model not loaded"}
    
    try:
        return {
            "model_name": getattr(transcriber.model, 'name_or_path', 'Unknown'),
            "model_type": type(transcriber.model).__name__,
            "device": str(transcriber.device) if hasattr(transcriber, 'device') else 'unknown',
            "torch_dtype": str(transcriber.torch_dtype) if hasattr(transcriber, 'torch_dtype') else 'unknown'
        }
    except Exception as e:
        return {"error": f"Could not get model info: {str(e)}"}

@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup khi shutdown"""
    global transcriber
    if transcriber is not None:
        try:
            del transcriber
        except:
            pass
        transcriber = None
    
    # Clear cache
    try:
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
    except:
        pass
    
    gc.collect()
    
    # Xóa thư mục temp nếu còn file nào
    for temp_dir in ["./temp_audio_v2", "./temp_audio"]:
        if os.path.exists(temp_dir):
            try:
                import shutil
                shutil.rmtree(temp_dir)
                print(f"Cleaned up temp directory: {temp_dir}")
            except Exception as e:
                print(f"Warning: Could not clean up temp directory {temp_dir}: {e}")