import { Injectable } from '@nestjs/common';
import { RabbitSubscribe, RabbitRPC } from '@golevelup/nestjs-rabbitmq';
import { CatalogItemStockService } from './catalog-item-stock.service';

export interface Item {
  itemId: number;
  amount: number;
}

@Injectable()
export class CatalogItemStockConsumer {
  constructor(private readonly stockService: CatalogItemStockService) {}

  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.restock',
    queue: 'catalog_item_stock_restock_queue',
  })
  async handleRestock(msg: Item[]) {
    try {
      console.log('Restock event received:', msg);
      await this.stockService.restockAtomic(msg);
      console.log('Restock batch success');
    } catch (err: any) {
      console.error(`Restock batch failed: ${err.message}`);
    }
  }

  @RabbitRPC({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.getall',
    queue: 'catalog_item_stock_getall_queue',
  })
  public async handleGetAll(msg: any): Promise<Item[]> {
    return this.stockService.getFullStock();
  }

  @RabbitRPC({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.reserve',
    queue: 'catalog_item_stock_reserve_rpc_queue',
  })
  async handleReserveRpc(msg: Item[]) {
    console.log('Reserve RPC request received:', msg);
    try {
      await this.stockService.reserveAtomic(msg);
      return { success: true };
    } catch (err: any) {
      console.error(`Reserve batch failed: ${err.message}`);
      return { success: false, reason: err.message };
    }
  }

  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.confirm',
    queue: 'catalog_item_stock_confirm_queue',
  })
  async handleConfirm(msg: Item[]) {
    try {
      console.log('Confirm Order event received:', msg);
      await this.stockService.confirmAtomic(msg);
      console.log('Confirm batch success');
    } catch (err: any) {
      console.error(`Confirm batch failed: ${err.message}`);
    }
  }

  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.cancel',
    queue: 'catalog_item_stock_cancel_queue',
  })
  async handleCancel(msg: Item[]) {
    try {
      console.log('Cancel Reservation event received:', msg);
      await this.stockService.cancelAtomic(msg);
      console.log('Cancel batch success');
    } catch (err: any) {
      console.error(`Cancel batch failed: ${err.message}`);
    }
  }
}
