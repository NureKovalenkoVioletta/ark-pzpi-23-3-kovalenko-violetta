FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY src/ ./src/
COPY config/ ./config/

ENV PYTHONUNBUFFERED=1
ENV PYTHONPATH=/app/src
ENV DOCKER_ENV=true

WORKDIR /app/src

RUN if [ -f /app/config/config.docker.json ]; then \
        cp /app/config/config.docker.json /app/config/config.json; \
    fi

EXPOSE 5000

CMD ["python", "web_app.py"]

