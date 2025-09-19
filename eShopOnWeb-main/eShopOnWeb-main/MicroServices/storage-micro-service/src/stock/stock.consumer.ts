import { Injectable } from '@nestjs/common';
import { RabbitSubscribe } from '@golevelup/nestjs-rabbitmq';

@Injectable()
export class StockConsumer {
  @RabbitSubscribe({
    exchange: 'stock.exchange',
    routingKey: 'stock.updated',
    queue: 'stock_update_queue',
  })
  async handleStockUpdated(msg: { itemId: string; quantity: number }) {
    console.log('Stock updated event received by consumer:', msg);
    // here you can react: notify, update analytics, etc.
  }
}