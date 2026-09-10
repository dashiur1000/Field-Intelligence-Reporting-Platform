import logging
import os

os.makedirs("Logs", exist_ok=True)

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.FileHandler("Logs/processor.log", encoding="utf-8"),
        logging.StreamHandler()
    ]
)