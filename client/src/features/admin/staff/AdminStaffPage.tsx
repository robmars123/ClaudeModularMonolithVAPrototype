import { useState, type FormEvent } from 'react';
import { createStaffAccount } from '../../../auth/authApi';
import type { CreateStaffRequest, StaffUserResponse } from '../../../auth/types';
import { ApiError } from '../../../api/httpClient';
import { Card } from '../../../components/ui/Card';
import { Input, Select } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';
import { ErrorMessage } from '../../../components/ui/Feedback';

const STAFF_ROLES: CreateStaffRequest['role'][] = ['Admin', 'ProductManager', 'Support'];

export function AdminStaffPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState<CreateStaffRequest['role']>('ProductManager');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [created, setCreated] = useState<StaffUserResponse | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    setCreated(null);
    try {
      const result = await createStaffAccount({ firstName, lastName, email, password, role });
      setCreated(result);
      setFirstName('');
      setLastName('');
      setEmail('');
      setPassword('');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-lg flex-col gap-6">
      <h1 className="text-2xl font-semibold text-slate-900">Provision staff account</h1>

      <Card>
        <form onSubmit={(e) => void handleSubmit(e)} className="flex flex-col gap-4">
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
          <Input
            id="email"
            label="Email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <Input
            id="password"
            label="Temporary password"
            type="password"
            required
            minLength={6}
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <Select
            id="role"
            label="Role"
            value={role}
            onChange={(e) => setRole(e.target.value as CreateStaffRequest['role'])}
          >
            {STAFF_ROLES.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </Select>

          {error && <ErrorMessage message={error} />}
          {created && (
            <p className="text-sm text-emerald-600">
              Created {created.fullName} ({created.email}) as {created.role}.
            </p>
          )}

          <Button type="submit" pending={submitting} className="w-full">
            Create account
          </Button>
        </form>
      </Card>
    </div>
  );
}
