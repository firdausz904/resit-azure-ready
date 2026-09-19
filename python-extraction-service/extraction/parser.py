import re
from dataclasses import dataclass
from datetime import date

import dateparser

AMOUNT_PATTERN = re.compile(r"(?:rm|myr)?\s*([0-9]{1,3}(?:[.,][0-9]{3})*[.,][0-9]{2})", re.IGNORECASE)
TOTAL_KEYWORDS = ("total", "jumlah", "grand total", "amount due", "total due")
DATE_KEYWORDS_PATTERN = re.compile(
    r"\b(\d{1,2}[/\-.]\d{1,2}[/\-.]\d{2,4}|\d{1,2}\s+\w+\s+\d{2,4})\b"
)

CATEGORY_KEYWORDS: dict[str, tuple[str, ...]] = {
    "Groceries": ("grocer", "mydin", "village grocer", "jaya grocer", "tesco", "lotus", "giant"),
    "Dining": ("cafe", "coffee", "restoran", "restaurant", "tealive", "old town", "mcdonald", "kfc"),
    "Transport": ("grab", "petronas", "shell", "petrol", "toll", "touch n go", "parking"),
    "Utilities": ("tnb", "tenaga", "syabas", "air selangor", "unifi", "telco", "maxis", "celcom"),
    "Health": ("pharmacy", "guardian", "watsons", "clinic", "hospital"),
}


@dataclass(frozen=True)
class ParsedReceipt:
    merchant: str
    total: float
    purchased_on: date
    suggested_category: str
    merchant_confidence: float
    total_confidence: float
    date_confidence: float
    raw_text: str


def parse(lines: list[tuple[str, float]]) -> ParsedReceipt:
    raw_text = "\n".join(text for text, _ in lines)

    merchant, merchant_confidence = _extract_merchant(lines)
    total, total_confidence = _extract_total(lines)
    purchased_on, date_confidence = _extract_date(lines)

    return ParsedReceipt(
        merchant=merchant,
        total=total,
        purchased_on=purchased_on,
        suggested_category=_suggest_category(merchant),
        merchant_confidence=merchant_confidence,
        total_confidence=total_confidence,
        date_confidence=date_confidence,
        raw_text=raw_text,
    )


def _extract_merchant(lines: list[tuple[str, float]]) -> tuple[str, float]:
    for text, confidence in lines[:5]:
        cleaned = text.strip()
        if len(cleaned) >= 3 and any(character.isalpha() for character in cleaned):
            return cleaned.title(), confidence

    return "Unknown Merchant", 0.0


def _extract_total(lines: list[tuple[str, float]]) -> tuple[float, float]:
    candidates: list[tuple[float, float]] = []

    for text, confidence in lines:
        lowered = text.lower()
        match = AMOUNT_PATTERN.search(text)

        if not match:
            continue

        amount = _to_float(match.group(1))

        if any(keyword in lowered for keyword in TOTAL_KEYWORDS):
            return amount, confidence

        candidates.append((amount, confidence))

    if candidates:
        return max(candidates, key=lambda candidate: candidate[0])

    return 0.0, 0.0


def _extract_date(lines: list[tuple[str, float]]) -> tuple[date, float]:
    for text, confidence in lines:
        match = DATE_KEYWORDS_PATTERN.search(text)

        if not match:
            continue

        parsed = dateparser.parse(match.group(1), settings={"DATE_ORDER": "DMY"})

        if parsed:
            return parsed.date(), confidence

    return date.today(), 0.0


def _suggest_category(merchant: str) -> str:
    lowered = merchant.lower()

    for category, keywords in CATEGORY_KEYWORDS.items():
        if any(keyword in lowered for keyword in keywords):
            return category

    return "Other"


def _to_float(raw: str) -> float:
    return round(float(raw.replace(",", "")), 2)
