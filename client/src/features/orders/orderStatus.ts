type BadgeTone = 'neutral' | 'success' | 'warning' | 'danger';

/** Maps the backend's Order.Status string to a badge tone. Falls back to neutral for unrecognized values. */
export function orderStatusTone(status: string): BadgeTone {
  const normalized = status.toLowerCase();
  if (['cancelled', 'canceled', 'failed'].includes(normalized)) return 'danger';
  if (['delivered', 'completed', 'placed'].includes(normalized)) return 'success';
  if (['pending', 'processing', 'shipped'].includes(normalized)) return 'warning';
  return 'neutral';
}
