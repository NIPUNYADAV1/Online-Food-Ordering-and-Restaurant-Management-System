export interface SessionUser {
  id: number;
  name: string;
  email: string;
  role: 'Customer' | 'Owner' | 'Admin';
  token: string;
}

export interface FoodItem {
  id: number;
  restaurantId: number;
  restaurantName: string;
  name: string;
  description: string;
  category: string;
  price: number;
  imageUrl: string | null;
  isAvailable: boolean;
}

export interface Restaurant {
  id: number;
  name: string;
  description: string;
  cuisine: string;
  address: string;
  imageUrl: string | null;
  foodItems: FoodItem[];
}

export interface CartItem {
  id: number;
  foodItemId: number;
  name: string;
  restaurantName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  total: number;
}

export interface OrderItem {
  foodItemId: number;
  name: string;
  unitPrice: number;
  quantity: number;
}

export interface Order {
  id: number;
  status: string;
  paymentStatus: string;
  totalAmount: number;
  createdAt: string;
  items: OrderItem[];
}

export interface PaymentOrder {
  orderId: number;
  razorpayOrderId: string;
  keyId: string;
  amount: number;
  currency: string;
}
