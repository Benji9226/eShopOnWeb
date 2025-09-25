import { Entity, Column, PrimaryGeneratedColumn } from 'typeorm';

@Entity('catalog_item_stock')
export class CatalogItemStock {
  @PrimaryGeneratedColumn()
  id: number;

  @Column('int')
  itemId: number;

  @Column('int')
  quantity: number;
}
