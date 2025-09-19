import { Module } from '@nestjs/common';
import { RabbitMQModule } from '@golevelup/nestjs-rabbitmq';
import { TypeOrmModule } from '@nestjs/typeorm';
import { StockService } from './stock.service';
import { StockController } from './stock.controller';
import { StockConsumer } from './stock.consumer';
import { StockItem } from './entities/stock-item.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([StockItem]),
    RabbitMQModule.forRoot({
          exchanges: [
            { name: 'stock.exchange', type: 'topic' },
          ],
          uri: process.env.RABBITMQ_URI || 'amqp://guest:guest@localhost:5672',
          connectionInitOptions: { wait: false },
        }),
    ],
  controllers: [StockController],
  providers: [StockService, StockConsumer],
})
export class StockModule {}