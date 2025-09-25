import { Controller, Post, Body } from '@nestjs/common';
import { CatalogItemStockService } from './catalog-item-stock.service';

@Controller('catalog-item-stock')
export class CatalogItemStockController {
  constructor(private readonly stockService: CatalogItemStockService) {}

  @Post('update')
  async update(@Body() body: { itemId: number; quantity: number }) {
    return this.stockService.updateStock(body.itemId, body.quantity);
  }
}
