import { Injectable } from '@nestjs/common';
import { EventPattern, Payload } from '@nestjs/microservices';
import { StockService } from '../stock.service';

@Injectable()
export class StockEventsHandler {
  constructor(private readonly stockService: StockService) {}

  @EventPattern('order.created')
  async handleOrderCreated(@Payload() data: any) {
    console.log('Order created event received:', data);
    await this.stockService.updateStock(data.items);
  }

  @EventPattern('order.cancelled')
  async handleOrderCancelled(@Payload() data: any) {
    console.log('Order cancelled event received:', data);
    await this.stockService.restoreStock(data.items);
  }
}