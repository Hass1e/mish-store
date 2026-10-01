import { useCallback, useEffect, useRef, useState } from 'react';
import { mapApiToProduct } from '../../components/ProductCard/mapApiToProduct';

interface ProductPage {
  items: Parameters<typeof mapApiToProduct>[0][];
  hasMore?: boolean;
  totalPages?: number;
}

export function useCatalog(
  categoryId: number | null,
  search: string,
  sort: string,
  categorySelected: boolean,
  categoryReady: boolean
) {
  const [items, setItems] = useState<ReturnType<typeof mapApiToProduct>[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isInitialLoad, setIsInitialLoad] = useState(true);
  const [hasMore, setHasMore] = useState(true);

  const observerRef = useRef<HTMLDivElement>(null);
  const filterKey = JSON.stringify([categoryId, search.trim(), sort, categorySelected]);
  const filterKeyRef = useRef(filterKey);
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const loadingRef = useRef(false);
  const hasMoreRef = useRef(true);
  const nextPageRef = useRef(2);
  const intersectionConsumedRef = useRef(false);
  filterKeyRef.current = filterKey;

  const loadPage = useCallback(async (page: number, append: boolean) => {
    if (loadingRef.current || filterKeyRef.current !== filterKey) return;

    const controller = new AbortController();
    controllerRef.current = controller;
    const requestId = ++requestIdRef.current;
    loadingRef.current = true;
    setIsLoading(true);

    try {
      const params = new URLSearchParams({
        page: page.toString(),
        size: '20',
      });
      if (categoryId !== null) params.set('categoryId', categoryId.toString());
      if (search.trim()) params.set('search', search.trim());
      if (sort) params.set('sort', sort);

      const response = await fetch(`/api/products?${params}`, { signal: controller.signal });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);

      const data: ProductPage = await response.json();
      if (requestIdRef.current !== requestId || filterKeyRef.current !== filterKey) return;

      const mappedItems = data.items.map(mapApiToProduct);
      setItems(previous => append ? [...previous, ...mappedItems] : mappedItems);

      const more = data.hasMore ?? page < (data.totalPages ?? page);
      hasMoreRef.current = more;
      setHasMore(more);
      nextPageRef.current = page + 1;
    } catch (error) {
      if (!(error instanceof DOMException && error.name === 'AbortError')) {
        console.error('Ошибка загрузки товаров:', error);
      }
    } finally {
      if (requestIdRef.current === requestId) {
        loadingRef.current = false;
        setIsLoading(false);
        setIsInitialLoad(false);
      }
    }
  }, [categoryId, filterKey, search, sort]);

  useEffect(() => {
    controllerRef.current?.abort();
    requestIdRef.current += 1;
    loadingRef.current = false;
    hasMoreRef.current = true;
    nextPageRef.current = 2;
    intersectionConsumedRef.current = false;

    setItems([]);
    setHasMore(true);
    setIsInitialLoad(true);

    if (!categoryReady) return;

    if ((categorySelected && categoryId === null) || (search.trim() && search.trim().length < 3)) {
      hasMoreRef.current = false;
      setHasMore(false);
      setIsLoading(false);
      setIsInitialLoad(false);
      return;
    }

    void loadPage(1, false);

    return () => controllerRef.current?.abort();
  }, [categoryId, categoryReady, categorySelected, filterKey, loadPage, search]);

  const loadNextPage = useCallback(() => {
    if (!hasMoreRef.current || loadingRef.current) return;
    void loadPage(nextPageRef.current, true);
  }, [loadPage]);

  useEffect(() => {
    const observer = new IntersectionObserver(
      entries => {
        const entry = entries[0];
        if (!entry?.isIntersecting) {
          intersectionConsumedRef.current = false;
          return;
        }

        if (intersectionConsumedRef.current || !hasMoreRef.current || loadingRef.current) return;

        intersectionConsumedRef.current = true;
        loadNextPage();
      },
      { threshold: 0.1 }
    );

    if (observerRef.current) observer.observe(observerRef.current);
    return () => observer.disconnect();
  }, [loadNextPage]);

  return { items, isLoading, hasMore, observerRef, isInitialLoad, loadNextPage };
}
