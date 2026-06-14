import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'events',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'events',
    loadChildren: () => import('./features/events/events.routes').then(m => m.eventsRoutes)
  },
  {
    path: 'my-events',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Organizer'] },
    loadComponent: () => import('./features/events/my-events/my-events.component').then(m => m.MyEventsComponent)
  },
  {
    path: 'my-registrations',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Attendee'] },
    loadComponent: () => import('./features/registrations/my-registrations/my-registrations.component').then(m => m.MyRegistrationsComponent)
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboards/dashboards.routes').then(m => m.dashboardsRoutes)
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.adminRoutes)
  },
  {
    path: '**',
    loadComponent: () => import('./features/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];