import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { apiFetch } from "../../utils/api";
import "./CatalogMenu.css";

interface CategoryTreeItem {
    id: number;
    name: string;
    slug: string;
    children: CategoryTreeItem[];
}

const CATEGORY_ORDER = ["clothing", "shoes", "accessories"];

function isCategoryTree(value: unknown): value is CategoryTreeItem[] {
    return Array.isArray(value) && value.every(isCategory);
}

function isCategory(value: unknown): value is CategoryTreeItem {
    if (typeof value !== "object" || value === null || Array.isArray(value)) {
        return false;
    }

    const category = value as Record<string, unknown>;
    return typeof category.id === "number"
        && typeof category.name === "string"
        && typeof category.slug === "string"
        && isCategoryTree(category.children);
}

interface CatalogMenuProps {
    isOpen: boolean;
    onClose: () => void;
}

export function CatalogMenu({ isOpen, onClose }: CatalogMenuProps) {
    const [categories, setCategories] = useState<CategoryTreeItem[]>([]);
    const [activeCategoryId, setActiveCategoryId] = useState<number | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const controller = new AbortController();

        const loadCategories = async () => {
            try {
                setIsLoading(true);
                setError(null);

                const response = await apiFetch("/api/categories/tree", {
                    signal: controller.signal,
                });
                if (!response.ok) {
                    throw new Error(`Не удалось загрузить категории (HTTP ${response.status}).`);
                }

                const data: unknown = await response.json();
                if (!isCategoryTree(data)) {
                    throw new Error("Сервер вернул категории в неизвестном формате.");
                }

                const orderedCategories = [...data].sort((first, second) => {
                    const firstOrder = CATEGORY_ORDER.indexOf(first.slug);
                    const secondOrder = CATEGORY_ORDER.indexOf(second.slug);
                    return (firstOrder === -1 ? CATEGORY_ORDER.length : firstOrder)
                        - (secondOrder === -1 ? CATEGORY_ORDER.length : secondOrder);
                });

                setCategories(orderedCategories);
                setActiveCategoryId(currentId =>
                    orderedCategories.some(category => category.id === currentId)
                        ? currentId
                        : orderedCategories[0]?.id ?? null,
                );
            } catch (loadError) {
                if (controller.signal.aborted) {
                    return;
                }

                setError(loadError instanceof Error
                    ? loadError.message
                    : "Не удалось загрузить категории из базы данных.");
            } finally {
                if (!controller.signal.aborted) {
                    setIsLoading(false);
                }
            }
        };

        void loadCategories();
        return () => controller.abort();
    }, []);

    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                onClose();
            }
        };

        if (isOpen) {
            document.addEventListener("keydown", handleEscape);
            document.body.style.overflow = "hidden";
        }

        return () => {
            document.removeEventListener("keydown", handleEscape);
            document.body.style.overflow = "";
        };
    }, [isOpen, onClose]);

    if (!isOpen) {
        return null;
    }

    const activeCategory = categories.find(category => category.id === activeCategoryId);

    return (
        <div className="catalog-menu-overlay" onClick={onClose}>
            <nav
                className="catalog-menu"
                onClick={event => event.stopPropagation()}
                aria-label="Каталог товаров"
            >
                <div className="catalog-menu__header">
                    <h1>Каталог</h1>
                    <button
                        className="catalog-menu__close"
                        onClick={onClose}
                        aria-label="Закрыть каталог"
                    >
                        ✕
                    </button>
                </div>

                <div className="catalog-menu__content">
                    <ul className="catalog-menu__catagories">
                        {isLoading && <li>Загрузка категорий...</li>}
                        {!isLoading && !error && categories.map(category => (
                            <li key={category.id}>
                                <button
                                    className={`catalog-menu__category-btn ${
                                        activeCategoryId === category.id
                                            ? "catalog-menu__category-btn--active"
                                            : ""
                                    }`}
                                    onClick={() => setActiveCategoryId(category.id)}
                                    aria-pressed={activeCategoryId === category.id}
                                >
                                    {category.name}
                                </button>
                            </li>
                        ))}
                    </ul>

                    <div className="catalog-menu__subcategories">
                        {error && <p role="alert">{error}</p>}
                        {!isLoading && !error && categories.length === 0 && (
                            <p>В базе пока нет категорий.</p>
                        )}
                        {activeCategory && (
                            <>
                                <h3 className="catalog-menu__subcategory-title">
                                    {activeCategory.name}
                                </h3>
                                <ul className="catalog-menu__subcategory-list">
                                    <li>
                                        <Link
                                            to={`/catalog/${encodeURIComponent(activeCategory.slug)}`}
                                            className="catalog-menu__subcategory-link"
                                            onClick={onClose}
                                        >
                                            Все товары раздела
                                        </Link>
                                    </li>
                                    {activeCategory.children.map(category => (
                                        <li key={category.id}>
                                            <Link
                                                to={`/catalog/${encodeURIComponent(activeCategory.slug)}/${encodeURIComponent(category.slug)}`}
                                                className="catalog-menu__subcategory-link"
                                                onClick={onClose}
                                            >
                                                {category.name}
                                            </Link>
                                        </li>
                                    ))}
                                </ul>
                            </>
                        )}
                    </div>
                </div>
            </nav>
        </div>
    );
}
