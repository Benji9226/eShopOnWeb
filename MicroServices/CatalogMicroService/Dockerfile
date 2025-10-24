FROM python:3.12-slim

# Install system deps needed by asyncpg/psycopg (libpq) and build tools
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      gcc \
      libpq-dev \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy and install dependencies first (better layer caching)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the service code
COPY . .

# Default start command (compose can override)
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
