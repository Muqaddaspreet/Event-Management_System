import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { AdminService } from '../services/admin.service';
import { EventService } from '../../events/services/event.service';
import { AdminDashboardResponse } from '../models/admin-dashboard.model';
import { EventSummary } from '../../events/models/event-summary.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  private eventService = inject(EventService);

  loading       = signal(true);
  error         = signal(false);
  stats         = signal<AdminDashboardResponse | null>(null);
  pendingEvents = signal<EventSummary[]>([]);

  approving    = signal<Record<number, boolean>>({});
  rejecting    = signal<Record<number, boolean>>({});
  actionErrors = signal<Record<number, string>>({});

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.error.set(false);

    forkJoin({
      dashboard: this.adminService.getDashboard(),
      pending:   this.eventService.adminGetAll({ status: 'PendingApproval', pageSize: 5 })
    }).subscribe({
      next: ({ dashboard, pending }) => {
        this.stats.set(dashboard);
        this.pendingEvents.set(pending.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  retry(): void { this.load(); }

  isApproving(id: number): boolean { return !!this.approving()[id]; }
  isRejecting(id: number): boolean { return !!this.rejecting()[id]; }
  getError(id: number): string     { return this.actionErrors()[id] ?? ''; }

  approveEvent(event: EventSummary): void {
    if (!confirm(`Approve "${event.title}" and publish it?`)) return;
    this.approving.update(s => ({ ...s, [event.id]: true }));
    this.actionErrors.update(s => { const n = { ...s }; delete n[event.id]; return n; });

    this.eventService.approve(event.id).subscribe({
      next: () => {
        this.approving.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.pendingEvents.update(list => list.filter(e => e.id !== event.id));
        const s = this.stats();
        if (s) this.stats.set({ ...s, pendingEvents: s.pendingEvents - 1, publishedEvents: s.publishedEvents + 1 });
      },
      error: (err: HttpErrorResponse) => {
        this.approving.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.actionErrors.update(s => ({ ...s, [event.id]: err.error?.message ?? 'Failed to approve.' }));
      }
    });
  }

  rejectEvent(event: EventSummary): void {
    const reason = prompt(`Rejection reason for "${event.title}" (optional):`);
    if (reason === null) return;
    this.rejecting.update(s => ({ ...s, [event.id]: true }));
    this.actionErrors.update(s => { const n = { ...s }; delete n[event.id]; return n; });

    this.eventService.reject(event.id, { reason: reason || undefined }).subscribe({
      next: () => {
        this.rejecting.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.pendingEvents.update(list => list.filter(e => e.id !== event.id));
        const s = this.stats();
        if (s) this.stats.set({ ...s, pendingEvents: s.pendingEvents - 1 });
      },
      error: (err: HttpErrorResponse) => {
        this.rejecting.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.actionErrors.update(s => ({ ...s, [event.id]: err.error?.message ?? 'Failed to reject.' }));
      }
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}