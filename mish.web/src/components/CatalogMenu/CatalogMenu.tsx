import { useState, useEffect } from "react";
import './CatalogMenu.css'
//import { useInfiniteScroll } from "../../hooks/useInfiniteScroll";

const CATEGORIES = [
    {
        id: 'clothing',
        title: 'Одежда',
        subcategories: [
            {id:'tshirts', title: 'Футболка', link: '/catalog/clothing/tshirts'},
            {id: 'hoodies', title: 'Худи', link: '/catalog/clothing/hoodies'},
            {id: 'pants', title: 'Брюки', link: '/catalog/clothing/pants'},
            { id: 'jackets', title: 'Куртки', link: '/catalog/clothing/jackets' },
        ]
    },
    {
        id: 'shoes',
        title: 'Обувь',
        subcategories: [
            { id: 'sneakers', title: 'Кроссовки', link: '/catalog/shoes/sneakers' },
            { id: 'boots', title: 'Ботинки', link: '/catalog/shoes/boots' },
            { id: 'sandals', title: 'Сандалии', link: '/catalog/shoes/sandals' },
        ]
    },
    {
        id: 'accessories',
        title: 'Аксессуары',
        subcategories: [
            { id: 'bags', title: 'Сумки', link: '/catalog/accessories/bags' },
            { id: 'hats', title: 'Головные уборы', link: '/catalog/accessories/hats' },
        ]
    }
]

interface CatalogMenuProps {
    isOpen: boolean
    onClose: () => void
}

export function CatalogMenu({ isOpen, onClose }: CatalogMenuProps) {
    const [activeCategory, setActiveCategory] = useState<string>(CATEGORIES[0]?.id || '')

    useEffect(() =>{
        const handleEscape = (e: KeyboardEvent) => {
        if (e.key === 'Escape') onClose()
    }
    if (isOpen) {
        document.addEventListener('keydown', handleEscape)
        document.body.style.overflow = 'hidden' 
    }
    return () => {
        document.removeEventListener('keydown', handleEscape)
        document.body.style.overflow = ''
    }
},[isOpen,onClose])
    if (!isOpen) return null
    const currentCategory = CATEGORIES.find(cat => cat.id === activeCategory)

    return (
        <div className="catalog-menu-overlay" onClick={onClose}>
            <nav 
                className="catalog-menu" 
                onClick={(e) => e.stopPropagation()} // Клик внутри меню не закрывает его
                aria-label="Каталог товаров"
            >
                {/* Шапка меню */}
                <div className="catalog-menu__header"> <h1>Каталог</h1> 
                <h2 className="catalog-menu__title">    </h2>
                <button
                    className="catalog-menu__close"
                    onClick={onClose}
                    aria-label="закрыть каталог"
                    > ✕</button>
                    </div>

                <div className="catalog-menu__content">
                    <ul className="catalog-menu__catagories">   
                        {CATEGORIES.map((category) => (
                            <li key={category.id}>
                                <button
                                    className={`catalog-menu__category-btn ${
                                        activeCategory === category.id ? 'catalog-menu__category-btn--active' : ""
                                    }`}
                                    onClick={() => setActiveCategory(category.id)}
                                    >{category.title}</button>
                            </li>
                        ))}
                    </ul>
        <div className="catalog-menu__subcategories">{currentCategory && (
            <>
                <h3 className="catalog-menu__subcategory-title">{currentCategory.title}</h3>
                <ul className="catalog-menu__subcategory-list"> {currentCategory.subcategories.map((sub)=> (
                    <li key={sub.id}>
                        <a
                            href={sub.link}
                            className="catalog-menu__subcategory-link"
                            onClick={onClose}
                        >{sub.title}</a>
                    </li>
                ))}
                </ul>
            </>
        )}
                    </div>
                </div>
            </nav>
        </div>
    )
}