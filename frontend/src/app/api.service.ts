import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Cart, Order, PaymentOrder, Restaurant, SessionUser } from './models';

const API_URL = 'http://localhost:5000/api';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  restaurants(search = '', category = ''): Observable<Restaurant[]> {
    const params: Record<string, string> = {};
    if (search.trim()) params['search'] = search.trim();
    if (category) params['category'] = category;
    return this.http.get<Restaurant[]>(`${API_URL}/restaurants`, { params });
  }

  login(email: string, password: string): Observable<SessionUser> {
    return this.http.post<SessionUser>(`${API_URL}/auth/login`, { email, password });
  }

  register(name: string, email: string, password: string): Observable<SessionUser> {
    return this.http.post<SessionUser>(`${API_URL}/auth/register`, { name, email, password });
  }

  cart(): Observable<Cart> {
    return this.http.get<Cart>(`${API_URL}/cart`);
  }

  addToCart(foodItemId: number): Observable<Cart> {
    return this.http.post<Cart>(`${API_URL}/cart/items`, { foodItemId, quantity: 1 });
  }

  updateCartItem(id: number, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${API_URL}/cart/items/${id}`, { quantity });
  }

  removeCartItem(id: number): Observable<Cart> {
    return this.http.delete<Cart>(`${API_URL}/cart/items/${id}`);
  }

  clearCart(): Observable<Cart> {
    return this.http.delete<Cart>(`${API_URL}/cart`);
  }

  orders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${API_URL}/orders`);
  }

  managementOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${API_URL}/orders/management`);
  }

  createPaymentOrder(): Observable<PaymentOrder> {
    return this.http.post<PaymentOrder>(`${API_URL}/orders/payment/order`, {});
  }

  verifyPayment(payment: { razorpay_order_id: string; razorpay_payment_id: string; razorpay_signature: string }): Observable<Order> {
    return this.http.post<Order>(`${API_URL}/orders/payment/verify`, {
      razorpayOrderId: payment.razorpay_order_id,
      razorpayPaymentId: payment.razorpay_payment_id,
      razorpaySignature: payment.razorpay_signature,
    });
  }

  updateOrderStatus(id: number, status: string): Observable<Order> {
    return this.http.put<Order>(`${API_URL}/orders/${id}/status`, { status });
  }

  createRestaurant(body: object): Observable<Restaurant> {
    return this.http.post<Restaurant>(`${API_URL}/admin/restaurants`, body);
  }

  updateRestaurant(id: number, body: object): Observable<void> {
    return this.http.put<void>(`${API_URL}/admin/restaurants/${id}`, body);
  }

  deactivateRestaurant(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/admin/restaurants/${id}`);
  }

  createOwner(body: object): Observable<{ wasPromoted: boolean; message: string }> {
    return this.http.post<{ wasPromoted: boolean; message: string }>(`${API_URL}/admin/owners`, body);
  }

  owners(): Observable<{
    id: number;
    name: string;
    email: string;
    assignment: { restaurantId: number; restaurantName: string } | null;
  }[]> {
    return this.http.get<{
      id: number;
      name: string;
      email: string;
      assignment: { restaurantId: number; restaurantName: string } | null;
    }[]>(`${API_URL}/admin/owners`);
  }

  assignOwner(restaurantId: number, ownerId: number | null): Observable<void> {
    return this.http.put<void>(`${API_URL}/admin/restaurants/${restaurantId}/owner`, { ownerId });
  }

  createFoodItem(restaurantId: number, body: object): Observable<void> {
    return this.http.post<void>(`${API_URL}/admin/restaurants/${restaurantId}/food-items`, body);
  }

  updateFoodItem(id: number, body: object): Observable<void> {
    return this.http.put<void>(`${API_URL}/admin/food-items/${id}`, body);
  }

  removeFoodItem(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/admin/food-items/${id}`);
  }
}
