import { NavLink } from 'react-router-dom';



export interface CategoryTreeDto {
  id: number;
  name: string;
  slug: string;
  children: CategoryTreeDto[];
}

interface Props {
  categories: CategoryTreeDto[];
}

type Subcategory = CategoryTreeDto['children'][number];

export function CatalogSidebar({ categories }: Props) {
  return (
    <aside className="catalog-sidebar">
      <h3 className="sidebar-title">Каталог</h3>
      <nav className="sidebar-nav">
        <NavLink to="/catalog" end className="sidebar-link">
          Все товары
        </NavLink>
        
        {categories.map((category) => (
          <div key={category.id} className="category-group">
            <NavLink 
              to={`/catalog/${category.slug}`} 
              className={({ isActive }) => `sidebar-link parent-link ${isActive ? 'active' : ''}`}
            >
              {category.name}
            </NavLink>
            
            {category.children.length > 0 && (
              <div className="subcategories">
                {category.children.map((sub: Subcategory) => (
                  <NavLink
                    key={sub.id}
                    to={`/catalog/${category.slug}/${sub.slug}`}
                    className={({ isActive }) => `sidebar-link sub-link ${isActive ? 'active' : ''}`}
                  >
                    {sub.name}
                  </NavLink>
                ))}
              </div>
            )}
          </div>
        ))}
      </nav>
    </aside>
  );
}