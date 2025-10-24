import { Injectable } from '@nestjs/common';
import { AmqpConnection } from '@golevelup/nestjs-rabbitmq';
import { DataSource } from 'typeorm';
import { CatalogItemStock } from './entities/catalog-item-stock.entity';
import { Item } from './catalog-item-stock.consumer';

@Injectable()
export class CatalogItemStockService {
  constructor(
    private readonly amqpConnection: AmqpConnection,
    private readonly dataSource: DataSource
  ) {}

  private async withLockedItems(
    items: Item[],
    work: (stocks: CatalogItemStock[], save: (s: CatalogItemStock) => Promise<void>) => Promise<void>
  ): Promise<CatalogItemStock[]> {
    const sortedItems = [...items].sort((a, b) => a.itemId - b.itemId);

    return this.dataSource.transaction(async (manager) => {
      const lockedStocks: CatalogItemStock[] = [];

      for (const item of sortedItems) {
        let stock = await manager.findOne(CatalogItemStock, {
          where: { itemId: item.itemId },
          lock: { mode: 'pessimistic_write' },
        });

        if (!stock) {
          stock = manager.create(CatalogItemStock, { itemId: item.itemId, total: 0, reserved: 0 });
          await manager.save(stock);
        }

        lockedStocks.push(stock);
      }

      const save = async (s: CatalogItemStock) => {
        await manager.save(s);
      };

      await work(lockedStocks, save);

      return lockedStocks;
    });
  }

  async getFullStock(): Promise<Item[]> {
    const stocks = await this.dataSource.getRepository(CatalogItemStock).find();
    return stocks.map(s => ({
      itemId: s.itemId,
      amount: s.total,       
      reserved: s.reserved,  
    }));
  }

  private async publishEvent(event: string, payload: any) {
    await this.amqpConnection.publish(
      'catalog_item_stock.exchange',
      `catalog_item_stock.${event}`,
      payload,
    );
  }

  async restockAtomic(items: Item[]) {
    await this.withLockedItems(items, async (stocks, save) => {
      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];

        stock.total += item.amount;
        await save(stock);

        await this.publishEvent('restock.success', { itemId: item.itemId, total: stock.total });
      }
    });
  }

  async reserveAtomic(items: Item[]) {
    await this.withLockedItems(items, async (stocks, save) => {
      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        const available = stock.total - stock.reserved;
        if (available < item.amount) {
          throw new Error(`Not enough stock for item ${item.itemId}. Available: ${available}`);
        }
      }

      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        stock.reserved += item.amount;
        await save(stock);

        await this.publishEvent('reserve.success', { itemId: item.itemId, reserved: stock.reserved });
      }
    });
  }

  async confirmAtomic(items: Item[]) {
    await this.withLockedItems(items, async (stocks, save) => {
      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        if (stock.reserved < item.amount) {
          throw new Error(`Not enough reserved stock for item ${item.itemId}. Reserved: ${stock.reserved}`);
        }
      }

      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        stock.reserved -= item.amount;
        stock.total -= item.amount;
        await save(stock);

        await this.publishEvent('confirm.success', { itemId: item.itemId, total: stock.total });
      }
    });
  }

  async cancelAtomic(items: Item[]) {
    await this.withLockedItems(items, async (stocks, save) => {
      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        if (stock.reserved < item.amount) {
          throw new Error(`Cannot cancel more than reserved for item ${item.itemId}. Reserved: ${stock.reserved}`);
        }
      }

      for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const stock = stocks[i];
        stock.reserved -= item.amount;
        await save(stock);

        await this.publishEvent('cancel.success', { itemId: item.itemId, reserved: stock.reserved });
      }
    });
  }
}
