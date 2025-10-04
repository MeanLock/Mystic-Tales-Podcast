from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer
import numpy as np
from typing import List
import os
from fastapi.middleware.cors import CORSMiddleware
# from SurveyTalkEmbeddingVector_v1 import app as SurveyTalkEmbeddingVector_v1_app
# from AudioTransctiption_v1 import app as AudioTransctiption_v1_app
from AudioTransctiption_v2 import app as AudioTransctiption_v2_app
from AudioTransctiption_v3 import app as AudioTransctiption_v3_app
from AudioTransctiption_v4 import app as AudioTransctiption_v4_app
from AudioTransctiption_v5 import app as AudioTransctiption_v5_app

os.environ["TF_CPP_MIN_LOG_LEVEL"] = "2"
os.environ["TF_ENABLE_ONEDNN_OPTS"] = "0"


app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # hoặc cụ thể ["https://embedding-vector-api.abc.vn"]
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# app.mount("/v1", SurveyTalkEmbeddingVector_v1_app)
# app.mount("/v1", AudioTransctiption_v1_app)Union
app.mount("/v2", AudioTransctiption_v2_app)
app.mount("/v3", AudioTransctiption_v3_app)
app.mount("/v4", AudioTransctiption_v4_app) 
app.mount("/v5", AudioTransctiption_v5_app) 