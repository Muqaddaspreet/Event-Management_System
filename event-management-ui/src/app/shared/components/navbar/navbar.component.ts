import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  private auth   = inject(AuthService);
  private router = inject(Router);

  readonly isLoggedIn  = this.auth.isLoggedIn;
  readonly currentUser = this.auth.currentUser;
  readonly currentRole = this.auth.currentRole;

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/events']);
  }
}