import { Injectable } from '@nestjs/common';
import { RabbitSubscribe } from '@golevelup/nestjs-rabbitmq';
import { CatalogItemStockService } from './catalog-item-stock.service';

@Injectable()
export class CatalogItemStockConsumer {
  constructor(private readonly stockService: CatalogItemStockService) {}

  // Restock an item
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.restock',
    queue: 'catalog_item_stock_restock_queue',
  })
  async handleRestock(msg: { itemId: number; amount: number }) {
    console.log('Restock event received:', msg);
    await this.stockService.restock(msg.itemId, msg.amount);
  }

  // Reserve stock for an order
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.reserve',
    queue: 'catalog_item_stock_reserve_queue',
  })
  async handleReserve(msg: { itemId: number; amount: number }) {
    console.log('Reserve event received:', msg);
    await this.stockService.reserve(msg.itemId, msg.amount);
  }

  // Confirm an order, deduct reserved stock
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.confirm',
    queue: 'catalog_item_stock_confirm_queue',
  })
  async handleConfirm(msg: { itemId: number; amount: number }) {
    console.log('Confirm event received:', msg);
    await this.stockService.confirm(msg.itemId, msg.amount);
  }

  // Cancel a reservation
  @RabbitSubscribe({
    exchange: 'catalog_item_stock.exchange',
    routingKey: 'catalog_item_stock.cancel',
    queue: 'catalog_item_stock_cancel_queue',
  })
  async handleCancel(msg: { itemId: number; amount: number }) {
    console.log('Cancel reservation event received:', msg);
    await this.stockService.cancelReservation(msg.itemId, msg.amount);
  }
}