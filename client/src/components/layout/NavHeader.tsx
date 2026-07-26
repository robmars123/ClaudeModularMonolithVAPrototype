import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { useCart } from '../../features/cart/CartContext';
import { Button } from '../ui/Button';

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `text-sm font-medium transition ${isActive ? 'text-slate-900' : 'text-slate-500 hover:text-slate-800'}`;

export function NavHeader() {
  const { user, isAuthenticated, logout } = useAuth();
  const { itemCount } = useCart();
  const navigate = useNavigate();

  const isCustomer = user?.roles.includes('Customer') ?? false;
  const isAdmin = user?.roles.includes('Admin') ?? false;
  const isProductManager = user?.roles.includes('ProductManager') ?? false;
  const isSupport = user?.roles.includes('Support') ?? false;

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-3">
        <div className="flex items-center gap-6">
          <NavLink to="/" className="text-base font-semibold text-slate-900">
            ECommerce
          </NavLink>
          <NavLink to="/products" className={linkClass}>
            Products
          </NavLink>
          {isCustomer && (
            <>
              <NavLink to="/orders" className={linkClass}>
                Orders
              </NavLink>
              <NavLink to="/profile" className={linkClass}>
                Profile
              </NavLink>
            </>
          )}
          {(isAdmin || isProductManager) && (
            <>
              <NavLink to="/admin/products" className={linkClass}>
                Manage Products
              </NavLink>
              <NavLink to="/admin/inventory" className={linkClass}>
                Inventory
              </NavLink>
            </>
          )}
          {(isAdmin || isSupport) && (
            <NavLink to="/admin/customers" className={linkClass}>
              Customers
            </NavLink>
          )}
          {isAdmin && (
            <NavLink to="/admin/staff" className={linkClass}>
              Staff
            </NavLink>
          )}
        </div>

        <div className="flex items-center gap-4">
          {isCustomer && (
            <NavLink to="/cart" className="text-sm font-medium text-slate-600 hover:text-slate-900">
              Cart{itemCount > 0 ? ` (${itemCount})` : ''}
            </NavLink>
          )}
          {isAuthenticated ? (
            <div className="flex items-center gap-3">
              <span className="text-sm text-slate-500">{user?.fullName}</span>
              <Button variant="secondary" size="sm" onClick={handleLogout}>
                Log out
              </Button>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <NavLink to="/login" className={linkClass}>
                Log in
              </NavLink>
              <NavLink
                to="/register"
                className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-700"
              >
                Register
              </NavLink>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
