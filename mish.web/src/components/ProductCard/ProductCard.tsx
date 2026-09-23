import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './ProductCard.css';
import { ENDPOINTS } from '../../config.tsx';
import { useCart } from '../../hooks/useCart'; // <-- Импортируем хук

interface ProductImage {
  id: number;
  url: string;
  alt: string;
}

interface ProductCardProps {
  id: number;
  title: string;
  price: number;
  oldPrice?: number | null;
  images: ProductImage[];
  link: string;
}

export function ProductCard({
  id,
  title,
  price,
  oldPrice,
  images,
  link
}: ProductCardProps) {
  const navigate = useNavigate();
  const { addToCart, removeFromCart, isInCart } = useCart(); // <-- Используем хук
  const [currentImgIndex, setCurrentImgIndex] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const isAdded = isInCart(id); // <-- Проверяем через хук

  const currentImage = images[currentImgIndex] || images[0];
  const imageUrl = currentImage?.url
    ? (currentImage.url.startsWith('http')
        ? currentImage.url
        : `${ENDPOINTS.UPLOADS}${currentImage.url.replace('/uploads', '')}`)
    : `${ENDPOINTS.UPLOADS}/placeholder.jpg`;

  const handleAddToCart = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (isLoading) return;

    setIsLoading(true);
    try {
      // Передаем данные товара, чтобы гостевая корзина могла посчитать сумму
      await addToCart(id, 1, {
        productName: title,
        price: price,
        imageUrl: currentImage?.url
      });
    } catch (error) {
      console.error('Ошибка при добавлении в корзину:', error);
      alert('Не удалось добавить товар');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemoveFromCart = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (isLoading) return;

    setIsLoading(true);
    try {
      await removeFromCart(id);
    } catch (error) {
      console.error('Ошибка при удалении из корзины:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoToCart = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    navigate('/cart');
  };

  return (
    <article className="product-card">
      <Link to={link} className="product-card__link">
        <div className="product-card__image-wrapper">
          <img src={imageUrl} alt={currentImage?.alt || title} className="product-card__image" />
          
          {images.length > 1 && (
            <>
              <button type="button" className="product-card__arrow product-card__arrow--prev" onClick={(e) => { e.preventDefault(); e.stopPropagation(); setCurrentImgIndex((prev) => prev === 0 ? images.length - 1 : prev - 1); }}>‹</button>
              <button type="button" className="product-card__arrow product-card__arrow--next" onClick={(e) => { e.preventDefault(); e.stopPropagation(); setCurrentImgIndex((prev) => (prev + 1) % images.length); }}>›</button>
            </>
          )}
        </div>

        <div className="product-card__info">
          <h3 className="product-card__title">{title}</h3>

          <div className="product-card__price-block">
            <span className="product-card__price">{price.toLocaleString('ru-RU')} ₽</span>
            {oldPrice && oldPrice > 0 && (
              <span className="product-card__old-price">{oldPrice.toLocaleString('ru-RU')} ₽</span>
            )}
          </div>

          <div className="product-card__actions">
            <button
              type="button"
              className={`product-card__add-btn ${isAdded ? 'product-card__add-btn--added' : ''}`}
              onClick={isAdded ? handleGoToCart : handleAddToCart}
              disabled={isLoading}
            >
              {isLoading ? '...' : (isAdded ? 'В корзине' : 'В корзину')}
            </button>

            {isAdded && (
              <button
                type="button"
                className="product-card__remove-btn"
                onClick={handleRemoveFromCart}
                disabled={isLoading}
                aria-label="Удалить из корзины"
              >
                <img src="/trash.png" alt="Удалить" className="product-card__remove-icon" />
              </button>
            )}
          </div>
        </div>
      </Link>
    </article>
  );
}