import { useState, type ChangeEvent, type FormEvent } from 'react';
import { createProduct } from '../../../services/productService';
import './CreateProduct.css';

export function CreateProduct() {
  const [isSubmitting, setIsSubmitting] = useState(false);
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
  });

  // Массив файлов и их превью
  const [imageFiles, setImageFiles] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [imageError, setImageError] = useState<string | null>(null);

  // Обработчик текстовых полей
  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    const checked = e.target instanceof HTMLInputElement ? e.target.checked : false;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  // Валидация и добавление изображений
  const handleImageChange = (e: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    setImageError(null);

    if (files.length === 0) return;

    // Проверка каждого файла перед добавлением
    for (const file of files) {
      if (!['image/png', 'image/jpeg'].includes(file.type)) {
        setImageError('Разрешены только файлы формата PNG или JPEG.');
        return;
      }
      if (file.size > 3 * 1024 * 1024) {
        setImageError(`Файл "${file.name}" слишком большой. Максимум 3MB.`);
        return;
      }
    }

    // Создаем превьюшки для новых файлов
    const newPreviews = files.map(file => URL.createObjectURL(file));
    
    setImageFiles(prev => [...prev, ...files]);
    setPreviews(prev => [...prev, ...newPreviews]);
    
    // Очищаем input, чтобы можно было выбрать тот же файл повторно при необходимости
    e.target.value = ''; 
  };

  // Удаление фото из списка
  const removeImage = (index: number) => {
    URL.revokeObjectURL(previews[index]); // Освобождаем память браузера
    setImageFiles(prev => prev.filter((_, i) => i !== index));
    setPreviews(prev => prev.filter((_, i) => i !== index));
  };
const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
  e.preventDefault();
  setError(null);

  console.log(' Отладка отправки:');
  console.log('   imageFiles.length:', imageFiles.length);
  console.log('   formData.name:', formData.name);
  console.log('   formData.price:', formData.price);

  if (!formData.name.trim()) return setError('Введите название товара');
  if (formData.name.length > 32) return setError('Название не должно превышать 32 символа');
  if (!formData.price || Number(formData.price) <= 0) return setError('Цена должна быть больше 0');
  if (imageFiles.length === 0) return setError('Загрузите хотя бы одно изображение');

  try {
    setIsSubmitting(true);

    const dataToSend = new FormData();
    
    // Добавляем текстовые поля
    Object.entries(formData).forEach(([key, value]) => {
      dataToSend.append(key, String(value));
    });

    // Добавляем файлы ИЗ СТЕЙТА (он актуален, т.к. счетчик работает)
    let validFilesCount = 0;
    imageFiles.forEach((file, index) => {
      // Проверка на валидность File объекта
      if (file && file instanceof File && file.size > 0) {
        dataToSend.append('images', file);
        validFilesCount++;
      } else {
        console.warn(`⚠️ Файл #${index} невалиден:`, file);
      }
    });

    console.log(`✅ В FormData добавлено ${validFilesCount} файлов`);
    console.log('📤 Отправка запроса...');

    const response = await createProduct(dataToSend);
    console.log('📥 Ответ от сервера:', response);
    
    setSuccess(true);
    previews.forEach(url => URL.revokeObjectURL(url));
    setFormData({
      name: '', price: '', oldPrice: '', description: '', 
      composition: '', weight: '', stock: '', isNew: false, isHit: false
    });
    setImageFiles([]);
    setPreviews([]);
    
  } catch (err) {
    console.error('❌ Ошибка при создании:', err);
    setError(err instanceof Error ? err.message : 'Не удалось создать товар');
  } finally {
    setIsSubmitting(false);
  }
};

  if (success) {
    return (
      <div className="create-page">
        <div className="form-island" style={{ textAlign: 'center', padding: '60px' }}>
          <h2 style={{ color: '#059669' }}>Товар успешно создан!</h2>
          <p>SKU был сгенерирован автоматически.</p>
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
        <h1 className="form-title">Новый товар</h1>
        
        {error && <div className="error-text" style={{ marginBottom: '16px', textAlign: 'center', fontSize: '1rem' }}>{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-grid">
            
            {/* Название */}
            <div className="form-group full-width">
              <label className="label">Название товара (макс. 32 символа)</label>
              <input 
                type="text" 
                name="name" 
                value={formData.name} 
                onChange={handleChange} 
                minLength={6}
                maxLength={32}
                className="input" 
                placeholder="Например: Носки 100% Хлопок"
              />
              <span style={{ fontSize: '0.8rem', color: '#9ca3af', textAlign: 'right' }}>
                {formData.name.length}/32
              </span>
            </div>

            {/* Цена и Старая цена */}
            <div className="form-group">
              <label className="label">Цена (₽)</label>
              <input 
                type="number" 
                name="price" 
                value={formData.price} 
                onChange={handleChange} 
                min="1"
                step="1"
                className="input" 
                placeholder="0 000.00"
              />
            </div>

            <div className="form-group">
              <label className="label">Старая цена (опционально)</label>
              <input 
                type="number" 
                name="oldPrice" 
                value={formData.oldPrice} 
                onChange={handleChange} 
                min="2"
                step="1"
                className="input" 
                placeholder="0 000.00"
              />
            </div>

            {/* Загрузка фото */}
            <div className="form-group full-width">
              <label className="label">Главное изображение</label>
              <div className="file-upload-area" onClick={() => document.getElementById('fileInput')?.click()}>
                <input 
                  id="fileInput"
                  type="file" 
                  accept=".png, .jpg, .jpeg" 
                  onChange={handleImageChange} 
                  style={{ display: 'none' }} 
                />
              {imageFiles.length > 0 ? (
                <div style={{ color: '#059669', fontWeight: 'bold' }}>
                    ✅ Выбрано файлов: {imageFiles.length}
                    {/* Если хочешь показать имена всех файлов: */}
                    {/* ({imageFiles.map(f => f.name).join(', ')}) */}
                </div>
                ) : (
                <div>
                    <span style={{ fontSize: '1.5rem' }}>📷</span>
                    <p>Нажмите для выбора файла (PNG, JPG)</p>
                </div>
                )}
              </div>
                 {imageError && <div className="error-text">{imageError}</div>}
              <p className="file-hint">Только PNG, JPG, JPEG, максимум 1000x1000 пикселей и до 3мб.</p>

                   <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', marginBottom: '12px', minHeight: '100px' }}>
                {previews.map((src, idx) => (
                  <div key={idx} style={{ position: 'relative', width: '100px', height: '100px' }}>
                    <img 
                      src={src} 
                      alt={`preview-${idx}`} 
                      style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: '12px', border: '1px solid #e5e7eb' }} 
                    />
                    <button 
                      type="button" 
                      onClick={() => removeImage(idx)}
                      style={{ 
                        position: 'absolute', top: '-6px', right: '-6px', 
                        background: '#585858', color: 'white', border: 'none', 
                        borderRadius: '50%', width: '24px', height: '24px', 
                        cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold'
                      }}
                    >×</button>
                  </div>                ))}
                </div>

             
            </div>

            {/* Описание */}
            <div className="form-group full-width">
              <label className="label">Описание</label>
              <textarea 
                name="description" 
                value={formData.description} 
                onChange={handleChange} 
                className="textarea" 
                placeholder="Подробное описание товара..."
              />
            </div>

            {/* Характеристики */}
            <div className="form-group">
              <label className="label">Вес (граммы)</label>
              <input type="number" name="weight" step="1" value={formData.weight} onChange={handleChange} className="input" />
            </div>

            <div className="form-group">
              <label className="label">Количество на складе</label>
              <input type="number" name="stock" step="1" value={formData.stock} onChange={handleChange} className="input" />
            </div>

            <div className="form-group full-width">
              <label className="label">Состав / Материал</label>
              <input type="text" name="composition" value={formData.composition} onChange={handleChange} className="input" />
            </div>

            {/* Чекбоксы */}
            <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '12px' }}>
              <input type="checkbox" name="isNew" checked={formData.isNew} onChange={handleChange} id="isNew" style={{ width: '20px', height: '20px' }} />
              <label htmlFor="isNew" className="label" style={{ margin: 0 }}>Это новинка</label>
            </div>

            <div className="form-group" style={{ flexDirection: 'row', alignItems: 'center', gap: '12px' }}>
              <input type="checkbox" name="isHit" checked={formData.isHit} onChange={handleChange} id="isHit" style={{ width: '20px', height: '20px' }} />
              <label htmlFor="isHit" className="label" style={{ margin: 0 }}>Хит продаж</label>
            </div>

          </div>

          <button type="submit" disabled={isSubmitting} className="submit-btn">
            {isSubmitting ? 'Создание...' : 'Создать товар'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default CreateProduct;