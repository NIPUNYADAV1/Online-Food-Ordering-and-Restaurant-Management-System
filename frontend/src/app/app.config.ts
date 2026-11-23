import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      (request, next) => {
        const saved = localStorage.getItem('foodapp-session');
        if (!saved) return next(request);
        try {
          const token = (JSON.parse(saved) as { token?: string }).token;
          return next(token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request);
        } catch {
          localStorage.removeItem('foodapp-session');
          return next(request);
        }
      },
    ])),
  ]
};
