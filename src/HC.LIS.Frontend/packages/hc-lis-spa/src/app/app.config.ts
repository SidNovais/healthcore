import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { configureClient } from '@hc-lis/api-client';
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { AUTH_PORT } from './core/application/i-auth-port';
import { AUTH_API } from './core/infrastructure/auth/i-auth-api';
import { SdkAuthApi } from './core/infrastructure/auth/sdk-auth-api';
import { SdkAuthAdapter } from './core/infrastructure/auth/sdk-auth-adapter';
import { AuthService } from './core/application/auth.service';

function initializeApp(authService: AuthService): () => Promise<void> {
  return () => {
    configureClient(environment.apiUrl);
    return authService.me();
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    { provide: AUTH_API, useClass: SdkAuthApi },
    { provide: AUTH_PORT, useClass: SdkAuthAdapter },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AuthService],
      multi: true,
    },
  ],
};
