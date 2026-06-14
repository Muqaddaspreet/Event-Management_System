import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const adminRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      {
        path: '',
        loadComponent: () => import('./admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./manage-events/manage-events.component').then(m => m.ManageEventsComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./manage-users/manage-users.component').then(m => m.ManageUsersComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./manage-categories/manage-categories.component').then(m => m.ManageCategoriesComponent)
      },
      {
        path: 'venues',
        loadComponent: () => import('./manage-venues/manage-venues.component').then(m => m.ManageVenuesComponent)
      },
      {
        path: 'registrations',
        loadComponent: () => import('./view-registrations/view-registrations.component').then(m => m.ViewRegistrationsComponent)
      }
    ]
  }
];