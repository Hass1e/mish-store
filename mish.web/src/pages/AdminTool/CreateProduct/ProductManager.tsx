// src/pages/ProductManager.tsx
import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateProduct } from './CreateProduct'; // Импортируем твою форму
import { getManagedProducts } from '../../../services/productService'; // Нужен эндпоинт списка товаров для админа/продавца
import "./ProductManager.css"
interface ManagedProduct {
  id: number;
  name: string;
  sku?: string;
}

export function ProductManager() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  
  const [mode, setMode] = useState<'create' | 'select'>('create');
  const [products, setProducts] = useState<ManagedProduct[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [isLoading, setIsLoading] = useState(false);

 

  const loadProducts = async () => {
    setIsLoading(true);
    try {
      // Используем эндпоинт managed, который фильтрует по роли (Seller/Admin)
      const data = await getManagedProducts(1, 100); 
      setProducts(data.items || []);
    } catch (err) {
      console.error('Ошибка загрузки списка товаров:', err);
    } finally {
      setIsLoading(false);
    }
  };

   // Загружаем список доступных товаров при открытии страницы выбора
  useEffect(() => {
    if (mode === 'select') {
      loadProducts();
    }
  }, [mode]);

  // Фильтрация списка на фронтенде (для быстрого поиска по SKU/ID/Name)
  const filteredProducts = products.filter(p => 
    p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    (p.sku && p.sku.toLowerCase().includes(searchQuery.toLowerCase())) ||
    String(p.id).includes(searchQuery)
  );

  // Если в URL есть ID - сразу показываем форму редактирования
  if (id) {
    return (
      <div className="product-manager">
        <CreateProduct />
      </div>
    );
  }

  // Режим создания - просто рендерим форму
  if (mode === 'create') {
    return (
      <div className="product-manager">
        <div className="product-manager__header">
          <h1>Управление товарами</h1>
          <button onClick={() => setMode('select')} className="product-manager__secondary-button">
             Редактировать существующий
          </button>
        </div>
        <CreateProduct />
      </div>
    );
  }

  // Режим выбора товара для редактирования
  return (
    <div className="product-manager">
      <div className="product-manager__header">
        <h1>Выберите товар для редактирования</h1>
        <button onClick={() => setMode('create')} className="product-manager__secondary-button">
          ➕ Создать новый товар
        </button>
      </div>

      <div className="product-manager__search-bar">
        <input
          type="text"
          placeholder="Поиск по названию, SKU или ID..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="product-manager__search-input"
        />
      </div>

      <div className="products-list">
        {isLoading ? (
          <p>Загрузка товаров...</p>
        ) : filteredProducts.length === 0 ? (
          <p>Товары не найдены</p>
        ) : (
          filteredProducts.map(product => (
            <div 
              key={product.id} 
              className="product-manager__product-card"
              onClick={() => navigate(`/products/${product.id}/edit`)}
            >
              <div className="product-manager__product-info">
                <span className="product-manager__product-name">{product.name}</span>
                {product.sku && <span className="product-manager__product-sku">SKU: {product.sku}</span>}
                <span className="product-manager__product-id">ID: {product.id}</span>
              </div>
              <span className="product-manager__edit-arrow">→</span>
            </div>
          ))
        )}
      </div>
    </div>
  );
}