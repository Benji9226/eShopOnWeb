import { Injectable } from '@nestjs/common';

@Injectable()
export class StockService {
  async updateStock(items: { productId: string; quantity: number }[]) {
    // TODO: implement DB update logic
    console.log('Updating stock:', items);
  }

  async restoreStock(items: { productId: string; quantity: number }[]) {
    // TODO: implement DB restore logic
    console.log('Restoring stock:', items);
  }
}