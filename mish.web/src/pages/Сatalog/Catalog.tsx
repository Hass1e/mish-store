import { useParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useDebounce } from '../../hooks/useDebounce';
import { ProductCard } from "../../components/ProductCard/ProductCard.tsx";
import { ProductCardSkeleton } from '../../components/ProductCard/ProductCardSkeleton';
import { CatalogSidebar } from './CatalogSidebar';
import { CatalogFilters } from './CatalogFilters';
import { useCatalog } from './useCatalog';
import './Catalog.css';

// Вспомогательная функция для поиска ID по slug в дереве
function findCategoryIdBySlug(tree: any[], parentSlug?: string, subSlug?: string): number | null {
  if (!parentSlug) return null;
  
  const parent = tree.find(c => c.slug === parentSlug);
  if (!parent) return null;
  
  if (!subSlug) return parent.id; // Выбрана только родительская категория
  
  const sub = parent.children.find((c: any) => c.slug === subSlug);
  return sub ? sub.id : null;
}

export function Catalog() {
  const { parentSlug, subSlug } = useParams();
  
  const [categories, setCategories] = useState<any[]>([]);
  const [categoriesLoaded, setCategoriesLoaded] = useState(false);
  const [search, setSearch] = useState('');
  const [sort, setSort] = useState('');
  const debouncedSearch = useDebounce(search, 500);
  
  // Загружаем дерево категорий один раз при монтировании
  useEffect(() => {
    fetch('/api/categories/tree')
      .then(res => res.json())
      .then(data => setCategories(data))
      .catch(err => console.error('Ошибка загрузки категорий:', err))
      .finally(() => setCategoriesLoaded(true));
  }, []);

  // Находим ID категории на основе URL
  const categoryId = findCategoryIdBySlug(categories, parentSlug, subSlug);
  const categorySelected = Boolean(parentSlug);
  const categoryReady = !categorySelected || categoriesLoaded;

  // Используем наш умный хук с бесконечным скроллом и сбросом
  const { items, isLoading, hasMore, observerRef, isInitialLoad, loadNextPage } = useCatalog(
    categoryId, 
    debouncedSearch.trim(),
    sort,
    categorySelected,
    categoryReady
  );

  return (
    <div className="catalog-page">
      <CatalogSidebar categories={categories} />
      
      <main className="catalog-main">
        <CatalogFilters 
          search={search} 
          setSearch={setSearch} 
          sort={sort} 
          setSort={setSort} 
        />

        <section className="catalog-grid">
          {isInitialLoad && Array.from({ length: 8 }).map((_, i) => (
            <ProductCardSkeleton key={`init-${i}`} />
          ))}

          {!isInitialLoad && items.map((product) => (
            <ProductCard key={product.id} {...product} />
          ))}
          
          <div ref={observerRef} style={{ height: '1px' }} />
          
          {isLoading && !isInitialLoad && (
            <ProductCardSkeleton />
          )}
        </section>

        {!hasMore && items.length > 0 && (
          <div className="end-catalog">Все товары загружены</div>
        )}

        {hasMore && items.length > 0 && (
          <button
            type="button"
            className="catalog-load-more"
            onClick={loadNextPage}
            disabled={isLoading}
          >
            {isLoading ? 'Загрузка...' : 'Загрузить ещё'}
          </button>
        )}

        {items.length === 0 && !isLoading && !isInitialLoad && (
          <div className="empty-catalog">
            {search.trim().length > 0 && search.trim().length < 3 
              ? "Введите минимум 3 символа для поиска" 
              : "В этой категории пока нет товаров"}
          </div>
        )}
      </main>
    </div>
  );
}