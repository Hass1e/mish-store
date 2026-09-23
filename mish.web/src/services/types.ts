

export interface ProductSummary {
  id: number;
  name: string;
  mainImageUrl: string;
  price: number;
  oldPrice?: number | null; // Может быть null, если старой цены нет
  isNew: boolean;
  isHit: boolean;
  rating: number;
  reviewCount: number;
}

export interface ProductDetail extends ProductSummary {
  description: string;
  imageUrls: string[];
  composition: string;
  tags: string[];
  weight: number;
  stock: number;
  sku: string;
  // ... сюда добавите остальные 30 полей по мере необходимости
}