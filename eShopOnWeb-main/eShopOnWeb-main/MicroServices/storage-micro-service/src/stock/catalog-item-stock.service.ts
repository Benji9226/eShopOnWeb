import { Injectable } from '@nestjs/common';
import { AmqpConnection } from '@golevelup/nestjs-rabbitmq';
import { DataSource } from 'typeorm';
import { CatalogItemStock } from './entities/catalog-item-stock.entity';

@Injectable()
export class CatalogItemStockService {
  constructor(
    private readonly amqpConnection: AmqpConnection,
    private readonly dataSource: DataSource
  ) {}

  /** Helper to safely lock an item and run transactional updates */
  private async withLockedItem(
    itemId: number,
    work: (stock: CatalogItemStock, save: (s: CatalogItemStock) => Promise<void>) => Promise<void>,
  ): Promise<CatalogItemStock> {
    return this.dataSource.transaction(async (manager) => {
      let stock = await manager.findOne(CatalogItemStock, {
        where: { itemId },
        lock: { mode: 'pessimistic_write' },
      });

      if (!stock) {
        stock = manager.create(CatalogItemStock, { itemId, total: 0, reserved: 0 });
        await manager.save(stock);
      }

      const save = async (s: CatalogItemStock) => {
        await manager.save(s);
      };

      await work(stock, save);
      return stock;
    });
  }

  /** Publish events with explicit success/failure */
  private async publishEvent(event: string, payload: any) {
    await this.amqpConnection.publish(
      'catalog_item_stock.exchange',
      `catalog_item_stock.${event}`,
      payload,
    );
  }

  /** Add stock to total */
  async restock(itemId: number, amount: number) {
    const updated = await this.withLockedItem(itemId, async (stock, save) => {
      stock.total += amount;
      await save(stock);
      await this.publishEvent('restock.success', { itemId, total: stock.total });
    });
    return updated;
  }

  /** Reserve stock for an order */
  async reserve(itemId: number, amount: number) {
    try {
      const updated = await this.withLockedItem(itemId, async (stock, save) => {
        const available = stock.total - stock.reserved;
        if (available < amount) throw new Error(`Not enough stock. Available: ${available}`);
        stock.reserved += amount;
        await save(stock);
        await this.publishEvent('reserve.success', { itemId, reserved: stock.reserved });
      });
      return updated;
    } catch (err: any) {
      await this.publishEvent('reserve.failed', { itemId, requested: amount, reason: err.message });
      throw err;
    }
  }

  /** Confirm an order, deduct reserved stock */
  async confirm(itemId: number, amount: number) {
    try {
      const updated = await this.withLockedItem(itemId, async (stock, save) => {
        if (stock.reserved < amount) throw new Error(`Not enough reserved stock. Reserved: ${stock.reserved}`);
        stock.reserved -= amount;
        stock.total -= amount;
        await save(stock);
        await this.publishEvent('confirm.success', { itemId, total: stock.total });
      });
      return updated;
    } catch (err: any) {
      await this.publishEvent('confirm.failed', { itemId, requested: amount, reason: err.message });
      throw err;
    }
  }

  /** Cancel a reservation */
  async cancelReservation(itemId: number, amount: number) {
    try {
      const updated = await this.withLockedItem(itemId, async (stock, save) => {
        if (stock.reserved < amount) throw new Error(`Cannot cancel more than reserved. Reserved: ${stock.reserved}`);
        stock.reserved -= amount;
        await save(stock);
        await this.publishEvent('cancel.success', { itemId, reserved: stock.reserved });
      });
      return updated;
    } catch (err: any) {
      await this.publishEvent('cancel.failed', { itemId, requested: amount, reason: err.message });
      throw err;
    }
  }
}
