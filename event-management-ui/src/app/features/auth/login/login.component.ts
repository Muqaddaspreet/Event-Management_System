import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  private fb   = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  loading      = signal(false);
  serverError  = signal('');
  showPassword = signal(false);

  get email()    { return this.form.controls.email; }
  get password() { return this.form.controls.password; }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.serverError.set('');

    this.auth.login({
      email:    this.form.value.email!,
      password: this.form.value.password!
    }).subscribe({
      next: res  => this.navigateByRole(res.role),
      error: err => {
        this.loading.set(false);
        this.serverError.set(
          err?.error?.message ?? 'Invalid email or password. Please try again.'
        );
      }
    });
  }

  private navigateByRole(role: UserRole): void {
    if (role === 'Admin')     this.router.navigate(['/admin']);
    else if (role === 'Organizer') this.router.navigate(['/my-events']);
    else                      this.router.navigate(['/events']);
  }
}