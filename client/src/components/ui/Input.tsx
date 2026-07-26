import { forwardRef, type InputHTMLAttributes, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react';

interface FieldProps {
  label?: string;
  error?: string;
  hint?: string;
}

const FIELD_CLASSES =
  'mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm outline-none focus:border-slate-500 focus:ring-1 focus:ring-slate-500 disabled:bg-slate-50 disabled:text-slate-400';

export const Input = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement> & FieldProps>(
  function Input({ label, error, hint, id, className = '', ...rest }, ref) {
    return (
      <div>
        {label && (
          <label className="block text-sm font-medium text-slate-700" htmlFor={id}>
            {label}
          </label>
        )}
        <input ref={ref} id={id} className={`${FIELD_CLASSES} ${className}`} {...rest} />
        {hint && !error && <p className="mt-1 text-xs text-slate-500">{hint}</p>}
        {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
      </div>
    );
  },
);

export const Textarea = forwardRef<
  HTMLTextAreaElement,
  TextareaHTMLAttributes<HTMLTextAreaElement> & FieldProps
>(function Textarea({ label, error, hint, id, className = '', ...rest }, ref) {
  return (
    <div>
      {label && (
        <label className="block text-sm font-medium text-slate-700" htmlFor={id}>
          {label}
        </label>
      )}
      <textarea ref={ref} id={id} className={`${FIELD_CLASSES} ${className}`} {...rest} />
      {hint && !error && <p className="mt-1 text-xs text-slate-500">{hint}</p>}
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
});

export const Select = forwardRef<
  HTMLSelectElement,
  SelectHTMLAttributes<HTMLSelectElement> & FieldProps
>(function Select({ label, error, hint, id, className = '', children, ...rest }, ref) {
  return (
    <div>
      {label && (
        <label className="block text-sm font-medium text-slate-700" htmlFor={id}>
          {label}
        </label>
      )}
      <select ref={ref} id={id} className={`${FIELD_CLASSES} bg-white ${className}`} {...rest}>
        {children}
      </select>
      {hint && !error && <p className="mt-1 text-xs text-slate-500">{hint}</p>}
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
});
