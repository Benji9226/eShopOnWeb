import pika
from ..models.catalog import Catalog
import os

RABBITMQ_HOST = os.getenv("RABBITMQ_HOST", "localhost")

class RabbitMQClient:
    def __init__(self, host=RABBITMQ_HOST):
        self.host = host

    def publish(self, queue_name: str, catalog: Catalog):
        connection = pika.BlockingConnection(pika.ConnectionParameters(self.host))
        channel = connection.channel()
        channel.queue_declare(queue=queue_name, durable=True)

        channel.basic_publish(
            exchange="",
            routing_key=queue_name,
            body=catalog.json(),
            properties=pika.BasicProperties(delivery_mode=2)
        )

        connection.close()