import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/user.model';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth         = inject(AuthService);
  const router       = inject(Router);
  const allowedRoles = route.data['roles'] as UserRole[];

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  const userRole = auth.currentRole();
  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  return router.createUrlTree(['/events']);
};