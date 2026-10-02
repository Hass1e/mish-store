// AdminTool/CreateProduct/CreateProduct.tsx
import { useCreateProduct } from './useCreateProduct';
import { ENDPOINTS } from '../../../config';
import './CreateProduct.css';

function resolveImageUrl(url: string): string {
  if (/^(https?:|blob:|data:)/i.test(url)) return url;
  const path = url.startsWith('/uploads/')
    ? url.slice('/uploads'.length)
    : `/${url.replace(/^\/+/, '')}`;
  return `${ENDPOINTS.UPLOADS}${path}`;
}

export function CreateProduct() {
  const {
    isEditMode,
    isLoading,
    isSubmitting,
    error,
    success,
    formData,
    categories,
    parentCatId,
    previews,
    imageError,
    handleChange,
    handleParentCategoryChange,
    handleImageChange,
    removeImage,
    handleSubmit,
    setSuccess
  } = useCreateProduct();

  if (isLoading) return <div className="create-page"><div className="form-island">Загрузка данных...</div></div>;

  if (success && !isEditMode) {
    return (
      <div className="create-page">
        <div className="form-island success-state">
          <h2 className="success-title">Товар успешно создан!</h2>
          <button onClick={() => setSuccess(false)} className="submit-btn secondary-btn">
            Создать еще один
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="create-page">
      <div className="form-island">
        <h1 className="form-title">{isEditMode ? 'Редактирование товара' : 'Новый товар'}</h1>
        
        {error && <div className="error-message">{error}</div>}
        {success && isEditMode && (
          <div className="success-message">
            Изменения сохранены. Перенаправляем...
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-group full-width">
              <label className="label">Название товара</label>
              <input type="text" name="name" value={formData.name} onChange={handleChange} minLength={6} maxLength={32} className="input" />
              <span className="char-counter">{formData.name.length}/32</span>
            </div>

            <div className="form-group">
              <label className="label">Цена (₽)</label>
              <input type="number" name="price" value={formData.price} onChange={handleChange} min="1" step="1" className="input" />
            </div>

            <div className="form-group">
              <label className="label">Старая цена</label>
              <input type="number" name="oldPrice" value={formData.oldPrice} onChange={handleChange} min="2" step="1" className="input" />
            </div>

            <div className="form-group full-width">
              <label className="label">Изображения</label>
              <div className="file-upload-area" onClick={() => document.getElementById('fileInput')?.click()}>
                <input id="fileInput" type="file" accept=".png,.jpg,.jpeg" multiple onChange={handleImageChange} className="hidden-input" />
                <div className="upload-placeholder">
                  <span className="upload-icon">📷</span>
                  <p>Нажмите для выбора файлов</p>
                </div>
              </div>
              {imageError && <div className="error-message">{imageError}</div>}
              
              <div className="preview-container">
                {previews.map((src, idx) => (
                  <div key={idx} className="preview-item">
                    <img src={resolveImageUrl(src)} alt={`preview-${idx}`} className="preview-image" />
                    <button type="button" onClick={() => removeImage(idx)} className="remove-btn">×</button>
                  </div>
                ))}
              </div>
            </div>

            <div className="form-group full-width">
              <label className="label">Описание</label>
              <textarea name="description" value={formData.description} onChange={handleChange} className="textarea" rows={4} />
            </div>

            <div className="form-group">
              <label className="label">Вес (г)</label>
              <input type="number" name="weight" step="1" value={formData.weight} onChange={handleChange} className="input" />
            </div>

            <div className="form-group">
              <label className="label">Остаток на складе</label>
              <input type="number" name="stock" step="1" value={formData.stock} onChange={handleChange} className="input" />
            </div>

            <div className="form-group full-width">
              <label className="label">Состав / Материал</label>
              <input type="text" name="composition" value={formData.composition} onChange={handleChange} className="input" />
            </div>

            <div className="form-group checkbox-group">
              <input type="checkbox" name="isNew" checked={formData.isNew} onChange={handleChange} id="isNew" className="checkbox-input" />
              <label htmlFor="isNew" className="label checkbox-label">Новинка</label>
            </div>

            <div className="form-group checkbox-group">
              <input type="checkbox" name="isHit" checked={formData.isHit} onChange={handleChange} id="isHit" className="checkbox-input" />
              <label htmlFor="isHit" className="label checkbox-label">Хит продаж</label>
            </div>

            <>
              <div className="form-group checkbox-group">
                <input type="checkbox" name="isActive" checked={formData.isActive} onChange={handleChange} id="isActive" className="checkbox-input" />
                <label htmlFor="isActive" className="label checkbox-label">Активен (виден в каталоге)</label>
              </div>
              
              <div className="form-group">
                <label className="label">Раздел</label>
                <select 
                  name="parentCategory" 
                  value={parentCatId} 
                  onChange={handleParentCategoryChange} 
                  className="input"
                >
                  <option value="">-- Выберите раздел --</option>
                  {categories.map(cat => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                </select>
            
                <label className="label">Категория</label>
                <select 
                  name="categoryId" 
                  value={formData.categoryId} 
                  onChange={handleChange} 
                  className="input"
                  disabled={!parentCatId}
                >
                  <option value="">-- Выберите категорию --</option>
                  {parentCatId && categories
                    .find(c => String(c.id) === parentCatId)
                    ?.children.map((child: any) => (
                      <option key={child.id} value={child.id}>
                        {child.name} ({child.id})
                      </option>
                    ))}
                </select>
              </div>
            </>
          </div>

          <button type="submit" disabled={isSubmitting} className="submit-btn">
            {isSubmitting ? (isEditMode ? 'Сохранение...' : 'Создание...') : (isEditMode ? 'Сохранить изменения' : 'Создать товар')}
          </button>
        </form>
      </div>
    </div>
  );
}

export default CreateProduct;