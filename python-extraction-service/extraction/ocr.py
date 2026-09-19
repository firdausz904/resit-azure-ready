from functools import lru_cache
from io import BytesIO
from pathlib import Path

from paddleocr import PaddleOCR
from pdf2image import convert_from_bytes
from PIL import Image


@lru_cache(maxsize=1)
def get_engine() -> PaddleOCR:
    return PaddleOCR(lang="en", use_angle_cls=True, show_log=False)


def load_pages(file_bytes: bytes, file_name: str) -> list[Image.Image]:
    suffix = Path(file_name).suffix.lower()

    if suffix == ".pdf":
        return convert_from_bytes(file_bytes, dpi=300)

    return [Image.open(BytesIO(file_bytes)).convert("RGB")]


def run_ocr(file_bytes: bytes, file_name: str) -> list[tuple[str, float]]:
    engine = get_engine()
    lines: list[tuple[str, float]] = []

    for page in load_pages(file_bytes, file_name):
        result = engine.ocr(_to_ndarray(page), cls=True)

        for block in result or []:
            for _, (text, confidence) in block:
                lines.append((text.strip(), float(confidence)))

    return lines


def _to_ndarray(image: Image.Image):
    import numpy as np

    return np.array(image)
