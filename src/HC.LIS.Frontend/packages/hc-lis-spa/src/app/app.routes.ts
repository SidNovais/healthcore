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
        canActivate: [roleGuard('Receptionist')],
        loadComponent: () =>
          import('./features/orders/new-order.component').then(m => m.NewOrderComponent),
      },
      {
        path: 'waiting-room',
        canActivate: [roleGuard('LabTechnician')],
        loadComponent: () =>
          import('./features/waiting-room/waiting-room.component').then(m => m.WaitingRoomComponent),
      },
      {
        path: 'worklist',
        canActivate: [roleGuard('Physician')],
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
