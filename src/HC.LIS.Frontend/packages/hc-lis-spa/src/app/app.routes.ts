import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { ShellComponent } from './core/shell/shell.component';
import { NotFoundComponent } from './core/shell/not-found.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'orders/new',
        canActivate: [roleGuard('Receptionist', 'ITAdmin')],
        loadComponent: () =>
          import('./features/orders/new-order.component').then(m => m.NewOrderComponent),
      },
      {
        path: 'orders/:id',
        canActivate: [roleGuard('Receptionist', 'Physician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/orders/order-detail.component').then(m => m.OrderDetailComponent),
      },
      {
        path: 'orders',
        canActivate: [roleGuard('Receptionist', 'Physician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/orders/order-list.component').then(m => m.OrderListComponent),
      },
      {
        path: 'triage',
        canActivate: [roleGuard('LabTechnician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/triage/triage.component').then(m => m.TriageComponent),
      },
      {
        path: 'waiting-room',
        redirectTo: 'triage',
        pathMatch: 'full',
      },
      {
        path: 'worklist',
        canActivate: [roleGuard('Physician', 'ITAdmin')],
        loadComponent: () =>
          import('./features/worklist/worklist.component').then(m => m.WorklistComponent),
      },
      {
        path: 'admin/users',
        canActivate: [roleGuard('ITAdmin')],
        loadComponent: () =>
          import('./features/admin/user-list.component').then(m => m.UserListComponent),
      },
      {
        path: 'unauthorized',
        loadComponent: () =>
          import('./core/shell/unauthorized.component').then(m => m.UnauthorizedComponent),
      },
    ],
  },
  {
    path: '**',
    component: NotFoundComponent,
  },
];
