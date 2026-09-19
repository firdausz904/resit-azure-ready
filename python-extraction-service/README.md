# Resit extraction service

FastAPI microservice wrapping PaddleOCR for offline receipt text extraction, plus a heuristic
parser that pulls merchant, total, date and a suggested category out of the raw OCR lines.

## Run
```
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000
```

`POST /extract` takes a single `multipart/form-data` file field named `file` (jpg, png or pdf)
and returns the parsed fields with a confidence score per field, matching what the .NET
`PythonReceiptExtractionClient` expects.
