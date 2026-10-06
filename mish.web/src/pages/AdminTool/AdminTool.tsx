      import './AdminTool.css';
      import { Link } from 'react-router-dom';

      export function AdminTool() {
        return (
          <div style={{ minHeight: '80vh' }} className="Admin-tool-container">
            <div className="admin-tool">
              <h1>Admin Tool</h1>
              <p>Добро пожаловать в панель администратора!</p>
              <p>Здесь вы можете управлять продуктами, пользователями и другими аспектами приложения.</p>
              <p>Выберите действие из меню ниже:</p>
              <div className="text">
                <li>
                  <Link to="/products/create">Создание товара</Link>
                </li>
                <li>
                  <Link to="/change-roles" target="_blank" rel="noreferrer">Смена роли</Link>
                </li>
                <li>
                  <a href="/swagger" target="_blank" rel="noreferrer">Swagger</a>
                </li>
                <li>
                  <a href="/api/auth/debug-cookies" target="_blank" rel="noreferrer">Debug Cookies</a>
                </li>
                <li>
                  <Link to="/Change-order" target="_blank" rel="noreferrer">Изменение статуса заказов</Link>
                </li>
                 <li>
                  <Link to="/Create/categories" target="_blank" rel="noreferrer">Создание новых категорий</Link>
                </li>
              </div>
            </div>
          </div>
        );
      }
