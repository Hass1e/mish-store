// CategoryTreeEditor.tsx
import React, { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { categoryApi, type CreateCategoryRequest, type CategoryResponse } from './CategoryApi';
import './CategoryTreeEditor.css';


// --- Логика работы с деревом (Immutable) ---

// Функция добавления узла в дерево. 
// Она создает НОВЫЕ объекты только на пути к родителю, остальное остается прежним.
const addNodeToTree = (
  tree: CategoryResponse[], 
  parentId: number | null, 
  newNode: CategoryResponse
): CategoryResponse[] => {
  // 1. Если родителя нет (null) - добавляем в корень
  if (parentId === null) {
    return [...tree, newNode];
  }

  // 2. Ищем родителя в текущем уровне
  return tree.map((node) => {
    if (node.id === parentId) {
      // Нашли! Возвращаем копию узла с обновленным массивом children
      return {
        ...node,
        children: [...node.children, newNode],
      };
    }
    
    // 3. Если не нашли, рекурсивно идем глубже в children
    // Важно: если children пустой, map не вызовется, и мы вернем node как есть
    if (node.children && node.children.length > 0) {
      return {
        ...node,
        children: addNodeToTree(node.children, parentId, newNode),
      };
    }
    
    return node;
  });
};

// --- Компонент Узла (Рекурсивный) ---

interface TreeNodeProps {
  node: CategoryResponse;
  level: number; // Уровень вложенности для отступа
  onCreated: (parentId: number | null, newNode: CategoryResponse) => void;
}

const MAX_NAME_LENGTH = 100;
const MAX_SLUG_LENGTH = 50;
const MAX_CHILDREN = 999;

const TreeNode: React.FC<TreeNodeProps> = ({ node, level, onCreated }) => {
  const [isAdding, setIsAdding] = useState(false);
  const [name, setName] = useState('');
  const [slug, setSlug] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const canAddChild = level === 0 && node.children.length < MAX_CHILDREN;

  const handleSave = async () => {
    if (!name.trim() || !slug.trim()) {
      setError('Заполните название и Slug');
      return;
    }
    if (name.trim().length > MAX_NAME_LENGTH || slug.trim().length > MAX_SLUG_LENGTH) {
      setError('Название — максимум 100 символов, Slug — максимум 50 символов');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const payload: CreateCategoryRequest = {
        name: name.trim(),
        slug: slug.trim(),
        parentId: node.id, // Текущий узел становится родителем
      };

      const newCategory = await categoryApi.create(payload);
      
      // Сообщаем наверх, что создан новый узел
      onCreated(node.id, newCategory);
      
      setName('');
      setSlug('');
      setIsAdding(false);
      toast.success(`Подкатегория "${newCategory.name}" создана`);
    } catch (err: any) {
      setError(err.message);
      toast.error(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="tree-node-wrapper">
      {/* Строка самого узла */}
      <div className={`node-row${level > 0 ? ' node-row--subcategory' : ''}`}>
        <span className="node-name">{node.name}</span>
        
        {canAddChild && (
          <button 
            className="add-btn" 
            onClick={() => setIsAdding(!isAdding)}
            title="Добавить подкатегорию"
            type="button"
          >
            +
          </button>
        )}
      </div>

      {isAdding && (
        <div className="inline-form-container">
          <div className="inline-form">
            <input
              className="input-field"
              placeholder="Название"
              value={name}
              onChange={(e) => setName(e.target.value)}
              maxLength={MAX_NAME_LENGTH}
              autoFocus
            />
            <input
              className="input-field"
              placeholder="Slug"
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
              maxLength={MAX_SLUG_LENGTH}
            />
            <button 
              className="save-btn" 
              onClick={handleSave}
              disabled={isLoading}
              type="button"
            >
              {isLoading ? '...' : 'OK'}
            </button>
            <button 
               className="cancel-btn" 
               onClick={() => setIsAdding(false)}
               type="button"
            >
              ✕
            </button>
          </div>
          {error && <div className="error-text">{error}</div>}
        </div>
      )}

      {node.children.length > 0 && (
        <div className="subcategory-list" aria-label={`Подкатегории ${node.name}`}>
          {node.children.map(child => (
            <TreeNode
              key={child.id}
              node={child}
              level={level + 1}
              onCreated={onCreated}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// --- Главный Компонент Страницы ---

export const CategoryTreeEditor: React.FC = () => {
  const [tree, setTree] = useState<CategoryResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  // Состояние для корневой категории
  const [isRootAdding, setIsRootAdding] = useState(false);
  const [rootName, setRootName] = useState('');
  const [rootSlug, setRootSlug] = useState('');
  const [rootError, setRootError] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const data = await categoryApi.getTree();
        setTree(data);
      } catch (err: any) {
        toast.error(err.message || 'Ошибка загрузки категорий');
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  const handleRootSave = async () => {
    if (!rootName.trim() || !rootSlug.trim()) {
      setRootError('Заполните все поля');
      return;
    }

    try {
      const newRoot = await categoryApi.create({
        name: rootName.trim(),
        slug: rootSlug.trim(),
        parentId: null,
      });
      
      setTree((prev) => [...prev, newRoot]);
      
      setRootName('');
      setRootSlug('');
      setIsRootAdding(false);
      toast.success(`Категория "${newRoot.name}" создана`);
    } catch (err: any) {
      setRootError(err.message);
      toast.error(err.message);
    }
  };

  const handleNodeCreated = (parentId: number | null, newNode: CategoryResponse) => {
    setTree((prevTree) => addNodeToTree(prevTree, parentId, newNode));
  };

  if (isLoading) return <div className="loading-state">Загрузка структуры...</div>;

  return (
    <div className="category-tree-editor">
      <div className="tree-list">
        {tree.length === 0 && !isRootAdding && (
          <p className="empty-state">Нет категорий. Создайте первую!</p>
        )}
        
        {/* Рендерим только корневые узлы. Они сами отрендерят своих детей */}
        {tree.map(node => (
          <TreeNode 
            key={node.id} 
            node={node} 
            level={0} 
            onCreated={handleNodeCreated} 
          />
        ))}
      </div>

      {/* Кнопка добавления корневой категории */}
      <div className="root-actions">
        {!isRootAdding ? (
          <button 
            className="add-root-btn" 
            onClick={() => setIsRootAdding(true)}
            type="button"
          >
            + Добавить корневую категорию
          </button>
        ) : (
          <div className="inline-form root-form">
            <input
              className="input-field"
              placeholder="Название категории"
              value={rootName}
              onChange={(e) => setRootName(e.target.value)}
              maxLength={MAX_NAME_LENGTH}
              autoFocus
            />
            <input
              className="input-field"
              placeholder="Slug"
              value={rootSlug}
              onChange={(e) => setRootSlug(e.target.value)}
              maxLength={MAX_SLUG_LENGTH}
            />
            <button className="save-btn" onClick={handleRootSave} type="button">
              Создать
            </button>
            <button className="cancel-btn" onClick={() => setIsRootAdding(false)} type="button">
              
            </button>
          </div>
        )}
        {rootError && <div className="error-text">{rootError}</div>}
      </div>
    </div>
  );
};