import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const dashboardsRoutes: Routes = [
  {
    path: 'organizer',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Organizer'] },
    loadComponent: () => import('./organizer-dashboard/organizer-dashboard.component').then(m => m.OrganizerDashboardComponent)
  },
  {
    path: 'attendee',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Attendee'] },
    loadComponent: () => import('./attendee-dashboard/attendee-dashboard.component').then(m => m.AttendeeDashboardComponent)
  }
];