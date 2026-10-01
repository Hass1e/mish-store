// Тип данных, который ПРИХОДИТ из ASP.NET
interface ApiProduct {
    id: number;
    name?: string;         // Может прийти как name
    productName?: string;  // Или как productName (стандарт .NET)
    price: number;
    mainImageUrl?: string;      
    imageUrls?: string[];  // На случай, если бэк отдает массив
    oldPrice?: number | null;   
    isNew: boolean;             
    isHit: boolean;             
    rating: number;
    categoryId?: number;   // Добавили на всякий случай
}

// Тип данных, который НУЖЕН карточке
export interface ProductCardData {
    id: number;
    title: string;
    price: number;
    oldPrice?: number | null;
    images: { id: number; url: string; alt: string }[];
    link: string;
    isNew?: boolean;
    isHit?: boolean;
    rating?: number;
}

export function mapApiToProduct(api: ApiProduct): ProductCardData {
    // Берем имя из любого возможного поля
    const title = api.name || api.productName || 'Без названия';
    
    // Берем картинку: либо одну главную, либо первую из массива, либо заглушку
    const imageUrl = api.mainImageUrl || (api.imageUrls && api.imageUrls[0]) || '/placeholder.jpg';

    return {
        id: api.id,
        title: title,
        price: api.price,
        oldPrice: api.oldPrice ?? null,
        images: [{
            id: 1,
            url: imageUrl,
            alt: title
        }],
        link: `/product/${api.id}`,
        isNew: api.isNew,
        isHit: api.isHit,
        rating: api.rating
    };
}