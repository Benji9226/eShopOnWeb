import { Controller, Post, Body } from '@nestjs/common';
import { StockService } from './stock.service';

@Controller('stock')
export class StockController {
  constructor(private readonly stockService: StockService) {}

  @Post('update')
  async update(@Body() body: { itemId: string; quantity: number }) {
    return this.stockService.updateStock(body.itemId, body.quantity);
  }
}