import { useState, useEffect } from 'react';
import './ProductDetail.css';
import { getProductDetail } from '../../services/productService';
import type { ProductDetail } from '../../services/types';
import { ENDPOINTS } from '../../config'; // ✅ Добавили импорт конфига

interface ProductDetailProps {
  productId: number;
}

export function ProductDetail({ productId }: ProductDetailProps) {
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  useEffect(() => {
    let isMounted = true;

    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getProductDetail(productId);
        
        if (isMounted) {
          setProduct(data as unknown as ProductDetail);
        }
      } catch (err) {
        if (isMounted) {
          setError(err instanceof Error ? err : new Error('Не удалось загрузить товар'));
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    if (productId) {
      loadData();
    }

    return () => {
      isMounted = false;
    };
  }, [productId]);

  if (loading) {
    return <div className="product-page"><div className="loading-spinner">Загрузка данных...</div></div>;
  }
  
  if (error) {
    return (
      <div className="product-page">
        <div className="island-card" style={{ padding: '32px', textAlign: 'center', color: '#dc2626' }}>
          <h2>Не удалось загрузить товар</h2>
          <p>{error?.message}</p>
          <button onClick={() => window.location.reload()} className="buy-button" style={{ marginTop: '16px', maxWidth: '200px' }}>
            Попробовать снова
          </button>
        </div>
      </div>
    );
  }

  if (!product) {
    return <div className="product-page">Товар не найден</div>;
  }

  // ✅ ИСПРАВЛЕННАЯ ЛОГИКА: Нормализуем пути ДО рендера
const images: string[] = (() => {
  const rawImages = product.imageUrls && product.imageUrls.length > 0 
    ? product.imageUrls 
    : [product.mainImageUrl];

  return rawImages.map(url => {
    if (!url) return null;
    // Если URL уже полный (http/https) - оставляем как есть
    if (url.startsWith('http')) return url;
    // Если относительный путь УЖЕ начинается с /uploads - просто добавляем базу
    if (url.startsWith('/uploads')) return `${ENDPOINTS.UPLOADS}${url.replace(/^\/uploads\/?/, '/')}`;
    // Для остальных случаев добавляем полный путь к uploads
    return `${ENDPOINTS.UPLOADS}/${url.replace(/^\//, '')}`;
  }).filter((url): url is string => url !== null);
})();

const safeImages = images.length > 0 ? images : [`${ENDPOINTS.UPLOADS}/placeholder.jpg`];

  const nextImage = () => setCurrentImageIndex((prev) => (prev + 1) % safeImages.length);
  const prevImage = () => setCurrentImageIndex((prev) => (prev - 1 + safeImages.length) % safeImages.length);

  return (
    <div className="product-page">
      <div className="product-grid">
        
        {/* ЛЕВЫЙ ОСТРОВОК: Карусель */}
        <div className="island-card">
          <div className="carousel-container">
            <img
              src={safeImages[currentImageIndex]}
              alt={product.name} 
              className="carousel-image" 
            />
            {safeImages.length > 1 && (
              <>
                <button className="carousel-btn prev" onClick={prevImage} aria-label="Предыдущее фото">
                  &#10094;
                </button>
                <button className="carousel-btn next" onClick={nextImage} aria-label="Следующее фото">
                  &#10095;
                </button>
              </>
            )}
          </div>
          
          {safeImages.length > 1 && (
            <div className="thumbnails">
              {safeImages.map((img, idx) => (
                <div 
                  key={idx} 
                  className={`thumbnail ${currentImageIndex === idx ? 'active' : ''}`}
                  onClick={() => setCurrentImageIndex(idx)}
                >
                  <img src={img} alt={`Миниатюра ${idx + 1}`} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* ПРАВЫЙ ОСТРОВОК: Информация */}
        <div className="island-card product-info">
          <h1 className="product-title">{product.name}</h1>
          
          <div className="rating-block">
            <span className="stars">★ {product.rating}</span>
            <span>|</span>
            <span>{product.reviewCount || 0} отзывов</span>
          </div>

          <div className="price-block">
            {product.oldPrice && (
              <span className="old-price">{product.oldPrice.toLocaleString('ru-RU')} ₽</span>
            )}
            <span className="current-price">{product.price.toLocaleString('ru-RU')} ₽</span>
          </div>

          <p className="product-description">{product.description}</p>

          <button className="buy-button">
            Добавить в корзину
          </button>

          <div className="stock-status">
            <span>●</span>
            <span>В наличии: {product.stock} шт. (Артикул: {product.sku})</span>
          </div>
        </div>
      </div>

      {/* НИЖНИЙ ОСТРОВОК: Характеристики */}
      <div className="island-card specs-island" style={{ marginTop: '32px' }}>
        <h2 className="specs-title">Характеристики</h2>
        <div className="specs-grid">
          <div className="spec-item">
            <span className="spec-label">Артикул (SKU)</span>
            <span className="spec-value">{product.sku}</span>
          </div>
          <div className="spec-item">
            <span className="spec-label">Вес</span>
            <span className="spec-value">{product.weight} кг</span>
          </div>
          <div className="spec-item">
            <span className="spec-label">Состав / Материал</span>
            <span className="spec-value">{product.composition}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ProductDetail;