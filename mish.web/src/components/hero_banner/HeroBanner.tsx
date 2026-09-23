import { useState, useEffect, useCallback} from 'react';
import './HeroBanner.css'

// типы контракт данных
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
    autoPlayDelay?: number; // задержка автопроигрывания в миллисекундах

    }

    // компонент HeroBanner
    export function HeroBanner({ slides, autoPlayDelay = 5000 }: HeroBannerProps) {
        const [currentIndex, setCurrentIndex] = useState(0);
        const [isPaused, setIsPaused] = useState(false);
        const goToNext = useCallback(() => {
            setCurrentIndex((prev) => (prev + 1) % slides.length);
        }, [slides.length]);

        const goToSlide = (index: number) => {
            setCurrentIndex(index);
        };

        useEffect(() => {
            if (isPaused) return;

            const timerId = setInterval(goToNext, autoPlayDelay);
            return () => clearInterval(timerId);
        }, [goToNext, autoPlayDelay, isPaused]);
    return (
        <section className="hero-banner"
            onMouseEnter={() => setIsPaused(true)}
            onMouseLeave={() => setIsPaused(false)}
            aria-label="Главный баннер с акциями"
        >
            <div className="hero-banner__track"
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
                onClick = {() => goToSlide((currentIndex - 1 + slides.length) % slides.length)}
                aria-lavel="пред слайд"
                > ‹
                </button>
                <button 
                    className= "hero-banner__arrow hero-banner__arrow--next"
                    onClick={goToNext}
                    aria-lavel="след слайд"
                    >›</button>

                <div className= "hero-banner__dots">    
                    {slides.map((slide,index) => (
                        <button
                            key={slide.id}
                            className={`hero-banner__dot ${
                                index === currentIndex ? 'hero-banner__dot--active' : ''
                            }`}
                                onClick= {() => goToSlide(index)}
                                aria-label={'перейти к слайду ${index + 1}'}
                                aria-current={index === currentIndex ? 'true' : undefined}
                                />
            
                    ))}
                    </div>  

                    {!isPaused && (
                        <div
                            className="hero-banner__progress"
                        key={currentIndex}
                        style={{
                            animationDuration: `${autoPlayDelay}ms`}}
                             /> 
                        )}

                </section>
            

        )
    }