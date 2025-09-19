import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { MicroserviceOptions, Transport } from '@nestjs/microservices';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // REST API port
  app.listen(3000);

  // RabbitMQ microservice
  const microservice = app.connectMicroservice<MicroserviceOptions>({
    transport: Transport.RMQ,
    options: {
      urls: ['amqp://user:pass@rabbitmq:5672'],
      queue: 'stock_queue',
      queueOptions: { durable: true },
    },
  });

  await app.startAllMicroservices();
  console.log('Microservice & API running...');
}
bootstrap();