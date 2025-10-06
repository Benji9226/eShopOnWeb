import { Injectable } from '@nestjs/common';
import { RabbitSubscribe, RabbitRPC } from '@golevelup/nestjs-rabbitmq';
import { CatalogItemStockService } from './catalog-item-stock.service';

@Injectable()
export class CatalogItemStockConsumer {
  constructor(private readonly stockService: CatalogItemStockService) {}

  // Restock an item (async)
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.restock',
    queue: 'catalog_item_stock_restock_queue',
  })
  async handleRestock(msg: { itemId: number; amount: number }) {
    try {
      console.log('Restock event received:', msg);
      await this.stockService.restock(msg.itemId, msg.amount);
      console.log(`Restock success: itemId=${msg.itemId}, amount=${msg.amount}`);
    } catch (err: any) {
      console.error(
        `Restock failed: itemId=${msg.itemId}, amount=${msg.amount}, reason=${err.message}`,
      );
    }
  }

  // Reserve an item (synchronous RPC)
  @RabbitRPC({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.reserve',
    queue: 'catalog_item_stock_reserve_rpc_queue',
  })
  async handleReserveRpc(msg: { itemId: number; amount: number }) {
    console.log('Reserve RPC request received:', msg);
    try {
      await this.stockService.reserve(msg.itemId, msg.amount);
      console.log(`Reserve success: itemId=${msg.itemId}, amount=${msg.amount}`);
      return { success: true };
    } catch (err: any) {
      console.error(`Reserve failed: itemId=${msg.itemId}, reason=${err.message}`);
      return { success: false, reason: err.message };
    }
  }

  // Confirm an order (async)
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.confirm',
    queue: 'catalog_item_stock_confirm_queue',
  })
  async handleConfirm(msg: { itemId: number; amount: number }) {
    try {
      console.log('Confirm event received:', msg);
      await this.stockService.confirm(msg.itemId, msg.amount);
      console.log(`Confirm success: itemId=${msg.itemId}, amount=${msg.amount}`);
    } catch (err: any) {
      console.error(`Confirm failed: ${err.message}`);
    }
  }

  // Cancel a reservation (async)
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.cancel',
    queue: 'catalog_item_stock_cancel_queue',
  })
  async handleCancel(msg: { itemId: number; amount: number }) {
    try {
      console.log('Cancel Reservation event received:', msg);
      await this.stockService.cancelReservation(msg.itemId, msg.amount);
      console.log(`Cancel success: itemId=${msg.itemId}, amount=${msg.amount}`);
    } catch (err: any) {
      console.error(
        `Cancel failed: itemId=${msg.itemId}, amount=${msg.amount}, reason=${err.message}`,
      );
    }
  }
}
