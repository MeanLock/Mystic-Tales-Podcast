import asyncio
import io
import os
import tempfile
import subprocess
import logging

logger = logging.getLogger(__name__)

class AudioConverter:
    """Utility class để convert audio formats"""
    
    @staticmethod
    def to_wav_pydub(audio_bytes: bytes, source_format: str) -> bytes:
        """Convert bằng pydub"""
        from pydub import AudioSegment
        
        audio = AudioSegment.from_file(
            io.BytesIO(audio_bytes),
            format=source_format
        )
        
        # Normalize về 16kHz mono
        audio = audio.set_frame_rate(16000).set_channels(1)
        
        wav_buffer = io.BytesIO()
        audio.export(wav_buffer, format='wav')
        wav_buffer.seek(0)
        
        return wav_buffer.read()
    
    @staticmethod
    def to_wav_ffmpeg(audio_bytes: bytes, source_ext: str) -> bytes:
        """Convert bằng ffmpeg subprocess"""
        with tempfile.NamedTemporaryFile(suffix=source_ext, delete=False) as f_in:
            f_in.write(audio_bytes)
            path_in = f_in.name
        
        path_out = tempfile.mktemp(suffix='.wav')
        
        try:
            subprocess.run([
                'ffmpeg', '-i', path_in,
                '-ar', '16000', '-ac', '1',
                '-y', path_out
            ], check=True, capture_output=True, timeout=30)
            
            with open(path_out, 'rb') as f:
                return f.read()
        finally:
            os.unlink(path_in)
            if os.path.exists(path_out):
                os.unlink(path_out)
    
    @staticmethod
    async def convert_async(audio_bytes: bytes, source_format: str, 
                           method: str = 'pydub') -> bytes:
        """Async wrapper để không block event loop"""
        loop = asyncio.get_event_loop()
        
        if method == 'pydub':
            func = AudioConverter.to_wav_pydub
        else:
            func = AudioConverter.to_wav_ffmpeg
        
        return await loop.run_in_executor(
            None, 
            func, 
            audio_bytes, 
            source_format
        )