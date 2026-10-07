import { useState, useEffect, useCallback, useRef } from 'react';
import './HeroBanner.css'

interface Slide {
    id: number;
    imageUrl: string;
    title: string;
    subtitle: string;
    ctaText: string;
    ctaLink: string;
}

interface HeroBannerProps {
    slides: Slide[];
    autoPlayDelay?: number;
}

export function HeroBanner({ slides, autoPlayDelay = 5000 }: HeroBannerProps) {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isPaused, setIsPaused] = useState(false);
    
    // Используем ref для хранения таймера, чтобы избежать проблем с замыканиями
    const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
    
    // Очищаем таймер при размонтировании компонента
    useEffect(() => {
        return () => {
            if (timerRef.current) {
                clearInterval(timerRef.current);
            }
        };
    }, []);

    // Мемоизируем функцию перехода к следующему слайду
    const goToNext = useCallback(() => {
        setCurrentIndex((prev) => (prev + 1) % slides.length);
    }, [slides.length]);

    // Функция для перехода к конкретному слайду
    const goToSlide = useCallback((index: number) => {
        setCurrentIndex(index);
    }, []);

    // Эффект для автопроигрывания
    useEffect(() => {
        // Очищаем предыдущий таймер
        if (timerRef.current) {
            clearInterval(timerRef.current);
        }

        // Если компонент на паузе или нет слайдов, не запускаем таймер
        if (isPaused || slides.length === 0) {
            return;
        }

        // Запускаем новый таймер
        timerRef.current = setInterval(goToNext, autoPlayDelay);

        // Очищаем таймер при размонтировании или изменении зависимостей
        return () => {
            if (timerRef.current) {
                clearInterval(timerRef.current);
                timerRef.current = null;
            }
        };
    }, [goToNext, autoPlayDelay, isPaused, slides.length]);

    // Предотвращаем рендеринг, если нет слайдов
    if (!slides || slides.length === 0) {
        return null;
    }

    return (
        <section 
            className="hero-banner"
            onMouseEnter={() => setIsPaused(true)}
            onMouseLeave={() => setIsPaused(false)}
            aria-label="Главный баннер с акциями"
        >
            <div 
                className="hero-banner__track"
                style={{
                    transform: `translateX(-${currentIndex * 100}%)`,
                }}
            >   
                {slides.map((slide) => (
                    <div key={slide.id} className="hero-banner__slide">
                        <img
                            src={slide.imageUrl}
                            alt={slide.title}
                            className="hero-banner__img"
                            draggable={false}
                            loading="lazy" // Оптимизация загрузки изображений
                        />
                        <div className="hero-banner__content">
                            <h2 className="hero-banner__title">{slide.title}</h2>
                            <p className="hero-banner__subtitle">{slide.subtitle}</p>
                            <a href={slide.ctaLink} className="hero-banner__cta">{slide.ctaText}</a>
                        </div>
                    </div>
                ))}
            </div>

            <button
                className="hero-banner__arrow hero-banner__arrow--prev"
                onClick={() => goToSlide((currentIndex - 1 + slides.length) % slides.length)}
                aria-label="Предыдущий слайд" // Исправлена опечатка в aria-label
            >
                ‹
            </button>
            
            <button 
                className="hero-banner__arrow hero-banner__arrow--next"
                onClick={goToNext}
                aria-label="Следующий слайд" // Исправлена опечатка в aria-label
            >
                ›
            </button>

            <div className="hero-banner__dots">    
                {slides.map((slide, index) => (
                    <button
                        key={slide.id}
                        className={`hero-banner__dot ${
                            index === currentIndex ? 'hero-banner__dot--active' : ''
                        }`}
                        onClick={() => goToSlide(index)}
                        aria-label={`Перейти к слайду ${index + 1}`} // Исправлен шаблонный литерал
                        aria-current={index === currentIndex ? 'true' : undefined}
                    />
                ))}
            </div>  

            {!isPaused && (
                <div
                    className="hero-banner__progress"
                    key={currentIndex}
                    style={{
                        animationDuration: `${autoPlayDelay}ms`
                    }}
                /> 
            )}
        </section>
    );
}