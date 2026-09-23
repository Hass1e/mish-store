import './Order.css';

export function Orders() {
  return (
    <div className="orders">
      <h2 className="section-title">Мои заказы</h2>
      <div className="empty-state">
        <p>У вас пока нет заказов</p>
      </div>
    </div>
  );
}