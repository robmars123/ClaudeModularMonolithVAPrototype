import { useState, type FormEvent } from 'react';
import { createCategory } from '../../products/api';
import { ApiError } from '../../../api/httpClient';
import { Input } from '../../../components/ui/Input';
import { Button } from '../../../components/ui/Button';
import { ErrorMessage } from '../../../components/ui/Feedback';

export function CategoryQuickCreate({ onCreated }: { onCreated: () => void }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await createCategory({ name, description: description || undefined });
      setName('');
      setDescription('');
      onCreated();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not reach the server.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={(e) => void handleSubmit(e)} className="flex flex-wrap items-end gap-3">
      <Input id="categoryName" label="New category name" required value={name} onChange={(e) => setName(e.target.value)} />
      <Input
        id="categoryDescription"
        label="Description (optional)"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
      />
      <Button type="submit" variant="secondary" pending={submitting}>
        Add category
      </Button>
      {error && <ErrorMessage message={error} />}
    </form>
  );
}
