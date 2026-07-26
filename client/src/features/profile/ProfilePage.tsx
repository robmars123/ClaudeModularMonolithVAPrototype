import { useEffect, useState, type FormEvent } from 'react';
import { useProfile } from './useProfile';
import { updateMyProfile, updateShippingAddress } from './api';
import type { Address } from './types';
import { ApiError } from '../../api/httpClient';
import { Card } from '../../components/ui/Card';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Spinner, ErrorMessage } from '../../components/ui/Feedback';

const EMPTY_ADDRESS: Address = { street: '', city: '', state: '', zipCode: '', country: '' };

export function ProfilePage() {
  const { state, reload, setState } = useProfile();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [profileSubmitting, setProfileSubmitting] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [profileSaved, setProfileSaved] = useState(false);

  const [address, setAddress] = useState<Address>(EMPTY_ADDRESS);
  const [addressSubmitting, setAddressSubmitting] = useState(false);
  const [addressError, setAddressError] = useState<string | null>(null);
  const [addressSaved, setAddressSaved] = useState(false);

  useEffect(() => {
    if (state.status !== 'ready') return;
    setFirstName(state.data.firstName);
    setLastName(state.data.lastName);
    setPhone(state.data.phone ?? '');
    setAddress(state.data.shippingAddress ?? EMPTY_ADDRESS);
  }, [state]);

  if (state.status === 'loading') return <Spinner label="Loading your profile…" />;
  if (state.status === 'error') return <ErrorMessage message={state.message} />;

  const handleProfileSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setProfileSubmitting(true);
    setProfileError(null);
    setProfileSaved(false);
    try {
      const updated = await updateMyProfile({ firstName, lastName, phone: phone || undefined });
      setState({ status: 'ready', data: updated });
      setProfileSaved(true);
    } catch (err) {
      setProfileError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setProfileSubmitting(false);
    }
  };

  const handleAddressSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setAddressSubmitting(true);
    setAddressError(null);
    setAddressSaved(false);
    try {
      await updateShippingAddress(address);
      setAddressSaved(true);
      reload();
    } catch (err) {
      setAddressError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setAddressSubmitting(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-lg flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Your profile</h1>

      <Card>
        <h2 className="text-sm font-medium text-slate-700">Personal details</h2>
        <form onSubmit={(e) => void handleProfileSubmit(e)} className="mt-4 flex flex-col gap-4">
          <p className="text-sm text-slate-500">{state.data.email}</p>
          <div className="grid grid-cols-2 gap-3">
            <Input
              id="firstName"
              label="First name"
              required
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
            />
            <Input
              id="lastName"
              label="Last name"
              required
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
            />
          </div>
          <Input id="phone" label="Phone (optional)" value={phone} onChange={(e) => setPhone(e.target.value)} />

          {profileError && <ErrorMessage message={profileError} />}
          {profileSaved && !profileError && <p className="text-sm text-emerald-600">Profile updated.</p>}

          <Button type="submit" pending={profileSubmitting} className="w-full">
            Save profile
          </Button>
        </form>
      </Card>

      <Card>
        <h2 className="text-sm font-medium text-slate-700">Shipping address</h2>
        <form onSubmit={(e) => void handleAddressSubmit(e)} className="mt-4 flex flex-col gap-4">
          <Input
            id="street"
            label="Street"
            required
            value={address.street}
            onChange={(e) => setAddress({ ...address, street: e.target.value })}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input
              id="city"
              label="City"
              required
              value={address.city}
              onChange={(e) => setAddress({ ...address, city: e.target.value })}
            />
            <Input
              id="state"
              label="State"
              required
              value={address.state}
              onChange={(e) => setAddress({ ...address, state: e.target.value })}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input
              id="zipCode"
              label="ZIP code"
              required
              value={address.zipCode}
              onChange={(e) => setAddress({ ...address, zipCode: e.target.value })}
            />
            <Input
              id="country"
              label="Country"
              required
              value={address.country}
              onChange={(e) => setAddress({ ...address, country: e.target.value })}
            />
          </div>

          {addressError && <ErrorMessage message={addressError} />}
          {addressSaved && !addressError && <p className="text-sm text-emerald-600">Address updated.</p>}

          <Button type="submit" pending={addressSubmitting} className="w-full">
            Save address
          </Button>
        </form>
      </Card>
    </div>
  );
}
