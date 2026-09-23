
import './App.css'
import { Header } from './header.tsx'
import { Home } from './components/hero_banner/HeroBanner__mass.tsx'
import { Catalog } from './components/ProductCard/ProductCard_mass.tsx'
import { Footer } from './components/footer/Footer.tsx'
import { createBrowserRouter, RouterProvider, Outlet, useParams, useNavigate, ScrollRestoration } from 'react-router-dom'
import { ProductDetail } from './components/ProductDetail/ProductDetail.tsx'
import ScrollToTop from './components/ScrollToTop/ScrollToTop.tsx'
import { CreateProduct } from './pages/home/CreateProduct/CreateProduct.tsx'
import { AdminTool } from './pages/home/AdminTool/AdminTool.tsx'
import { Profile } from './pages/home/Profile/Profile.tsx'
import { CartPage } from './pages/Cart/CartPage.tsx'
import AuthModal from './components/AuthModal/AuthModal.tsx'



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
                element: <><Home /><Catalog /></>,
            },
            {
                path: "product/:id",
                element: <ProductDetailPage />,
            },
            {
                path: "products/create",
                element: <CreateProduct />,
            },
            {
                path: "atool",
                element: <AdminTool />,
            },
            {
                path: "/profile/*",
                element: <Profile />,
            },
            {
               path: "/cart",
               element: <CartPage />,
            },
            {
              path: "/login",
              element: <LoginPage />

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

