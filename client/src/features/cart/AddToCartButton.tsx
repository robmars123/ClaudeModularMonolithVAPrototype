import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { useCart } from './CartContext';
import { Button } from '../../components/ui/Button';
import { ApiError } from '../../api/httpClient';

export function AddToCartButton({ productId, disabled }: { productId: string; disabled?: boolean }) {
  const { isAuthenticated, user } = useAuth();
  const { addItem } = useCart();
  const navigate = useNavigate();
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [justAdded, setJustAdded] = useState(false);

  const isCustomer = user?.roles.includes('Customer') ?? false;

  const handleClick = async () => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    setError(null);
    setJustAdded(false);
    setPending(true);
    try {
      await addItem(productId, 1);
      setJustAdded(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not add item to cart.');
    } finally {
      setPending(false);
    }
  };

  if (isAuthenticated && !isCustomer) return null;

  return (
    <div className="flex flex-col items-start gap-1">
      <Button
        type="button"
        size="sm"
        pending={pending}
        disabled={disabled}
        onClick={() => void handleClick()}
      >
        {isAuthenticated ? 'Add to cart' : 'Log in to buy'}
      </Button>
      {justAdded && !error && <span className="text-xs text-emerald-600">Added to cart.</span>}
      {error && <span className="text-xs text-red-600">{error}</span>}
    </div>
  );
}
