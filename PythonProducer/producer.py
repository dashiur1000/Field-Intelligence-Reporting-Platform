import json
import os
import logging
from email import message_from_binary_file
from pathlib import Path
from confluent_kafka import Producer


bootstrap_servers = os.getenv('KAFKA_BOOTSTRAP_SERVERS', 'kafka:9092')
conf = {
    'bootstrap.servers': bootstrap_servers
}
producer = Producer(conf)

def delivery_report(err, msg):
    if err is not None:
        logging.warning(f'Message delivery failed: {err}')
        print(f'Message delivery failed: {err}')
    else:
        logging.info(f'Message delivered to {msg.topic()}')
        print(f'Message delivered to {msg.topic()}')

def produce_json_to_kafka(json_file_path, topic_name):
    count = 0
    logging.info("open file")
    try:
        with open(json_file_path, "r", encoding="utf-8") as file:
            reader = json.load(file)

            for line in reader:
                value = json.dumps(line)

                producer.produce(
                    topic_name,
                    value=value.encode("utf-8"),
                    callback=delivery_report
                )
                producer.poll(0)
                count += 1
                logging.info("Sent to Topic")
            producer.flush(timeout=5)
            logging.info("Sending completed")
        print(f"{count}")
    except Exception as e:
        logging.error(f"ERROR! {e}")


def main():
    topic_name = "field-reports"
    json_file_path = Path("../Data/field_reports.json")
    produce_json_to_kafka(json_file_path, topic_name)