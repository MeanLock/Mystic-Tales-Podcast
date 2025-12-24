# Dockerfile for FastAPI deployment
FROM python:3.10-slim

WORKDIR /app

# ✅ Combine apt-get commands into one layer (saves space)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        ffmpeg && \
    ffmpeg -version && \
    ffprobe -version && \
    curl --version && \
    rm -rf /var/lib/apt/lists/*

# ✅ Copy and install dependencies FIRST (best for caching)
# This layer is cached unless requirements.txt changes
COPY requirements.txt /app/
RUN pip install --no-cache-dir -r requirements.txt

# ✅ Copy application code LAST (changes frequently)
# When code changes, only this layer rebuilds
COPY . /app

# ✅ Expose port
EXPOSE 8001

# ✅ Run application
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8001"]