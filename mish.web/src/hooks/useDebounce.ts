import { useState, useEffect } from 'react';

/**
 * Хук для задержки обновления значения.
 * Идеально подходит для полей поиска, чтобы не спамить API запросами при каждом нажатии клавиши.
 * 
 * @param value - Значение, которое нужно задержать (например, строка из input)
 * @param delay - Задержка в миллисекундах (например, 500)
 * @returns Задержанное значение
 */
export function useDebounce<T>(value: T, delay: number): T {
  // Состояние для хранения задержанного значения
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    // Устанавливаем таймер, который обновит состояние через `delay` мс
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    // Функция очистки: если `value` изменился до истечения таймера, 
    // мы отменяем предыдущий таймер и запускаем новый. 
    // Это и есть магия debounce!
    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]); // Перезапускаем эффект только при изменении value или delay

  return debouncedValue;
}