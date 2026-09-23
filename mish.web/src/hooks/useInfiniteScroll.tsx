//useInfiniteScroll.tsx
import { useEffect, useRef, useState } from 'react';

// Тип для ответа от сервера (можно вынести в types.ts)
interface FetchResponse {
    data: any[];
    total: number;
}

export function useInfiniteScroll(
    fetchData: (page: number, size: number) => Promise<FetchResponse>, 
    pageSize: number = 50
) {
 
    



    const [items, setItems] = useState<any[]>([]); 
    const [page, setPage] = useState(1);
    const [isLoading, setIsLoading] = useState(false);
    const [isInitialLoad, setIsInitialLoad] = useState(true);
    const [hasMore, setHasMore] = useState(true);
    
    const observerRef = useRef<HTMLDivElement | null>(null);


    // Внутри useInfiniteScroll, перед return { items, ... }
console.log('[HOOK] Возвращаем items:', items.map(i => ({ id: i.id, title: i.title, oldPrice: i.oldPrice })));
    useEffect(() => {
        if (!hasMore || isLoading) return;

        const loadMore = async () => {
            setIsLoading(true);
            try {
                const response = await fetchData(page, pageSize);
                
                // Добавляем новые товары к старым
               // Внутри useEffect, после получения response:
            setItems(prev => {
            // 1. Фильтруем новые товары: оставляем только те, которых НЕТ в prev
            const uniqueNewItems = response.data.filter(
                newItem => !prev.some(existing => existing.id === newItem.id)
            );
            
            // 2. Добавляем только уникальные
            return [...prev, ...uniqueNewItems];
});
                
                // Проверяем, есть ли еще страницы
                const totalPages = Math.ceil(response.total / pageSize);
                setHasMore(page < totalPages);
                
                // Готовимся к следующей странице
                setPage(prev => prev + 1);
            } catch (error) {
                console.error('Ошибка загрузки:', error);
            } finally {
                setIsLoading(false);
                 if (isInitialLoad) setIsInitialLoad(false);
            }
        };

        loadMore();
    }, [page]); // Перезапускается, когда меняется page

    // Настройка Observer'а (следит за низом списка)
    useEffect(() => {
        const observer = new IntersectionObserver(
            (entries) => {
                if (entries[0].isIntersecting && hasMore && !isLoading) {
                    setPage(prev => prev + 1);
                }
            },
            { rootMargin: '200px' } // Грузим заранее, за 200px до конца
        );

        const currentRef = observerRef.current;
        if (currentRef) {
            observer.observe(currentRef);
        }

        return () => {
            if (currentRef) observer.unobserve(currentRef);
        };
    }, [hasMore, isLoading]);

    return { items, isLoading, hasMore, observerRef, isInitialLoad };
}