// AdminTool/CreateProduct/useCreateProduct.ts
import { useState, useEffect, type ChangeEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { createProduct, updateProduct, getProductForEdit } from '../../../services/productService';
//import { ENDPOINTS } from '../../../config';

interface CategoryTree {
  id: number;
  name: string;
  children: CategoryTree[];
}

export function useCreateProduct() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(isEditMode);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  
  // Категории
  const [categories, setCategories] = useState<CategoryTree[]>([]);
  const [parentCatId, setParentCatId] = useState('');

  // Форма
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
    isActive: true,
    categoryId: ''
  });

  // Изображения
  const [existingImageUrls, setExistingImageUrls] = useState<string[]>([]);
  const [newImageFiles, setNewImageFiles] = useState<File[]>([]);
  const [previews, setPreviews] = useState<string[]>([]);
  const [imageError, setImageError] = useState<string | null>(null);

  // Загрузка категорий
  useEffect(() => {
    fetch('/api/categories/tree')
      .then(res => res.json())
      .then(data => setCategories(data))
      .catch(err => console.error('Ошибка загрузки категорий:', err));
  }, []);

  // Загрузка данных товара при редактировании
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

        if (data.imageUrls && data.imageUrls.length > 0) {
          setExistingImageUrls(data.imageUrls);
          setPreviews(data.imageUrls);
        }
      } catch (err) {
        setError('Не удалось загрузить данные товара.');
      } finally {
        setIsLoading(false);
      }
    };

    loadProduct();
  }, [id, isEditMode]);

  // Синхронизация родительской категории при редактировании
  useEffect(() => {
    if (isEditMode && formData.categoryId && categories.length > 0) {
      const targetId = Number(formData.categoryId);
      let foundParentId = '';
      
      for (const cat of categories) {
        if (cat.id === targetId) {
          foundParentId = String(cat.id);
          break;
        }
        const isChild = cat.children.some((child: CategoryTree) => child.id === targetId);
        if (isChild) {
          foundParentId = String(cat.id);
          break;
        }
      }
      setParentCatId(foundParentId);
    }
  }, [isEditMode, formData.categoryId, categories]);

  // Редирект после успешного редактирования
  useEffect(() => {
    if (!success || !isEditMode) return;
    const timeoutId = window.setTimeout(() => navigate(`/product/${id}`), 2000);
    return () => window.clearTimeout(timeoutId);
  }, [success, isEditMode, navigate, id]);

  // Очистка ObjectURL
  useEffect(() => {
    return () => {
      previews.forEach(url => {
        if (!url.startsWith('/uploads')) URL.revokeObjectURL(url);
      });
    };
  }, [previews]);

  const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const checked = e.target instanceof HTMLInputElement ? e.target.checked : false;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleParentCategoryChange = (e: ChangeEvent<HTMLSelectElement>) => {
    const newParentId = e.target.value;
    setParentCatId(newParentId);
    setFormData(prev => ({ ...prev, categoryId: '' }));
  };

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

  const removeImage = (index: number) => {
    const isExisting = index < existingImageUrls.length;
    
    if (isExisting) {
      setExistingImageUrls(prev => prev.filter((_, i) => i !== index));
      setPreviews(prev => prev.filter((_, i) => i !== index));
    } else {
      const newIndex = index - existingImageUrls.length;
      URL.revokeObjectURL(previews[index]);
      setNewImageFiles(prev => prev.filter((_, i) => i !== newIndex));
      setPreviews(prev => prev.filter((_, i) => i !== index));
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    if (!formData.name.trim()) return setError('Введите название товара');
    if (formData.name.length > 32) return setError('Название не должно превышать 32 символа');
    if (!formData.price || Number(formData.price) <= 0) return setError('Цена должна быть больше 0');
    if (!isEditMode && !formData.categoryId) return setError('Выберите категорию товара');
    
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

      existingImageUrls.forEach(url => dataToSend.append('existingImages', url));
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

  return {
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
  };
}