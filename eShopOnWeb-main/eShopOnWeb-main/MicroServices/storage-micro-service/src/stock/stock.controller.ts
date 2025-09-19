import { Controller, Get, Param } from '@nestjs/common';
import { StockService } from './stock.service';

@Controller('stock')
export class StockController {
  constructor(private readonly stockService: StockService) {}

  @Get(':productId')
  async getStock(@Param('productId') productId: string) {
    // TODO: fetch stock from DB
    return { productId, available: 100 };
  }
}