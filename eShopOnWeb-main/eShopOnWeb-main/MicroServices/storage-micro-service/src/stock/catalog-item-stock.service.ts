import { Injectable } from '@nestjs/common';
import { AmqpConnection } from '@golevelup/nestjs-rabbitmq';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { CatalogItemStock } from './entities/catalog-item-stock.entity';

@Injectable()
export class CatalogItemStockService {
  constructor(
    private readonly amqpConnection: AmqpConnection,
    @InjectRepository(CatalogItemStock)
    private readonly stockRepo: Repository<CatalogItemStock>,
  ) {}

  async updateStock(itemId: number, quantity: number) {
    let stockItem = await this.stockRepo.findOne({ where: { itemId } });

    if (!stockItem) {
      stockItem = this.stockRepo.create({ itemId, quantity });
    } else {
      stockItem.quantity = quantity;
    }

    await this.stockRepo.save(stockItem);

    console.log(`Stock updated in DB: ${itemId} → ${quantity}`);

    // publish event
    await this.amqpConnection.publish(
      'catalog_item_stock.exchange',
      'catalog_item_stock.updated',
      { itemId, quantity },
    );

    return stockItem;
  }
}
