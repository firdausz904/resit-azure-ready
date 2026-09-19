from fastapi import FastAPI, File, HTTPException, UploadFile
from pydantic import BaseModel

from extraction.ocr import run_ocr
from extraction.parser import parse

app = FastAPI(title="Resit Extraction Service")

SUPPORTED_SUFFIXES = (".jpg", ".jpeg", ".png", ".pdf")


class ExtractionResponse(BaseModel):
    merchant: str
    total: float
    purchased_on: str
    suggested_category: str
    merchant_confidence: float
    total_confidence: float
    date_confidence: float
    raw_text: str


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/extract", response_model=ExtractionResponse)
async def extract(file: UploadFile = File(...)) -> ExtractionResponse:
    if not file.filename or not file.filename.lower().endswith(SUPPORTED_SUFFIXES):
        raise HTTPException(status_code=415, detail="Unsupported file type.")

    file_bytes = await file.read()

    if not file_bytes:
        raise HTTPException(status_code=400, detail="Uploaded file is empty.")

    lines = run_ocr(file_bytes, file.filename)

    if not lines:
        raise HTTPException(status_code=422, detail="Could not read any text from this receipt.")

    parsed = parse(lines)

    return ExtractionResponse(
        merchant=parsed.merchant,
        total=parsed.total,
        purchased_on=parsed.purchased_on.isoformat(),
        suggested_category=parsed.suggested_category,
        merchant_confidence=parsed.merchant_confidence,
        total_confidence=parsed.total_confidence,
        date_confidence=parsed.date_confidence,
        raw_text=parsed.raw_text,
    )
