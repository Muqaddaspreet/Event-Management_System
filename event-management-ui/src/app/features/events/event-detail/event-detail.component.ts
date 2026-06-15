import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EventService } from '../services/event.service';
import { AuthService } from '../../../core/services/auth.service';
import { RegistrationService } from '../../registrations/services/registration.service';
import { EventDetail } from '../models/event-detail.model';

const GRADIENTS: Record<string, string> = {
  'Tech':          'linear-gradient(135deg,#667eea,#764ba2)',
  'Technology':    'linear-gradient(135deg,#667eea,#764ba2)',
  'Music':         'linear-gradient(135deg,#f093fb,#f5576c)',
  'Business':      'linear-gradient(135deg,#4facfe,#00f2fe)',
  'Sports':        'linear-gradient(135deg,#43e97b,#38f9d7)',
  'Art & Culture': 'linear-gradient(135deg,#fa709a,#fee140)',
  'Art':           'linear-gradient(135deg,#fa709a,#fee140)',
  'Education':     'linear-gradient(135deg,#a18cd1,#fbc2eb)',
  'Health':        'linear-gradient(135deg,#84fab0,#8fd3f4)',
  'Food':          'linear-gradient(135deg,#f6d365,#fda085)',
  'Photography':   'linear-gradient(135deg,#e0c3fc,#8ec5fc)',
  'Conference':    'linear-gradient(135deg,#f7971e,#ffd200)',
  'Workshop':      'linear-gradient(135deg,#0ba360,#3cba92)',
};

function categoryGradient(name: string): string {
  return GRADIENTS[name] ?? 'linear-gradient(135deg,#7c3aed,#4f46e5)';
}

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './event-detail.component.html',
  styleUrl: './event-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventDetailComponent implements OnInit {
  private route               = inject(ActivatedRoute);
  private eventService        = inject(EventService);
  private auth                = inject(AuthService);
  private registrationService = inject(RegistrationService);
  private router              = inject(Router);

  event    = signal<EventDetail | null>(null);
  loading  = signal(true);
  notFound = signal(false);
  error    = signal(false);

  registering     = signal(false);
  registerSuccess = signal(false);
  registerError   = signal<string | null>(null);

  readonly isLoggedIn  = this.auth.isLoggedIn;
  readonly currentRole = this.auth.currentRole;

  private eventId = 0;

  get gradient(): string {
    const ev = this.event();
    return ev ? categoryGradient(ev.categoryName) : 'linear-gradient(135deg,#7c3aed,#4f46e5)';
  }

  get availableSeats(): number {
    const ev = this.event();
    return ev ? Math.max(0, ev.capacity - ev.registrationCount) : 0;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.eventId  = idParam ? Number(idParam) : NaN;

    if (isNaN(this.eventId)) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }
    this.fetchEvent();
  }

  retry(): void { this.fetchEvent(); }

  private fetchEvent(): void {
    this.loading.set(true);
    this.error.set(false);
    this.notFound.set(false);

    this.eventService.getById(this.eventId).subscribe({
      next: ev => { this.event.set(ev); this.loading.set(false); },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(true);
        }
      }
    });
  }

  registerForEvent(): void {
    if (this.registering()) return;
    this.registering.set(true);
    this.registerError.set(null);

    this.registrationService.register({ eventId: this.eventId }).subscribe({
      next: () => {
        this.registering.set(false);
        this.registerSuccess.set(true);
        setTimeout(() => this.router.navigate(['/my-registrations']), 2000);
      },
      error: (err: HttpErrorResponse) => {
        this.registering.set(false);
        if (err.status === 409) {
          this.registerError.set('You are already registered for this event.');
        } else if (err.status === 422) {
          this.registerError.set(
            err.error?.message ?? 'This event is not available for registration.'
          );
        } else {
          this.registerError.set('Registration failed. Please try again.');
        }
      }
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', {
      weekday: 'long', month: 'long', day: 'numeric', year: 'numeric'
    });
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-US', {
      hour: 'numeric', minute: '2-digit'
    });
  }

  formatDateTime(iso: string): string {
    return `${this.formatDate(iso)} at ${this.formatTime(iso)}`;
  }
}