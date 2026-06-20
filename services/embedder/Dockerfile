# syntax=docker/dockerfile:1.7

FROM python:3.12-slim AS runtime

ENV PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    HF_HOME=/cache/huggingface \
    SENTENCE_TRANSFORMERS_HOME=/cache/sentence-transformers \
    OMP_NUM_THREADS=2 \
    EMBEDDER_MODEL=intfloat/multilingual-e5-small

WORKDIR /app

# Layer 1: lock down system deps. curl for the healthcheck, build-essential
# stays out because torch's prebuilt wheels are sufficient on linux/amd64.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Layer 2: install Python deps. Pin via the project file.
COPY pyproject.toml ./
RUN pip install --upgrade pip \
    && pip install \
        "fastapi==0.115.0" \
        "uvicorn[standard]==0.30.6" \
        "sentence-transformers==3.1.1" \
        "torch==2.4.1" \
        "numpy<2.0"

# Layer 3: warm the model into the image so first-run latency is sane.
RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('intfloat/multilingual-e5-small')"

# Layer 4: application code.
COPY app.py ./

# Non-root user so the container doesn't run as 0.
RUN useradd --uid 1000 --user-group --no-create-home --shell /sbin/nologin app \
    && chown -R app:app /app /cache
USER app

EXPOSE 8000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "1"]
