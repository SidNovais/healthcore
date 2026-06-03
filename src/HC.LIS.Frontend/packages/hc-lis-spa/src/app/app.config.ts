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
import { ORDERS_PORT } from './core/application/i-orders-port';
import { ORDERS_API } from './core/infrastructure/orders/i-orders-api';
import { SdkOrdersApi } from './core/infrastructure/orders/sdk-orders-api';
import { SdkOrdersAdapter } from './core/infrastructure/orders/sdk-orders-adapter';
import { COLLECTION_REQUESTS_PORT } from './core/application/i-collection-requests-port';
import { COLLECTION_REQUESTS_API } from './core/infrastructure/collection-requests/i-collection-requests-api';
import { SdkCollectionRequestsApi } from './core/infrastructure/collection-requests/sdk-collection-requests-api';
import { SdkCollectionRequestsAdapter } from './core/infrastructure/collection-requests/sdk-collection-requests-adapter';
import { WORKLIST_PORT } from './core/application/i-worklist-port';
import { WORKLIST_API } from './core/infrastructure/worklist/i-worklist-api';
import { SdkWorklistApi } from './core/infrastructure/worklist/sdk-worklist-api';
import { SdkWorklistAdapter } from './core/infrastructure/worklist/sdk-worklist-adapter';
import { USERS_PORT } from './core/application/i-users-port';
import { USERS_API } from './core/infrastructure/users/i-users-api';
import { SdkUsersApi } from './core/infrastructure/users/sdk-users-api';
import { SdkUsersAdapter } from './core/infrastructure/users/sdk-users-adapter';
import { PATIENTS_PORT } from './core/application/i-patients-port';
import { SdkPatientsAdapter } from './core/infrastructure/patients/sdk-patients-adapter';

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
    { provide: ORDERS_API, useClass: SdkOrdersApi },
    { provide: ORDERS_PORT, useClass: SdkOrdersAdapter },
    { provide: COLLECTION_REQUESTS_API, useClass: SdkCollectionRequestsApi },
    { provide: COLLECTION_REQUESTS_PORT, useClass: SdkCollectionRequestsAdapter },
    { provide: WORKLIST_API, useClass: SdkWorklistApi },
    { provide: WORKLIST_PORT, useClass: SdkWorklistAdapter },
    { provide: USERS_API, useClass: SdkUsersApi },
    { provide: USERS_PORT, useClass: SdkUsersAdapter },
    { provide: PATIENTS_PORT, useClass: SdkPatientsAdapter },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AuthService],
      multi: true,
    },
  ],
};
