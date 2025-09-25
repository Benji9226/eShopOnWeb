import { Module } from '@nestjs/common';
import { RabbitMQModule } from '@golevelup/nestjs-rabbitmq';
import { TypeOrmModule } from '@nestjs/typeorm';
import { CatalogItemStockService } from './catalog-item-stock.service';
import { CatalogItemStockController } from './catalog-item-stock.controller';
import { CatalogItemStockConsumer } from './catalog-item-stock.consumer';
import { CatalogItemStock } from './entities/catalog-item-stock.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([CatalogItemStock]),
    RabbitMQModule.forRoot({
      exchanges: [{ name: 'catalog_item_stock.exchange', type: 'topic' }],
      uri: process.env.RABBITMQ_URI || 'amqp://guest:guest@localhost:5672',
      connectionInitOptions: { wait: false },
    }),
  ],
  controllers: [CatalogItemStockController],
  providers: [CatalogItemStockService, CatalogItemStockConsumer],
})
export class CatalogItemStockModule {}
