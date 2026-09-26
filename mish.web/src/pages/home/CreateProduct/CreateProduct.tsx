import { useState, useEffect, type ChangeEvent, type FormEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { createProduct, updateProduct, getProductForEdit } from '../../../services/productService';
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
  const { id } = useParams<{ id: string }>(); // Если есть ID - режим редактирования
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(isEditMode); // Загружаем данные только в режиме edit
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Состояние формы
  const [formData, setFormData] = useState({
    name: '',
    price: '',
    oldPrice: '',
    description: '',
    composition: '',
    weight: '',
    stock: '',
    isNew: false,
    isHit: false,
    isActive: true, // Поле для админки
    categoryId: ''
  });

  // Гибридное состояние изображений: URL (старые) + File (новые)
  const [existingImageUrls, setExistingImageUrls] = useState<string[]>([]);
  const [newImageFiles, setNewImageFiles] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [imageError, setImageError] = useState<string | null>(null);

  useEffect(() => {
    if (!success || !isEditMode) return;

    const timeoutId = window.setTimeout(() => navigate('/products/'), 2000);
    return () => window.clearTimeout(timeoutId);
  }, [success, isEditMode, navigate]);

  // 1️⃣ ЗАГРУЗКА ДАННЫХ ПРИ РЕДАКТИРОВАНИИ
  useEffect(() => {
    if (!isEditMode) return;

    const loadProduct = async () => {
      try {
        setIsLoading(true);
        const data = await getProductForEdit(Number(id));
        
        setFormData({
          name: data.name,
          price: String(data.price),
          oldPrice: data.oldPrice ? String(data.oldPrice) : '',
          description: data.description || '',
          composition: data.composition || '',
          weight: String(data.weight ?? ''),
          stock: String(data.stock ?? ''),
          isNew: data.isNew,
          isHit: data.isHit,
          isActive: data.isActive ?? true,
          categoryId: String(data.categoryId ?? '')
        });

        // Существующие картинки становятся превьюшками
        if (data.imageUrls && data.imageUrls.length > 0) {
          setExistingImageUrls(data.imageUrls);
          setPreviews(data.imageUrls);
        }
      } catch (err) {
        setError('Не удалось загрузить данные товара. Возможно, у вас нет прав на его редактирование.');
      } finally {
        setIsLoading(false);
      }
    };

    loadProduct();
  }, [id, isEditMode]);

  // Обработчик текстовых полей
  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const checked = e.target instanceof HTMLInputElement ? e.target.checked : false;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  // Валидация и добавление НОВЫХ изображений
  const handleImageChange = (e: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    setImageError(null);

    if (files.length === 0) return;

    for (const file of files) {
      if (!['image/png', 'image/jpeg'].includes(file.type)) {
        setImageError('Разрешены только PNG или JPEG.');
        return;
      }
      if (file.size > 3 * 1024 * 1024) {
        setImageError(`Файл "${file.name}" слишком большой (макс. 3MB).`);
        return;
      }
    }

    const newPreviews = files.map(file => URL.createObjectURL(file));
    setNewImageFiles(prev => [...prev, ...files]);
    setPreviews(prev => [...prev, ...newPreviews]);
    e.target.value = ''; 
  };

  // Удаление фото (различаем старые URL и новые файлы)
  const removeImage = (index: number) => {
    const isExisting = index < existingImageUrls.length;
    
    if (isExisting) {
      // Удаляем старый URL из списка существующих
      setExistingImageUrls(prev => prev.filter((_, i) => i !== index));
      // Сдвигаем индексы новых файлов относительно превью
      setPreviews(prev => prev.filter((_, i) => i !== index));
    } else {
      // Удаляем новый файл
      const newIndex = index - existingImageUrls.length;
      URL.revokeObjectURL(previews[index]);
      setNewImageFiles(prev => prev.filter((_, i) => i !== newIndex));
      setPreviews(prev => prev.filter((_, i) => i !== index));
    }
  };

  // Очистка ObjectURL при размонтировании
  useEffect(() => {
    return () => {
      previews.forEach(url => {
        if (!url.startsWith('/uploads')) URL.revokeObjectURL(url);
      });
    };
  }, [previews]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    if (!formData.name.trim()) return setError('Введите название товара');
    if (formData.name.length > 32) return setError('Название не должно превышать 32 символа');
    if (!formData.price || Number(formData.price) <= 0) return setError('Цена должна быть больше 0');
    
    // При создании обязательно нужны фото, при редактировании можно оставить как есть
    if (!isEditMode && newImageFiles.length === 0 && existingImageUrls.length === 0) {
      return setError('Загрузите хотя бы одно изображение');
    }

    try {
      setIsSubmitting(true);
      const dataToSend = new FormData();
      
      Object.entries(formData).forEach(([key, value]) => {
        if (value !== '' && value !== null && value !== undefined) {
          dataToSend.append(key, String(value));
        }
      });

      // Отправляем оставшиеся старые URL (бэкенд поймет, что их не трогали)
      existingImageUrls.forEach(url => dataToSend.append('existingImages', url));
      
      // Добавляем только НОВЫЕ файлы
      newImageFiles.forEach(file => dataToSend.append('images', file));

      if (isEditMode) {
        await updateProduct(Number(id), dataToSend);
      } else {
        await createProduct(dataToSend);
      }
      
      setSuccess(true);
      if (!isEditMode) {
        setFormData({
          name: '', price: '', oldPrice: '', description: '', 
          composition: '', weight: '', stock: '', isNew: false, 
          isHit: false, isActive: true, categoryId: ''
        });
        setExistingImageUrls([]);
        setNewImageFiles([]);
        setPreviews([]);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка при сохранении');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <div className="create-page"><div className="form-island">Загрузка данных...</div></div>;

  if (success && !isEditMode) {
    return (
      <div className="create-page">
        <div className="form-island" style={{ textAlign: 'center', padding: '60px' }}>
          <h2 style={{ color: '#059669' }}>Товар успешно создан!</h2>
          <button onClick={() => setSuccess(false)} className="submit-btn" style={{ maxWidth: '200px', marginTop: '20px' }}>
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
        
        {error && <div className="error-text" style={{ marginBottom: '16px', textAlign: 'center' }}>{error}</div>}
        {success && isEditMode && (
          <div style={{ color: '#059669', marginBottom: '16px', textAlign: 'center' }}>
            Изменения сохранены. Перенаправляем...
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-group full-width">
              <label className="label">Название товара</label>
              <input type="text" name="name" value={formData.name} onChange={handleChange} minLength={6} maxLength={32} className="input" />
              <span style={{ fontSize: '0.8rem', color: '#9ca3af', textAlign: 'right' }}>{formData.name.length}/32</span>
            </div>

            <div className="form-group">
              <label className="label">Цена (₽)</label>
              <input type="number" name="price" value={formData.price} onChange={handleChange} min="1" step="1" className="input" />
            </div>

            <div className="form-group">
              <label className="label">Старая цена</label>
              <input type="number" name="oldPrice" value={formData.oldPrice} onChange={handleChange} min="2" step="1" className="input" />
            </div>

            {/* Изображения */}
            <div className="form-group full-width">
              <label className="label">Изображения</label>
              <div className="file-upload-area" onClick={() => document.getElementById('fileInput')?.click()}>
                <input id="fileInput" type="file" accept=".png,.jpg,.jpeg" multiple onChange={handleImageChange} style={{ display: 'none' }} />
                <div>
                  <span style={{ fontSize: '1.5rem' }}>📷</span>
                  <p>Нажмите для выбора файлов</p>
                </div>
              </div>
              {imageError && <div className="error-text">{imageError}</div>}
              
              <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', marginTop: '12px', minHeight: '100px' }}>
                {previews.map((src, idx) => (
                  <div key={idx} style={{ position: 'relative', width: '100px', height: '100px' }}>
                    <img src={resolveImageUrl(src)} alt={`preview-${idx}`} style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: '12px', border: '1px solid #e5e7eb' }} />
                    <button type="button" onClick={() => removeImage(idx)} style={{ position: 'absolute', top: '-6px', right: '-6px', background: '#ef4444', color: 'white', border: 'none', borderRadius: '50%', width: '24px', height: '24px', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold' }}>×</button>
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

            {/* Чекбоксы */}
            <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '12px' }}>
              <input type="checkbox" name="isNew" checked={formData.isNew} onChange={handleChange} id="isNew" style={{ width: '20px', height: '20px' }} />
              <label htmlFor="isNew" className="label" style={{ margin: 0 }}>Новинка</label>
            </div>

            <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '12px' }}>
              <input type="checkbox" name="isHit" checked={formData.isHit} onChange={handleChange} id="isHit" style={{ width: '20px', height: '20px' }} />
              <label htmlFor="isHit" className="label" style={{ margin: 0 }}>Хит продаж</label>
            </div>

            {/* Только для режима редактирования / админов */}
            {isEditMode && (
              <>
                <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '12px' }}>
                  <input type="checkbox" name="isActive" checked={formData.isActive} onChange={handleChange} id="isActive" style={{ width: '20px', height: '20px' }} />
                  <label htmlFor="isActive" className="label" style={{ margin: 0 }}>Активен (виден в каталоге)</label>
                </div>
                
                <div className="form-group">
                  <label className="label">Категория (ID)</label>
                  <input type="number" name="categoryId" value={formData.categoryId} onChange={handleChange} className="input" placeholder="ID категории" />
                </div>
              </>
            )}
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