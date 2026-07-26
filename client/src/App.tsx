import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { RequireRole } from './auth/RequireAuth';
import { CartProvider } from './features/cart/CartContext';
import { Layout } from './components/layout/Layout';
import { ForbiddenPage, NotFoundPage } from './components/layout/ForbiddenPage';
import { RegisterForm } from './features/register/RegisterForm';
import { LoginForm } from './features/login/LoginForm';
import { ProductListPage } from './features/products/ProductListPage';
import { ProductDetailPage } from './features/products/ProductDetailPage';
import { CartPage } from './features/cart/CartPage';
import { CheckoutPage } from './features/checkout/CheckoutPage';
import { OrdersListPage } from './features/orders/OrdersListPage';
import { OrderDetailPage } from './features/orders/OrderDetailPage';
import { ProfilePage } from './features/profile/ProfilePage';
import { AdminProductsPage } from './features/admin/products/AdminProductsPage';
import { AdminInventoryPage } from './features/admin/inventory/AdminInventoryPage';
import { AdminCustomersPage } from './features/admin/customers/AdminCustomersPage';
import { AdminStaffPage } from './features/admin/staff/AdminStaffPage';

const CENTERED = 'flex min-h-[calc(100vh-8rem)] items-center justify-center';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route element={<Layout />}>
              <Route index element={<Navigate to="/products" replace />} />

              <Route
                path="login"
                element={
                  <div className={CENTERED}>
                    <LoginForm />
                  </div>
                }
              />
              <Route
                path="register"
                element={
                  <div className={CENTERED}>
                    <RegisterForm />
                  </div>
                }
              />

              <Route path="products" element={<ProductListPage />} />
              <Route path="products/:id" element={<ProductDetailPage />} />

              <Route
                path="cart"
                element={
                  <RequireRole roles={['Customer']}>
                    <CartPage />
                  </RequireRole>
                }
              />
              <Route
                path="checkout"
                element={
                  <RequireRole roles={['Customer']}>
                    <CheckoutPage />
                  </RequireRole>
                }
              />
              <Route
                path="orders"
                element={
                  <RequireRole roles={['Customer']}>
                    <OrdersListPage />
                  </RequireRole>
                }
              />
              <Route
                path="orders/:id"
                element={
                  <RequireRole roles={['Customer']}>
                    <OrderDetailPage />
                  </RequireRole>
                }
              />
              <Route
                path="profile"
                element={
                  <RequireRole roles={['Customer']}>
                    <ProfilePage />
                  </RequireRole>
                }
              />

              <Route
                path="admin/products"
                element={
                  <RequireRole roles={['Admin', 'ProductManager']}>
                    <AdminProductsPage />
                  </RequireRole>
                }
              />
              <Route
                path="admin/inventory"
                element={
                  <RequireRole roles={['Admin', 'ProductManager']}>
                    <AdminInventoryPage />
                  </RequireRole>
                }
              />
              <Route
                path="admin/customers"
                element={
                  <RequireRole roles={['Admin', 'Support']}>
                    <AdminCustomersPage />
                  </RequireRole>
                }
              />
              <Route
                path="admin/staff"
                element={
                  <RequireRole roles={['Admin']}>
                    <AdminStaffPage />
                  </RequireRole>
                }
              />

              <Route path="403" element={<ForbiddenPage />} />
              <Route path="*" element={<NotFoundPage />} />
            </Route>
          </Routes>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
