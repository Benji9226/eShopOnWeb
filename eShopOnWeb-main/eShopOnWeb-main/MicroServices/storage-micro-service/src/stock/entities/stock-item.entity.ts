import { Entity, Column, PrimaryGeneratedColumn } from 'typeorm';

@Entity('stock_items')
export class StockItem {
  @PrimaryGeneratedColumn('uuid')
  id: string;

  @Column()
  itemId: string;

  @Column('int')
  quantity: number;
}