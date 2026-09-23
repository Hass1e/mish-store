import { useState, useEffect } from 'react';
import './ScrollToTop.css';

function ScrollToTop() {
  const [isVisible, setIsVisible] = useState(false);

  // Следим за прокруткой страницы
  useEffect(() => {
    const toggleVisibility = () => {
      // Если прокрутили больше чем на 300px, показываем кнопку
      if (window.scrollY > 300) {
        setIsVisible(true);
      } else {
        setIsVisible(false);
      }
    };

    window.addEventListener('scroll', toggleVisibility);

    // Удаляем обработчик при размонтировании компонента
    return () => window.removeEventListener('scroll', toggleVisibility);
  }, []);

  // Функция плавного скролла наверх
  const scrollToTop = () => {
    window.scrollTo({
      top: 0,
      behavior: 'smooth', // Плавный скролл
    });
  };

  return (
    <>
      {isVisible && (
        <div className="scroll-to-top-band" onClick={scrollToTop} title="Наверх">
          <div className="scroll-arrow">▲</div>
        </div>
      )}
    </>
  );
}

export default ScrollToTop;
