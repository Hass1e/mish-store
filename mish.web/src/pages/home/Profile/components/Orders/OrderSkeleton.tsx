export function OrderSkeleton() {
  return (
    <div className="order-card skeleton">
      <div className="skeleton-line w-40" />
      <div className="skeleton-line w-20 ml-auto" />
      <div className="skeleton-images">
        <div className="skeleton-img" />
        <div className="skeleton-img" />
        <div className="skeleton-img" />
      </div>
      <div className="skeleton-line w-30 mt-4" />
    </div>
  );
}