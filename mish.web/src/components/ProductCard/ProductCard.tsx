import { useState } from 'react';
import { Link } from 'react-router-dom';
import './ProductCard.css';
import { ENDPOINTS } from '../../config.tsx';
import { ProductCartActions } from '../ProductCartActions/ProductCartActions';

export interface ProductImage {
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
  const [currentImgIndex, setCurrentImgIndex] = useState(0);

  const currentImage = images[currentImgIndex] || images[0];
  const imageUrl = currentImage?.url
    ? (currentImage.url.startsWith('http')
        ? currentImage.url
        : `${ENDPOINTS.UPLOADS}${currentImage.url.replace('/uploads', '')}`)
    : `${ENDPOINTS.UPLOADS}/placeholder.jpg`;

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

          <ProductCartActions
            productId={id}
            productName={title}
            price={price}
            imageUrl={currentImage?.url}
            variant="card"
          />
        </div>
      </Link>
    </article>
  );
}