import { lazy, Suspense } from 'react'
import { createBrowserRouter, RouterProvider, Outlet, useParams, useNavigate, ScrollRestoration } from 'react-router-dom'

import './App.css'
import { Header } from './header.tsx'
import { Footer } from './components/footer/Footer.tsx'
import ScrollToTop from './components/ScrollToTop/ScrollToTop.tsx'
import { RequirePermission } from './components/Permission/RequirePermission.tsx'
import { ChangeRoles } from './pages/home/AdminTool/ChangeRoles/ChangeRoles.tsx'
// 1. Динамические импорты (загрузятся только при переходе на страницу)
const Home = lazy(() => import('./components/hero_banner/HeroBanner__mass.tsx').then(m => ({ default: m.Home })))
const Catalog = lazy(() => import('./components/ProductCard/ProductCard_mass.tsx').then(m => ({ default: m.Catalog })))
const CreateProduct = lazy(() => import('./pages/home/CreateProduct/CreateProduct.tsx').then(m => ({ default: m.CreateProduct })))
const AdminTool = lazy(() => import('./pages/home/AdminTool/AdminTool.tsx').then(m => ({ default: m.AdminTool })))
const Profile = lazy(() => import('./pages/home/Profile/Profile.tsx').then(m => ({ default: m.Profile })))
const CartPage = lazy(() => import('./pages/Cart/CartPage.tsx').then(m => ({ default: m.CartPage })))
const AuthModal = lazy(() => import('./components/AuthModal/AuthModal.tsx')) 
const ProductDetail = lazy(() => import('./components/ProductDetail/ProductDetail.tsx').then(m => ({ default: m.ProductDetail })))

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
                element: <Suspense fallback={<PageLoader />}><Home /><Catalog /></Suspense>,
            },
            {
                path: "product/:id",
                element: <Suspense fallback={<PageLoader />}><ProductDetailPage /></Suspense>,
            },
            {
                path: "products/create",
                element: (
                    <RequirePermission permission="products.create">
                        <Suspense fallback={<PageLoader />}>
                            <CreateProduct />
                        </Suspense>
                    </RequirePermission>
                ),
            },
            {
                path: "atool",
                element: (
                    <RequirePermission permission="users.manage_roles"> 
                        {/* Или другое право, которое есть только у админов */}
                        <Suspense fallback={<PageLoader />}>
                            <AdminTool />
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
    <>
      <Header />
        <title>ой крутой</title>
      <Outlet />
      <ScrollRestoration getKey={(location) => location.pathname} />
      <ScrollToTop />
      <Footer />
    </>
  )
}

function App() {
  return (
    <RouterProvider router={router} />
  )
}

export default App

