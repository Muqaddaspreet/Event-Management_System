import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
  private fb   = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role:     ['Attendee' as string, Validators.required]
  });

  loading      = signal(false);
  serverError  = signal('');
  showPassword = signal(false);

  get fullName() { return this.form.controls.fullName; }
  get email()    { return this.form.controls.email; }
  get password() { return this.form.controls.password; }
  get role()     { return this.form.controls.role; }

  selectRole(role: 'Attendee' | 'Organizer'): void {
    this.role.setValue(role);
  }

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

    this.auth.register({
      fullName: this.form.value.fullName!,
      email:    this.form.value.email!,
      password: this.form.value.password!,
      role:     this.form.value.role!
    }).subscribe({
      next: res  => this.navigateByRole(res.role),
      error: err => {
        this.loading.set(false);
        const apiError = err?.error;
        if (apiError?.errors?.length) {
          this.serverError.set(apiError.errors[0]);
        } else {
          this.serverError.set(
            apiError?.message ?? 'Registration failed. Please try again.'
          );
        }
      }
    });
  }

  private navigateByRole(role: UserRole): void {
    if (role === 'Organizer') this.router.navigate(['/my-events']);
    else                      this.router.navigate(['/events']);
  }
}