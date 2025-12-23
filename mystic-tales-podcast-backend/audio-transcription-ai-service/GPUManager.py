from fastapi import FastAPI, File, UploadFile, HTTPException
from transformers import pipeline, AutoModelForSpeechSeq2Seq, AutoProcessor
from datetime import datetime
import soundfile as sf
import gc
import torch
from contextlib import contextmanager
from typing import Dict, Any
import logging

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

class GPUManager:
    """Quản lý GPU resources và memory"""
    
    def __init__(self):
        self.device = self._get_optimal_device()
        self.memory_threshold = 0.8  # 80% memory usage threshold
        
    def _get_optimal_device(self):
        """Chọn device tối ưu"""
        if torch.cuda.is_available():
            # Select GPU with most free memory
            max_memory = 0
            best_device = 0
            
            for i in range(torch.cuda.device_count()):
                props = torch.cuda.get_device_properties(i)
                free_memory = props.total_memory - torch.cuda.memory_allocated(i)
                if free_memory > max_memory:
                    max_memory = free_memory
                    best_device = i
                    
            logger.info(f"Selected GPU {best_device} with {max_memory / 1024**3:.2f} GB free memory")
            return f"cuda:{best_device}"
        else:
            logger.warning("CUDA not available, using CPU")
            return "cpu"
    
    def get_memory_info(self) -> Dict[str, Any]:
        """Lấy thông tin memory GPU"""
        if not torch.cuda.is_available():
            return {"device": "cpu", "memory": "N/A"}
            
        current_device = torch.cuda.current_device()
        props = torch.cuda.get_device_properties(current_device)
        allocated = torch.cuda.memory_allocated(current_device)
        reserved = torch.cuda.memory_reserved(current_device)
        
        return {
            "device": f"cuda:{current_device}",
            "total_memory_gb": props.total_memory / 1024**3,
            "allocated_memory_gb": allocated / 1024**3,
            "reserved_memory_gb": reserved / 1024**3,
            "free_memory_gb": (props.total_memory - allocated) / 1024**3,
            "memory_usage_percent": (allocated / props.total_memory) * 100
        }
    
    def check_memory_usage(self) -> bool:
        """Kiểm tra memory usage có vượt threshold không"""
        if not torch.cuda.is_available():
            return True
            
        memory_info = self.get_memory_info()
        return memory_info["memory_usage_percent"] < (self.memory_threshold * 100)
    
    # @contextmanager
    # def gpu_memory_cleanup(self):
    #     """Context manager để cleanup GPU memory"""
    #     try:
    #         yield
    #     finally:
    #         if torch.cuda.is_available():
    #             torch.cuda.empty_cache()
    #             gc.collect()
    #             logger.debug("GPU memory cleaned up")
    @contextmanager
    def gpu_memory_cleanup(self):
        """Context manager for GPU memory cleanup - safe for CPU"""
        try:
            # ✅ Only cleanup GPU if available
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
            
            yield  # Execute the code block
            
        finally:
            # ✅ Only cleanup GPU if available
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
                gc.collect()
            else:
                # CPU - just gc
                gc.collect()