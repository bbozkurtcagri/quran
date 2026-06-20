# Embedder

Tiny FastAPI wrapper around
[`intfloat/multilingual-e5-small`](https://huggingface.co/intfloat/multilingual-e5-small)
that powers semantic search for QuranCompanion. Lives in its own container
because the model + transformer + torch stack is meaningfully easier in
Python than threading a multilingual BERT tokenizer through ONNX from .NET.

## Endpoints

| Method | Path      | Body                                              | Response                                  |
| ------ | --------- | ------------------------------------------------- | ----------------------------------------- |
| GET    | `/health` | —                                                 | `{"status": "ok"}`                        |
| GET    | `/info`   | —                                                 | `{model, dimension, max_batch}`           |
| POST   | `/embed`  | `{"texts": ["..."], "kind": "passage" \| "query"}` | `{model, dimension, embeddings: [[...]]}` |

`kind` controls the e5 prefix:

- `passage` — used at index time (each verse meal)
- `query` — used at search time (the user's text)

The model returns L2-normalised 384-dim vectors, so cosine similarity in
Postgres reduces to a dot product (or `embedding <=> $query` with pgvector).

## Run locally

```bash
docker compose up -d embedder
curl http://localhost:8001/health
curl -s -X POST http://localhost:8001/embed \
  -H 'Content-Type: application/json' \
  -d '{"texts": ["sabır"], "kind": "query"}' | jq '.embeddings | length, .embeddings[0] | length'
```

## Notes

- Image is ~2 GB once torch + model are baked in. First build takes 5–8 min
  (downloads torch wheels + model weights). Subsequent runs are instant.
- CPU-only. Adequate for our scale (6,236 indexed passages, a few
  queries/second). GPU is unnecessary.
- The model is bundled into the image at build time, so the container can
  start without internet access.
