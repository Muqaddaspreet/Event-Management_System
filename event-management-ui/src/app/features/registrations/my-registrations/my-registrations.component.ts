import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { RegistrationService } from '../services/registration.service';
import { Registration } from '../models/registration.model';

type FilterTab = 'All' | 'Registered' | 'Cancelled';

@Component({
  selector: 'app-my-registrations',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './my-registrations.component.html',
  styleUrl: './my-registrations.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyRegistrationsComponent implements OnInit {
  private regService = inject(RegistrationService);

  loading       = signal(true);
  error         = signal(false);
  registrations = signal<Registration[]>([]);
  activeTab     = signal<FilterTab>('All');
  cancelling    = signal<Record<number, boolean>>({});
  cancelErrors  = signal<Record<number, string>>({});

  readonly tabs: FilterTab[] = ['All', 'Registered', 'Cancelled'];

  readonly filtered = computed(() => {
    const tab = this.activeTab();
    const all = this.registrations();
    return tab === 'All' ? all : all.filter(r => r.status === tab);
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.regService.getMine().subscribe({
      next: regs => {
        this.registrations.set(regs);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  retry(): void { this.load(); }

  setTab(tab: FilterTab): void { this.activeTab.set(tab); }

  tabCount(tab: FilterTab): number {
    const all = this.registrations();
    return tab === 'All' ? all.length : all.filter(r => r.status === tab).length;
  }

  cancelRegistration(reg: Registration): void {
    if (!confirm(`Cancel your registration for "${reg.eventTitle}"?`)) return;

    this.cancelling.update(m => ({ ...m, [reg.id]: true }));
    this.cancelErrors.update(m => {
      const copy = { ...m };
      delete copy[reg.id];
      return copy;
    });

    this.regService.cancel(reg.id).subscribe({
      next: () => {
        this.cancelling.update(m => {
          const copy = { ...m };
          delete copy[reg.id];
          return copy;
        });
        this.registrations.update(list =>
          list.map(r => r.id === reg.id ? { ...r, status: 'Cancelled' } : r)
        );
      },
      error: (err: HttpErrorResponse) => {
        this.cancelling.update(m => {
          const copy = { ...m };
          delete copy[reg.id];
          return copy;
        });
        const msg = err.status === 409
          ? 'This registration is already cancelled.'
          : 'Failed to cancel. Please try again.';
        this.cancelErrors.update(m => ({ ...m, [reg.id]: msg }));
      }
    });
  }

  isCancelling(id: number): boolean {
    return this.cancelling()[id] ?? false;
  }

  getCancelError(id: number): string | null {
    return this.cancelErrors()[id] ?? null;
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-US', {
      hour: 'numeric', minute: '2-digit'
    });
  }
}