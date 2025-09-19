import { Module } from '@nestjs/common';
import { StockService } from './stock.service';
import { StockController } from './stock.controller';
import { StockEventsHandler } from './events/stock-events.handler';

@Module({
  imports: [],
  controllers: [StockController],
  providers: [StockService, StockEventsHandler],
})
export class StockModule {}
