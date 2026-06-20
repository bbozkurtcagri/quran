"""Embedding service for QuranCompanion semantic search.

Wraps `intfloat/multilingual-e5-small` behind a small FastAPI surface.
The model is loaded once at startup and reused for every request. Inputs
follow the e5 convention: passages prefixed with "passage: " for indexing,
queries with "query: " at search time. Callers decide which prefix to use
via the `kind` field.
"""

from __future__ import annotations

import logging
import os
from contextlib import asynccontextmanager
from typing import Literal

import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from sentence_transformers import SentenceTransformer

MODEL_NAME = os.environ.get("EMBEDDER_MODEL", "intfloat/multilingual-e5-small")
MAX_BATCH = int(os.environ.get("EMBEDDER_MAX_BATCH", "128"))

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
logger = logging.getLogger("embedder")

# Loaded once during the lifespan startup hook.
_model: SentenceTransformer | None = None


@asynccontextmanager
async def lifespan(_: FastAPI):
    global _model
    logger.info("loading model %s", MODEL_NAME)
    _model = SentenceTransformer(MODEL_NAME)
    # warm up the kernels so the first real request isn't 5 s slower
    _model.encode(["query: warmup"], normalize_embeddings=True)
    logger.info("model ready; dimension=%d", _model.get_sentence_embedding_dimension())
    yield


app = FastAPI(title="QuranCompanion Embedder", version="0.1.0", lifespan=lifespan)


class EmbedRequest(BaseModel):
    texts: list[str] = Field(..., min_length=1, max_length=MAX_BATCH)
    kind: Literal["query", "passage"] = "passage"


class EmbedResponse(BaseModel):
    model: str
    dimension: int
    embeddings: list[list[float]]


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok" if _model is not None else "loading"}


@app.get("/info")
def info() -> dict[str, str | int]:
    if _model is None:
        raise HTTPException(status_code=503, detail="model not loaded")
    return {
        "model": MODEL_NAME,
        "dimension": _model.get_sentence_embedding_dimension(),
        "max_batch": MAX_BATCH,
    }


@app.post("/embed", response_model=EmbedResponse)
def embed(req: EmbedRequest) -> EmbedResponse:
    if _model is None:
        raise HTTPException(status_code=503, detail="model not loaded")

    prefix = "query: " if req.kind == "query" else "passage: "
    prepared = [prefix + (t or "").strip() for t in req.texts]

    vectors = _model.encode(
        prepared,
        normalize_embeddings=True,
        convert_to_numpy=True,
        batch_size=min(len(prepared), 32),
        show_progress_bar=False,
    )
    # convert_to_numpy=True returns ndarray[float32]; cast to plain lists
    # so FastAPI serialises without bringing numpy into the wire format
    embeddings: list[list[float]] = np.asarray(vectors, dtype=np.float32).tolist()

    return EmbedResponse(
        model=MODEL_NAME,
        dimension=_model.get_sentence_embedding_dimension(),
        embeddings=embeddings,
    )
