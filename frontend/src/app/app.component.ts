import { CommonModule } from '@angular/common';
import { Component, NgZone, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { map } from 'rxjs';
import { ApiService } from './api.service';
import {
  Cart, FoodItem, Order, PaymentOrder, Restaurant, SessionUser,
} from './models';

declare global {
  interface Window {
    Razorpay?: new (options: Record<string, unknown>) => { open(): void };
  }
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  restaurants: Restaurant[] = [];
  cart: Cart = { id: 0, items: [], total: 0 };
  orders: Order[] = [];
  confirmedOrder: Order | null = null;
  owners: {
    id: number;
    name: string;
    email: string;
    assignment: { restaurantId: number; restaurantName: string } | null;
  }[] = [];
  user: SessionUser | null = this.readSession();
  search = '';
  category = '';
  activeView: 'menu' | 'cart' | 'orders' | 'manage' | 'confirmation' = 'menu';
  authMode: 'login' | 'register' = 'login';
  authForm = { name: '', email: '', password: '' };
  restaurantForm = { name: '', description: '', cuisine: '', address: '', imageUrl: '' };
  ownerForm = { name: '', email: '', password: '' };
  foodForm = { name: '', description: '', category: '', price: 0, imageUrl: '', isAvailable: true };
  selectedRestaurantId = 0;
  ownerIdByRestaurant: Partial<Record<number, number>> = {};
  editingRestaurantId = 0;
  editingFoodItemId = 0;
  message = '';
  error = '';
  busy = false;

  constructor(
    private readonly api: ApiService,
    private readonly zone: NgZone,
  ) {}

  ngOnInit(): void {
    this.loadRestaurants();
    if (this.user?.role === 'Customer') {
      this.loadCart();
      this.loadOrders();
    } else if (this.user) {
      this.loadManagementOrders();
      if (this.user.role === 'Admin') this.loadOwners();
    }
  }

  get foodItems(): FoodItem[] {
    return this.restaurants.flatMap((restaurant) => restaurant.foodItems);
  }

  get categories(): string[] {
    return [...new Set(this.foodItems.map((item) => item.category))].sort();
  }

  get cartCount(): number {
    return this.cart.items.reduce((total, item) => total + item.quantity, 0);
  }

  get isAdmin(): boolean {
    return this.user?.role === 'Admin';
  }

  loadRestaurants(): void {
    this.api.restaurants(this.search, this.category).subscribe({
      next: (restaurants) => { this.restaurants = restaurants; },
      error: (error) => this.showError(error),
    });
  }

  applyFilters(): void {
    this.loadRestaurants();
  }

  loginOrRegister(): void {
    this.clearFeedback();
    this.busy = true;
    const request = this.authMode === 'login'
      ? this.api.login(this.authForm.email, this.authForm.password)
      : this.api.register(this.authForm.name, this.authForm.email, this.authForm.password);
    request.subscribe({
      next: (user) => {
        this.user = user;
        localStorage.setItem('foodapp-session', JSON.stringify(user));
        this.authForm = { name: '', email: '', password: '' };
        this.busy = false;
        this.message = `Welcome${user.name ? `, ${user.name}` : ''}!`;
        this.loadAfterLogin();
      },
      error: (error) => { this.busy = false; this.showError(error); },
    });
  }

  logout(): void {
    localStorage.removeItem('foodapp-session');
    this.user = null;
    this.cart = { id: 0, items: [], total: 0 };
    this.orders = [];
    this.confirmedOrder = null;
    this.activeView = 'menu';
    this.message = 'You are signed out.';
    this.error = '';
  }

  addToCart(item: FoodItem): void {
    if (!this.user) {
      this.error = 'Sign in to add items to your cart.';
      return;
    }
    if (this.user.role !== 'Customer') {
      this.error = 'Customer accounts are required to place orders.';
      return;
    }
    this.api.addToCart(item.id).subscribe({
      next: (cart) => { this.cart = cart; this.error = ''; this.message = `${item.name} added to your cart.`; },
      error: (error) => this.showError(error),
    });
  }

  changeQuantity(itemId: number, quantity: number): void {
    if (quantity < 1) {
      this.removeFromCart(itemId);
      return;
    }
    this.api.updateCartItem(itemId, quantity).subscribe({
      next: (cart) => { this.cart = cart; this.error = ''; },
      error: (error) => this.showError(error),
    });
  }

  removeFromCart(itemId: number): void {
    this.api.removeCartItem(itemId).subscribe({
      next: (cart) => { this.cart = cart; this.error = ''; },
      error: (error) => this.showError(error),
    });
  }

  clearCart(): void {
    this.api.clearCart().subscribe({
      next: (cart) => { this.cart = cart; this.message = 'Your cart is empty.'; this.error = ''; },
      error: (error) => this.showError(error),
    });
  }

  checkout(): void {
    this.clearFeedback();
    this.busy = true;
    this.api.createPaymentOrder().subscribe({
      next: async (order) => {
        this.busy = false;
        try {
          await this.loadRazorpay();
          this.openRazorpay(order);
        } catch {
          this.error = 'Unable to load the Razorpay test checkout. Check your connection and try again.';
        }
      },
      error: (error) => { this.busy = false; this.showError(error); },
    });
  }

  createRestaurant(): void {
    const request = this.editingRestaurantId
      ? this.api.updateRestaurant(this.editingRestaurantId, this.restaurantForm).pipe(map(() => undefined))
      : this.api.createRestaurant(this.restaurantForm).pipe(map(() => undefined));
    request.subscribe({
      next: () => {
        this.editingRestaurantId = 0;
        this.restaurantForm = { name: '', description: '', cuisine: '', address: '', imageUrl: '' };
        this.message = 'Restaurant saved.';
        this.loadRestaurants();
      },
      error: (error) => this.showError(error),
    });
  }

  editRestaurant(restaurant: Restaurant): void {
    this.editingRestaurantId = restaurant.id;
    this.restaurantForm = {
      name: restaurant.name,
      description: restaurant.description,
      cuisine: restaurant.cuisine,
      address: restaurant.address,
      imageUrl: restaurant.imageUrl ?? '',
    };
  }

  cancelRestaurantEdit(): void {
    this.editingRestaurantId = 0;
    this.restaurantForm = { name: '', description: '', cuisine: '', address: '', imageUrl: '' };
  }

  createOwner(): void {
    this.api.createOwner(this.ownerForm).subscribe({
      next: (result) => {
        this.ownerForm = { name: '', email: '', password: '' };
        this.message = result.message;
        this.loadOwners();
      },
      error: (error) => this.showError(error),
    });
  }

  assignOwner(restaurantId: number): void {
    const ownerId = this.ownerIdByRestaurant[restaurantId];
    if (!ownerId) {
      this.error = 'Choose an owner for this restaurant first.';
      return;
    }
    this.api.assignOwner(restaurantId, ownerId).subscribe({
      next: () => {
        this.error = '';
        this.message = 'Owner assigned to restaurant.';
        this.loadOwners();
      },
      error: (error) => this.showError(error),
    });
  }

  unassignOwner(restaurantId: number): void {
    this.api.assignOwner(restaurantId, null).subscribe({
      next: () => {
        delete this.ownerIdByRestaurant[restaurantId];
        this.error = '';
        this.message = 'Owner unassigned. They are now available for another restaurant.';
        this.loadOwners();
      },
      error: (error) => this.showError(error),
    });
  }

  ownerAssignedElsewhere(ownerId: number, restaurantId: number): boolean {
    const owner = this.owners.find((candidate) => candidate.id === ownerId);
    return owner?.assignment !== null
      && owner?.assignment !== undefined
      && owner.assignment.restaurantId !== restaurantId;
  }

  createFoodItem(): void {
    if (!this.selectedRestaurantId) {
      this.error = 'Select a restaurant first.';
      return;
    }
    const request = this.editingFoodItemId
      ? this.api.updateFoodItem(this.editingFoodItemId, this.foodForm)
      : this.api.createFoodItem(this.selectedRestaurantId, this.foodForm);
    request.subscribe({
      next: () => {
        this.editingFoodItemId = 0;
        this.foodForm = { name: '', description: '', category: '', price: 0, imageUrl: '', isAvailable: true };
        this.message = 'Menu item saved.';
        this.loadRestaurants();
      },
      error: (error) => this.showError(error),
    });
  }

  editFoodItem(item: FoodItem): void {
    this.editingFoodItemId = item.id;
    this.selectedRestaurantId = item.restaurantId;
    this.foodForm = {
      name: item.name,
      description: item.description,
      category: item.category,
      price: item.price,
      imageUrl: item.imageUrl ?? '',
      isAvailable: item.isAvailable,
    };
  }

  cancelFoodEdit(): void {
    this.editingFoodItemId = 0;
    this.foodForm = { name: '', description: '', category: '', price: 0, imageUrl: '', isAvailable: true };
  }

  removeFoodItem(itemId: number): void {
    this.api.removeFoodItem(itemId).subscribe({
      next: () => { this.message = 'Menu item removed.'; this.loadRestaurants(); },
      error: (error) => this.showError(error),
    });
  }

  deactivateRestaurant(id: number): void {
    this.api.deactivateRestaurant(id).subscribe({
      next: () => { this.message = 'Restaurant deactivated.'; this.loadRestaurants(); },
      error: (error) => this.showError(error),
    });
  }

  advanceOrder(order: Order): void {
    const next = this.nextStatusFor(order.status);
    if (!next) return;
    this.api.updateOrderStatus(order.id, next).subscribe({
      next: () => { this.message = `Order #${order.id} updated to ${next}.`; this.loadManagementOrders(); },
      error: (error) => this.showError(error),
    });
  }

  refreshOrders(): void {
    if (this.user?.role === 'Customer') this.loadOrders();
    else this.loadManagementOrders();
  }

  trackConfirmedOrder(): void {
    this.activeView = 'orders';
    this.loadOrders();
  }

  handlePaymentSuccess(response: {
    razorpay_order_id: string;
    razorpay_payment_id: string;
    razorpay_signature: string;
  }): void {
    this.zone.run(() => {
      this.api.verifyPayment(response).subscribe({
        next: (placed) => this.zone.run(() => {
          this.confirmedOrder = placed;
          this.message = '';
          this.error = '';
          this.activeView = 'confirmation';
          this.loadCart();
          this.loadOrders();
        }),
        error: (error) => this.zone.run(() => this.showError(error)),
      });
    });
  }

  nextStatusFor(status: string): string | null {
    return ({
      Placed: 'Confirmed',
      Confirmed: 'Preparing',
      Preparing: 'Ready',
      Ready: 'OutForDelivery',
      OutForDelivery: 'Delivered',
    } as Record<string, string>)[status] ?? null;
  }

  private loadAfterLogin(): void {
    if (this.user?.role === 'Customer') {
      this.activeView = 'menu';
      this.loadCart();
      this.loadOrders();
    } else {
      this.activeView = 'manage';
      this.loadManagementOrders();
      if (this.user?.role === 'Admin') this.loadOwners();
    }
  }

  private loadCart(): void {
    this.api.cart().subscribe({
      next: (cart) => { this.cart = cart; },
      error: (error) => this.showError(error),
    });
  }

  private loadOrders(): void {
    this.api.orders().subscribe({
      next: (orders) => { this.orders = orders; },
      error: (error) => this.showError(error),
    });
  }

  private loadManagementOrders(): void {
    if (!this.user || this.user.role === 'Customer') return;
    this.api.managementOrders().subscribe({
      next: (orders) => { this.orders = orders; },
      error: (error) => this.showError(error),
    });
  }

  private loadOwners(): void {
    this.api.owners().subscribe({
      next: (owners) => {
        this.owners = owners;
        this.ownerIdByRestaurant = Object.fromEntries(
          owners
            .filter((owner) => owner.assignment !== null)
            .map((owner) => [owner.assignment!.restaurantId, owner.id]),
        );
      },
      error: (error) => this.showError(error),
    });
  }

  private loadRazorpay(): Promise<void> {
    if (window.Razorpay) return Promise.resolve();
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Razorpay checkout could not be loaded.'));
      document.body.appendChild(script);
    });
  }

  private openRazorpay(order: PaymentOrder): void {
    if (!window.Razorpay) {
      this.error = 'Razorpay checkout is unavailable.';
      return;
    }
    const checkout = new window.Razorpay({
      key: order.keyId,
      amount: order.amount,
      currency: order.currency,
      name: 'Goodfood',
      description: `Order #${order.orderId}`,
      order_id: order.razorpayOrderId,
      handler: (response: { razorpay_order_id: string; razorpay_payment_id: string; razorpay_signature: string }) =>
        this.handlePaymentSuccess(response),
      modal: { ondismiss: () => this.zone.run(() => {
        this.message = 'Payment window closed. You can try again from your cart.';
      }) },
      theme: { color: '#ed5a3d' },
    });
    checkout.open();
  }

  private readSession(): SessionUser | null {
    const saved = localStorage.getItem('foodapp-session');
    if (!saved) return null;
    try {
      return JSON.parse(saved) as SessionUser;
    } catch {
      localStorage.removeItem('foodapp-session');
      return null;
    }
  }

  private showError(error: {
    status?: number;
    error?: {
      message?: string;
      title?: string;
      detail?: string;
      errors?: Record<string, string[]>;
    } | string;
    message?: string;
  }): void {
    if (error.status === 0) {
      this.error = 'Cannot reach the API. Start the backend and confirm it is running at http://localhost:5000.';
      return;
    }
    if (typeof error.error === 'string') {
      this.error = error.error;
      return;
    }

    const validationErrors = Object.values(error.error?.errors ?? {}).flat();
    this.error = error.error?.message
      ?? (validationErrors.length ? validationErrors.join(' ') : undefined)
      ?? error.error?.detail
      ?? error.error?.title
      ?? error.message
      ?? 'Something went wrong. Please try again.';
  }

  private clearFeedback(): void {
    this.message = '';
    this.error = '';
  }
}

// admin@example.com
// ChooseAStrongPassword123!