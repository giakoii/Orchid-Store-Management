export interface Orchid {
  orchidId: number;
  orchidName: string;
  orchidDescription: string;
  orchidUrl: string;
  price: number;
  isNatural: boolean;
  categoryId: number;
  categoryName: string;
  createdAt: string;
  createdBy: string | null;
}

export interface Category {
  categoryId: number;
  categoryName: string;
  parentCategoryId: number | null;
  parentCategoryName: string | null;
  orchidCount: number;
  childCategories: Category[];
}

export interface OrchidResponse {
  response: {
    orchids: Orchid[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  };
  success: boolean;
  messageId: string;
  message: string;
  detailErrorList: any;
}

export interface CategoryResponse {
  response: {
    categories: Category[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  };
  success: boolean;
  messageId: string;
  message: string;
  detailErrorList: any;
}
