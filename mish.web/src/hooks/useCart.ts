import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../context/AuthContext';
import { cartApi } from '../utils/cartApi';

interface CartItemData {
  productId: number;
  quantity: number;
  productName?: string;
  price?: number;
  imageUrl?: string;
}

interface CartState {
  items: CartItemData[];
  totalItems: number;
  totalPrice: number;
}

const GUEST_CART_KEY = 'mish_guest_cart';

export function useCart() {
  const { isAuthenticated } = useAuth();
  const [cart, setCart] = useState<CartState>({ items: [], totalItems: 0, totalPrice: 0 });
  const [isLoading, setIsLoading] = useState(true);

  // Вспомогательная функция для безопасного обновления гостевой корзины
  const updateGuestCart = useCallback((newItems: CartItemData[]) => {
    localStorage.setItem(GUEST_CART_KEY, JSON.stringify(newItems));
    
    const totalPrice = newItems.reduce((sum, item) => sum + (item.price || 0) * item.quantity, 0);
    const totalItems = newItems.reduce((sum, item) => sum + item.quantity, 0);

    setCart({
      items: newItems,
      totalItems,
      totalPrice
    });
  }, []);

  useEffect(() => {
    const loadCart = async () => {
      setIsLoading(true);
      try {
        if (isAuthenticated) {
          const guestCartRaw = localStorage.getItem(GUEST_CART_KEY);
          if (guestCartRaw) {
            const guestItems: CartItemData[] = JSON.parse(guestCartRaw);
            for (const item of guestItems) {
              await cartApi.addToCart(item.productId, item.quantity);
            }
            localStorage.removeItem(GUEST_CART_KEY);
          }

          const data = await cartApi.getCart();
          setCart({
            items: data.items.map((item: any) => ({
              productId: item.productId,
              quantity: item.quantity,
              productName: item.productName,
              price: item.price,
              imageUrl: item.imageUrl
            })),
            totalItems: data.totalItemsCount,
            totalPrice: data.totalPrice
          });
        } else {
          const guestCartRaw = localStorage.getItem(GUEST_CART_KEY);
          if (guestCartRaw) {
            updateGuestCart(JSON.parse(guestCartRaw));
          } else {
            setCart({ items: [], totalItems: 0, totalPrice: 0 });
          }
        }
      } catch (error) {
        console.error('[useCart] Ошибка загрузки корзины:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadCart();
  }, [isAuthenticated, updateGuestCart]);

  const addToCart = useCallback(async (productId: number, quantity: number = 1, productData?: Partial<CartItemData>) => {
    if (isAuthenticated) {
      await cartApi.addToCart(productId, quantity);
      const data = await cartApi.getCart();
      setCart({
        items: data.items.map((item: any) => ({
          productId: item.productId,
          quantity: item.quantity,
          productName: item.productName,
          price: item.price,
          imageUrl: item.imageUrl
        })),
        totalItems: data.totalItemsCount,
        totalPrice: data.totalPrice
      });
    } else {
      const guestCartRaw = localStorage.getItem(GUEST_CART_KEY);
      const items: CartItemData[] = guestCartRaw ? JSON.parse(guestCartRaw) : [];

      const existingIndex = items.findIndex(item => item.productId === productId);
      let newItems: CartItemData[];

      if (existingIndex !== -1) {
        // ✅ СОЗДАЕМ КОПИЮ МАССИВА И ОБЪЕКТА, чтобы React увидел изменения
        newItems = [...items];
        newItems[existingIndex] = {
          ...newItems[existingIndex],
          quantity: newItems[existingIndex].quantity + quantity,
          // Обновляем данные, если они переданы, иначе оставляем старые
          ...(productData && {
            productName: productData.productName ?? newItems[existingIndex].productName,
            price: productData.price ?? newItems[existingIndex].price,
            imageUrl: productData.imageUrl ?? newItems[existingIndex].imageUrl
          })
        };

        // Если количество стало 0 или меньше, удаляем товар из массива
        if (newItems[existingIndex].quantity <= 0) {
          newItems.splice(existingIndex, 1);
        }
      } else {
        newItems = [...items, { productId, quantity, ...productData }];
      }

      updateGuestCart(newItems);
    }
  }, [isAuthenticated, updateGuestCart]);

  const removeFromCart = useCallback(async (productId: number) => {
    if (isAuthenticated) {
      await cartApi.removeFromCart(productId);
      const data = await cartApi.getCart();
      setCart({
        items: data.items.map((item: any) => ({
          productId: item.productId,
          quantity: item.quantity,
          productName: item.productName,
          price: item.price,
          imageUrl: item.imageUrl
        })),
        totalItems: data.totalItemsCount,
        totalPrice: data.totalPrice
      });
    } else {
      const guestCartRaw = localStorage.getItem(GUEST_CART_KEY);
      if (guestCartRaw) {
        const items: CartItemData[] = JSON.parse(guestCartRaw);
        // ✅ filter создает новый массив
        const newItems = items.filter(item => item.productId !== productId);
        updateGuestCart(newItems);
      }
    }
  }, [isAuthenticated, updateGuestCart]);

  // ✅ Упрощенная логика: просто добавляем -1. Если станет <= 0, addToCart сам удалит товар.
  const decreaseQuantity = useCallback(async (productId: number) => {
    await addToCart(productId, -1);
  }, [addToCart]);

  return {
    cart,
    isLoading,
    addToCart,
    removeFromCart,
    decreaseQuantity,
    isInCart: useCallback((productId: number) => {
      return cart.items.some(item => item.productId === productId);
    }, [cart.items])
  };
}