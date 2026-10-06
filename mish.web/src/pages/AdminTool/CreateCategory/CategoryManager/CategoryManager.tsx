// CategoryManager.tsx
import React from 'react';
import { CategoryTreeEditor } from '../CategoryTreeEditor'; // Путь к твоему компоненту
import './CategoryManager.css'; // Опционально: стили для самой страницы

export const CategoryManager: React.FC = () => {
  return (
    <div className="category-manager-page">
      <header className="page-header">
        <h1>Управление категориями</h1>
        <p className="page-subtitle">
          Создавайте и редактируйте структуру каталога. 
          Изменения применяются мгновенно.
        </p>
      </header>
      
      <main className="page-content">
        <CategoryTreeEditor />
      </main>
    </div>
  );
};