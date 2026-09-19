FROM python:3.11-slim

# System dependencies:
# - poppler-utils: required by pdf2image to rasterize PDF pages
# - libgl1 / libglib2.0-0: required by OpenCV (a paddleocr dependency) in headless containers
# - libgomp1: OpenMP runtime, required by libpaddle.so
RUN apt-get update && apt-get install -y --no-install-recommends \
    poppler-utils \
    libgl1 \
    libglib2.0-0 \
    libgomp1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Install Python dependencies first so this layer is cached unless requirements.txt changes
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the application code
COPY . .

EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
