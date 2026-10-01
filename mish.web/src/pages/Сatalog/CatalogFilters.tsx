interface Props {
  search: string;
  setSearch: (val: string) => void;
  sort: string;
  setSort: (val: string) => void;
}

export function CatalogFilters({ search, setSearch, sort, setSort }: Props) {
  return (
    <div className="catalog-filters">
      <input
        type="text"
        placeholder="Поиск от 3-х букв..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="search-input"
      />
      
      <select 
        value={sort} 
        onChange={(e) => setSort(e.target.value)}
        className="sort-select"
      >
        <option value="">По умолчанию (Новинки)</option>
        <option value="price_asc">Сначала дешевые</option>
        <option value="price_desc">Сначала дорогие</option>
        <option value="name">По названию (А-Я)</option>
        <option value="rating">По рейтингу</option>
        <option value="hit">Хиты продаж</option>
      </select>
    </div>
  );
}