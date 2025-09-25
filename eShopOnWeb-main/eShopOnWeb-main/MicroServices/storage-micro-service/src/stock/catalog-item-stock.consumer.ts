import { Injectable } from '@nestjs/common';
import { RabbitSubscribe } from '@golevelup/nestjs-rabbitmq';

@Injectable()
export class CatalogItemStockConsumer {
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.updated',
    queue: 'catalog_item_stock_update_queue',
  })
  async handleStockUpdated(msg: { itemId: number; quantity: number }) {
    console.log('Catalog item stock updated event received:', msg);
    // optionally react here
  }
}
