export interface Product {
  id: string;
  name: string;
  description: string;
  sku: string;
  price: number;
  imageUrl: string | null;
  isActive: boolean;
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface Category {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface ProductListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  isActive?: boolean;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  categoryId: string;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  id: string;
  name: string;
  description: string;
  price: number;
  categoryId: string;
  imageUrl?: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}
