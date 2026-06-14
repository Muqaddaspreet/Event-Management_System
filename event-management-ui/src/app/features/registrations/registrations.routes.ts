import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const registrationsRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Attendee'] },
    loadComponent: () => import('./my-registrations/my-registrations.component').then(m => m.MyRegistrationsComponent)
  }
];