import { useEffect, useState } from 'react';
import './ProductDetail.css';
import { getProductDetail } from '../../services/productService';
import type { ProductDetail as ProductDetailData } from '../../services/types';
import { ENDPOINTS } from '../../config';
import { ProductCartActions } from '../ProductCartActions/ProductCartActions';

interface ProductDetailProps {
  productId: number;
}

export function ProductDetail({ productId }: ProductDetailProps) {
  const [product, setProduct] = useState<ProductDetailData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getProductDetail(productId);

        if (isMounted) {
          setProduct(data as unknown as ProductDetailData);
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
    } else {
      setLoading(false);
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
          <p>{error.message}</p>
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

  return <LoadedProductDetail product={product} />;
}

function LoadedProductDetail({ product }: { product: ProductDetailData }) {
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  const rawImages = product.imageUrls?.length ? product.imageUrls : [product.mainImageUrl];
  const images = rawImages
    .filter((url): url is string => Boolean(url))
    .map(resolveProductImageUrl);
  const safeImages = images.length > 0 ? images : [`${ENDPOINTS.UPLOADS}/placeholder.jpg`];
  const currentImage = safeImages[currentImageIndex] ?? safeImages[0];

  const nextImage = () => setCurrentImageIndex((prev) => (prev + 1) % safeImages.length);
  const prevImage = () => setCurrentImageIndex((prev) => (prev - 1 + safeImages.length) % safeImages.length);

  return (
    <div className="product-page">
      <div className="product-grid">
        <div className="island-card">
          <div className="carousel-container">
            <img src={currentImage} alt={product.name} className="carousel-image" />
            {safeImages.length > 1 && (
              <>
                <button type="button" className="carousel-btn prev" onClick={prevImage} aria-label="Предыдущее фото">
                  &#10094;
                </button>
                <button type="button" className="carousel-btn next" onClick={nextImage} aria-label="Следующее фото">
                  &#10095;
                </button>
              </>
            )}
          </div>

          {safeImages.length > 1 && (
            <div className="thumbnails">
              {safeImages.map((image, index) => (
                <button
                  type="button"
                  key={image}
                  className={`thumbnail${currentImageIndex === index ? ' active' : ''}`}
                  onClick={() => setCurrentImageIndex(index)}
                  aria-label={`Показать фото ${index + 1}`}
                >
                  <img src={image} alt="" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="island-card product-info">
          <h1 className="product-title">{product.name}</h1>

          <div className="rating-block">
            <span className="stars">★ {product.rating}</span>
            <span>|</span>
            <span>{product.reviewCount || 0} отзывов</span>
          </div>

          <div className="price-block">
            {product.oldPrice != null && product.oldPrice > 0 && (
              <span className="old-price">{product.oldPrice.toLocaleString('ru-RU')} ₽</span>
            )}
            <span className="current-price">{product.price.toLocaleString('ru-RU')} ₽</span>
          </div>

          <p className="product-description">{product.description}</p>

          <ProductCartActions
            productId={product.id}
            productName={product.name}
            price={product.price}
            imageUrl={currentImage}
            variant="detail"
          />

          <div className="stock-status">
            <span>●</span>
            <span>В наличии: {product.stock} шт. (Артикул: {product.sku})</span>
          </div>
        </div>
      </div>

      <div className="island-card specs-island" style={{ marginTop: '32px' }}>
        <h2 className="specs-title">Характеристики</h2>
        <div className="specs-grid">
          <div className="spec-item">
            <span className="spec-label">Артикул (SKU)</span>
            <span className="spec-value">{product.sku}</span>
          </div>
          <div className="spec-item">
            <span className="spec-label">Вес</span>
            <span className="spec-value">{product.weight} грамм</span>
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

function resolveProductImageUrl(url: string): string {
  if (/^(https?:|blob:|data:)/i.test(url)) return url;

  if (url.startsWith('/uploads/')) {
    return `${ENDPOINTS.UPLOADS}${url.slice('/uploads'.length)}`;
  }

  return `${ENDPOINTS.UPLOADS}/${url.replace(/^\/+/, '')}`;
}

export default ProductDetail;
