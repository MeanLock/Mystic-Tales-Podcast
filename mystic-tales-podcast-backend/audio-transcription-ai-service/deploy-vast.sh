#!/bin/bash

# Vast.ai deployment script for audio transcription API

echo "🚀 Deploying Audio Transcription API on Vast.ai..."

# Stop and remove existing container (if any)
docker stop audio-transcription-service 2>/dev/null || true
docker rm audio-transcription-service 2>/dev/null || true

# Build the image
echo "📦 Building Docker image..."
docker build -t audio-transcription-api:latest .

# Run the container with GPU support
echo "🏃 Starting container..."
docker run -d \
  --name audio-transcription-service \
  --gpus all \
  -p 8001:8001 \
  -v /root/.cache/huggingface:/root/.cache/huggingface \
  -e HF_HOME=/root/.cache/huggingface \
  -e HF_HUB_DISABLE_TELEMETRY=1 \
  -e NVIDIA_VISIBLE_DEVICES=all \
  -e NVIDIA_DRIVER_CAPABILITIES=compute,utility \
  --restart unless-stopped \
  audio-transcription-api:latest

# Wait for container to start
echo "⏳ Waiting for container to start..."
sleep 5

# Check container status
echo "📊 Container status:"
docker ps | grep audio-transcription-service

# Show logs
echo "📜 Container logs:"
docker logs audio-transcription-service --tail 50

# Test health endpoint
echo "🏥 Testing health endpoint..."
sleep 10
curl -s http://localhost:8001/health | python3 -m json.tool || echo "Service not ready yet, check logs"

echo "✅ Deployment complete!"
echo "🌐 API available at: http://YOUR_VAST_IP:8001"
echo "📝 Check logs: docker logs -f audio-transcription-service"