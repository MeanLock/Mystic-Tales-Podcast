from fastapi import FastAPI
import os
from fastapi.middleware.cors import CORSMiddleware
# from SurveyTalkEmbeddingVector_v1 import app as SurveyTalkEmbeddingVector_v1_app
# from AudioTransctiption_v1 import app as AudioTransctiption_v1_app
from AudioSeparation_v1 import app as AudioSeparation_v1_app
from AudioSeparation_v2 import app as AudioSeparation_v2_app
import logging


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
app.mount("/v1", AudioSeparation_v1_app)
app.mount("/v2", AudioSeparation_v2_app)