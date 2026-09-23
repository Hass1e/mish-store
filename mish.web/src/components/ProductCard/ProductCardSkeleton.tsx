// ProductCardSkeleton.tsx
export function ProductCardSkeleton() {
    return (
        <div className="product-card animate-pulse">
            {/* Заглушка фото */}
            <div className="product-card__image" style={{ backgroundColor: '#e5e7eb', height: '235px' }} />
            
            {/* Заглушка контента */}
            <div className="product-card__content" style={{ padding: '12px' }}>
                <div style={{ height: '16px', backgroundColor: '#e5e7eb', borderRadius: '4px', width: '70%', marginBottom: '8px' }} />
                <div style={{ height: '16px', backgroundColor: '#e5e7eb', borderRadius: '4px', width: '40%' }} />
            </div>
        </div>
    );
}