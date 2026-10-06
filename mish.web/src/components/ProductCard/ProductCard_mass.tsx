import { ProductCard } from "./ProductCard";
import { ProductCardSkeleton } from './ProductCardSkeleton'; 
import { useInfiniteScroll } from "../../hooks/useInfiniteScroll";
import { mapApiToProduct } from "./mapApiToProduct.tsx";
import { apiFetch } from "../../utils/api";
import './ProductCard_message.css';


async function fetchProducts(page: number, size: number) {
    try {
        const res = await apiFetch(`/api/products?page=${page}&size=${size}`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);

        const data = await res.json();
        const mappedItems = (data.items || []).map(mapApiToProduct);

        return {
            data: mappedItems,
            total: data.total || mappedItems.length
        };
    } catch (error) {
        console.error('Ошибка загрузки товаров:', error);
        return { data: [], total: 0 };
    }
}

export function Catalog() {
    // ← ДОБАВЬ isInitialLoad в деструктуризацию
    const { items, isLoading, hasMore, observerRef, isInitialLoad } = useInfiniteScroll(fetchProducts, 50);

    return (
        <>
        <section className="catalog">
            {/* 1. ПЕРВАЯ ЗАГРУЗКА: 8 скелетонов на весь экран */}
            {isInitialLoad && Array.from({ length: 8 }).map((_, i) => (
                <ProductCardSkeleton key={`init-${i}`} />
            ))}

            {/* 2. РЕАЛЬНЫЕ ТОВАРЫ */}
            {!isInitialLoad && items.map((product) => (
                <ProductCard 
                    key={product.id} 
                    {...product} 
                />
            ))}
            
            {/* Невидимый триггер для подгрузки */}
            <div ref={observerRef} style={{ height: '1px' }} />
            
            {/* 3. ПОДГРУЗКА ПРИ СКРОЛЛЕ: Один скелетон вместо текста */}
            {isLoading && !isInitialLoad && (
                <ProductCardSkeleton />
            )}
            
 
        </section>
                   {/* Сообщение когда всё загружено */}
            {!hasMore && items.length > 0 && (
                <div className="end-catalog">Все товары загружены</div>
            )}

            {/* Если товаров вообще нет */}
            {items.length === 0 && !isLoading && !isInitialLoad && (
                <div className="empty-catalog">Каталог пуст</div>
            )}
            </>
    );
}