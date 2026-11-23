
/// <reference types="jasmine" />

import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiService } from './api.service';
import { AppComponent } from './app.component';

describe('AppComponent', () => {

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],

      providers: [
        {
          provide: ApiService,
          useValue: {
            restaurants: () => of([]),

            verifyPayment: () => of({
              id: 42,
              status: 'Placed',
              paymentStatus: 'Paid',
              totalAmount: 12.5,
              createdAt: '2026-11-10T00:00:00Z',

              items: [
                {
                  foodItemId: 7,
                  name: 'Avocado Grain Bowl',
                  unitPrice: 12.5,
                  quantity: 1
                }
              ]
            }),

            cart: () => of({
              id: 1,
              items: [],
              total: 0
            }),

            orders: () => of([])
          }
        }
      ]
    }).compileComponents();
  });


  // TEST 1: Component creation
  it('creates the storefront', () => {

    const fixture = TestBed.createComponent(AppComponent);

    expect(fixture.componentInstance).toBeTruthy();

  });


  // TEST 2: Restaurant discovery page
  it('renders the restaurant discovery page', () => {

    const fixture = TestBed.createComponent(AppComponent);

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('h1')?.textContent)
      .toContain('great meal');

  });


  // TEST 3: Payment confirmation page
  it('shows a dedicated confirmation page for a verified payment', () => {

    const fixture = TestBed.createComponent(AppComponent);

    const app = fixture.componentInstance;

    app.activeView = 'confirmation';

    app.confirmedOrder = {
      id: 42,
      status: 'Placed',
      paymentStatus: 'Paid',
      totalAmount: 12.5,
      createdAt: '2026-11-10T00:00:00Z',

      items: [
        {
          foodItemId: 7,
          name: 'Avocado Grain Bowl',
          unitPrice: 12.5,
          quantity: 1
        }
      ]
    };

    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(
      compiled.querySelector('.confirmation-card h1')?.textContent
    ).toContain('Congratulations!');

    expect(
      compiled.querySelector('.confirmation-order-number')?.textContent
    ).toContain('#42');

    expect(
      compiled.querySelector('.confirmation-item')?.textContent
    ).toContain('Avocado Grain Bowl');

  });


  // TEST 4: Automatic navigation after payment verification
  it('automatically switches to the confirmation page after payment verification', async () => {

    const fixture = TestBed.createComponent(AppComponent);

    const app = fixture.componentInstance;

    app.activeView = 'cart';

    await app.handlePaymentSuccess({
      razorpay_order_id: 'order_test_42',
      razorpay_payment_id: 'pay_test_42',
      razorpay_signature: 'signature'
    });

    fixture.detectChanges();

    expect(app.activeView).toBe('confirmation');

    expect(
      fixture.nativeElement.querySelector('.confirmation-card h1')?.textContent
    ).toContain('Congratulations!');

  });

});
