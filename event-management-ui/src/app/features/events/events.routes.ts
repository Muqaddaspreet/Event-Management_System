import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const eventsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./event-list/event-list.component').then(m => m.EventListComponent)
  },
  {
    path: 'mine',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Organizer'] },
    loadComponent: () => import('./my-events/my-events.component').then(m => m.MyEventsComponent)
  },
  {
    path: 'create',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Organizer'] },
    loadComponent: () => import('./event-form/event-form.component').then(m => m.EventFormComponent)
  },
  {
    path: ':id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Organizer', 'Admin'] },
    loadComponent: () => import('./event-form/event-form.component').then(m => m.EventFormComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./event-detail/event-detail.component').then(m => m.EventDetailComponent)
  }
];