import { Injectable } from '@nestjs/common';
import { AmqpConnection } from '@golevelup/nestjs-rabbitmq';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { StockItem } from './entities/stock-item.entity';

@Injectable()
export class StockService {
  constructor(
    private readonly amqpConnection: AmqpConnection,
    @InjectRepository(StockItem)
    private readonly stockRepo: Repository<StockItem>,
  ) {}

  async updateStock(itemId: string, quantity: number) {
    let stockItem = await this.stockRepo.findOne({ where: { itemId } });

    if (!stockItem) {
      stockItem = this.stockRepo.create({ itemId, quantity });
    } else {
      stockItem.quantity = quantity;
    }

    await this.stockRepo.save(stockItem);

    console.log(`Stock updated in DB: ${itemId} → ${quantity}`);

    // publish event
    await this.amqpConnection.publish('stock.exchange', 'stock.updated', {
      itemId,
      quantity,
    });

    return stockItem;
  }
}