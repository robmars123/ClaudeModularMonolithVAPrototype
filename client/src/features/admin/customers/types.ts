export interface CustomerListItem {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string | null;
  shippingAddress: unknown | null;
  createdAt: string;
}
