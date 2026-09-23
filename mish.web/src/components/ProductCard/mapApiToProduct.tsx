
// Тип данных, который ПРИХОДИТ из ASP.NET
// Тип данных, который ПРИХОДИТ из ASP.NET (для КАТАЛОГА)
interface ApiProduct {
    id: number;
    name: string;
    price: number;
    mainImageUrl: string;      
    oldPrice?: number | null;   
    isNew: boolean;             
    isHit: boolean;             
    rating: number;             // ⚠️ Важно: rating приходит как decimal, но в JS мы используем number (float)
    // description здесь НЕТ, так как это Summary DTO
}

// Тип данных, который НУЖЕН карточке
interface ProductCardData {
    id: number;
    title: string;
    price: number;
    oldPrice?: number | null;
    images: { id: number; url: string; alt: string }[]; // ✅ Массив объектов (для слайдера/галереи)
    link: string;
    // Можно добавить isNew/isHit/rating сюда же, если компонент их использует
}

export function mapApiToProduct(api: ApiProduct): ProductCardData {
      console.log('[MAPPER] Вход:', api.name, 'oldPrice=', api.oldPrice);
    return {
        id: api.id,
        title: api.name,                    // Name → title
        price: api.price,
        oldPrice: api.oldPrice ?? null,
        images: [{
            id: 1,
            url: api.mainImageUrl || '/uploads/placeholder.jpg',
            alt: api.name
        }],
        link: `/product/${api.id}`          // Генерируем ссылку
    };
}