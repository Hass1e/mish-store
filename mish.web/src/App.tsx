import { lazy, Suspense, type ComponentType } from 'react'
import { createBrowserRouter, RouterProvider, Outlet, useParams, useNavigate, ScrollRestoration } from 'react-router-dom'

import './App.css'
import { Header } from './header.tsx'
import { Footer } from './components/footer/Footer.tsx'
import ScrollToTop from './components/ScrollToTop/ScrollToTop.tsx'
import { RequirePermission } from './components/Permission/RequirePermission.tsx'
import { ChangeRoles } from './pages/AdminTool/ChangeRoles/ChangeRoles.tsx'
import { CartProvider } from './pages/Cart/components/CartContext.tsx'
// 1. Динамические импорты (загрузятся только при переходе на страницу)
const Home = lazy(() => import('./components/hero_banner/HeroBanner__mass.tsx').then(m => ({ default: m.Home })))
const CatalogHeader = lazy(() => import('./components/ProductCard/ProductCard_mass.tsx').then(m => ({ default: m.Catalog })))
//const CreateProduct = lazy(() => import('./pages/home/CreateProduct/CreateProduct.tsx').then(m => ({ default: m.CreateProduct })))
const AdminTool = lazy(() => import('./pages/AdminTool/AdminTool.tsx').then(m => ({ default: m.AdminTool })))
const Profile = lazy(() => import('./pages/home/Profile/Profile.tsx').then(m => ({ default: m.Profile })))
const CartPage = lazy(() => import('./pages/Cart/CartPage.tsx').then(m => ({ default: m.CartPage })))
const AuthModal = lazy(() => import('./components/AuthModal/AuthModal.tsx')) 
const ProductDetail = lazy(() => import('./components/ProductDetail/ProductDetail.tsx').then(m => ({ default: m.ProductDetail })))
const ProductManager = lazy(() => import('./pages/AdminTool/CreateProduct/ProductManager.tsx').then(m => ({ default: m.ProductManager })))
const Products = lazy(() => import('./pages/Products.tsx').then(m => ({ default: m.Products as ComponentType })))
const AdminOrders = lazy(() => import('./pages/AdminTool/DeliveryStatus/AdminOrders.tsx').then(m => ({ default: m.AdminOrders as ComponentType })))
const Catalog = lazy(() => import('../src/pages/Сatalog/Catalog.tsx').then(m => ({ default: m.Catalog as ComponentType })))


// Компонент-заглушка, пока страница скачивается
const PageLoader = () => <div style={{ minHeight: '50vh' }}>Загрузка...</div>

function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()
  return <ProductDetail productId={Number(id ?? 0)} />
}

function LoginPage() {
  const navigate = useNavigate()
  return <AuthModal isOpen={true} onClose={() => navigate('/')} />
}

const router = createBrowserRouter([
    {
        path: "/",
        element: <Layout />,
        children: [
            {
                index: true,
                // Оборачиваем ленивые компоненты в Suspense
                element: <Suspense fallback={<PageLoader />}><Home /><CatalogHeader /></Suspense>,
            },
            {
                path: "product/:id",
                element: <Suspense fallback={<PageLoader />}><ProductDetailPage /></Suspense>,
            },
            
            {
                path: "catalog",
                element: (
                <Suspense fallback={<PageLoader />}>
                    <Catalog />
                </Suspense>
                ),
            },
            {
                path: "catalog/:parentSlug",
                element: (
                <Suspense fallback={<PageLoader />}>
                    <Catalog />
                </Suspense>
                ),
            },
            {
                path: "catalog/:parentSlug/:subSlug",
                element: (
                <Suspense fallback={<PageLoader />}>
                    <Catalog />
                </Suspense>
                ),
            },


            {
                path: "products/create",
                element: (
                    <RequirePermission permission="products.create">
                        <Suspense fallback={<PageLoader />}>
                            <ProductManager  />
                        </Suspense>
                    </RequirePermission>
                ),
            },
            {
                path: "products/:id/edit",
                element: (
                    <RequirePermission permission="products.edit_any">
                        <Suspense fallback={<PageLoader />}>
                            <ProductManager  />
                        </Suspense>
                    </RequirePermission>
                ),
            },
            {
                path: "atool",
                element: (
                    <RequirePermission permission="users.manage_roles"> 
                        
                        <Suspense fallback={<PageLoader />}>
                            <AdminTool />
                        </Suspense>
                    </RequirePermission>
                ),
            },
               {
                path: "Change-order",
                element: (
                    <RequirePermission permission="orders.update_status"> 
                        
                        <Suspense fallback={<PageLoader />}>
                            <AdminOrders />
                        </Suspense>
                    </RequirePermission>
                ),
            },
            {
                path: "/change-roles",
                element: <Suspense fallback={<PageLoader />}><ChangeRoles /></Suspense>,
            },            {
                path: "/profile/*",
                element: <Suspense fallback={<PageLoader />}><Profile /></Suspense>,
            },
            {
               path: "/cart",
               element: <Suspense fallback={<PageLoader />}><CartPage /></Suspense>,
            },
            {
               path: "/products",
               element: <Suspense fallback={<PageLoader />}><Products /></Suspense>,
            },
            {
              path: "/login",
              element: <Suspense fallback={<PageLoader />}><LoginPage /></Suspense>
            },
            {
                path: "*",
                element: <div style={{ minHeight: '50vh' }}>Страница не найдена</div>,
            },
        ],
    },
]);




function Layout() {
  return (
    <CartProvider>
      <Header />
        <title>ой крутой</title>
      <Outlet />
      <ScrollRestoration getKey={(location) => location.pathname} />
      <ScrollToTop />
      <Footer />
    </CartProvider>
  )
}

function App() {
  return (
    <RouterProvider router={router} />
  )
}

export default App
