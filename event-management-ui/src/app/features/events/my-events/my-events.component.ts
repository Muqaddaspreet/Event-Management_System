import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EventService } from '../services/event.service';
import { EventSummary } from '../models/event-summary.model';

@Component({
  selector: 'app-my-events',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './my-events.component.html',
  styleUrl: './my-events.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyEventsComponent implements OnInit {
  private eventService = inject(EventService);

  loading    = signal(true);
  error      = signal(false);
  events     = signal<EventSummary[]>([]);
  cancelling = signal<Record<number, boolean>>({});
  cancelErrors = signal<Record<number, string>>({});

  readonly totalEvents    = computed(() => this.events().length);
  readonly publishedCount = computed(() => this.events().filter(e => e.status === 'Published').length);
  readonly pendingCount   = computed(() => this.events().filter(e => e.status === 'PendingApproval').length);
  readonly cancelledCount = computed(() => this.events().filter(e => e.status === 'Cancelled').length);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.eventService.getMine({ page: 1, pageSize: 100 }).subscribe({
      next: result => {
        this.events.set(result.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  retry(): void { this.load(); }

  cancelEvent(event: EventSummary): void {
    if (!confirm(`Cancel "${event.title}"? This cannot be undone.`)) return;

    this.cancelling.update(m => ({ ...m, [event.id]: true }));
    this.cancelErrors.update(m => { const c = { ...m }; delete c[event.id]; return c; });

    this.eventService.cancel(event.id).subscribe({
      next: () => {
        this.cancelling.update(m => { const c = { ...m }; delete c[event.id]; return c; });
        this.events.update(list =>
          list.map(e => e.id === event.id ? { ...e, status: 'Cancelled' } : e)
        );
      },
      error: (err: HttpErrorResponse) => {
        this.cancelling.update(m => { const c = { ...m }; delete c[event.id]; return c; });
        const msg = err.status === 409
          ? 'This event is already cancelled.'
          : 'Failed to cancel event. Please try again.';
        this.cancelErrors.update(m => ({ ...m, [event.id]: msg }));
      }
    });
  }

  isCancelling(id: number): boolean  { return this.cancelling()[id] ?? false; }
  getCancelError(id: number): string | null { return this.cancelErrors()[id] ?? null; }

  statusClass(status: string): string {
    switch (status) {
      case 'Published':      return 'status-badge--published';
      case 'PendingApproval': return 'status-badge--pending';
      case 'Rejected':       return 'status-badge--rejected';
      case 'Cancelled':      return 'status-badge--cancelled';
      default:               return '';
    }
  }

  statusLabel(status: string): string {
    return status === 'PendingApproval' ? 'Pending Approval' : status;
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