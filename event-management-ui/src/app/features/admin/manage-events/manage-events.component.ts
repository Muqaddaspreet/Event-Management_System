import { ChangeDetectionStrategy, Component, inject, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { EventService } from '../../events/services/event.service';
import { EventSummary } from '../../events/models/event-summary.model';

type StatusFilter = 'All' | 'PendingApproval' | 'Published' | 'Rejected' | 'Cancelled';

@Component({
  selector: 'app-manage-events',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './manage-events.component.html',
  styleUrl: './manage-events.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageEventsComponent implements OnInit {
  private eventService = inject(EventService);

  loading    = signal(true);
  error      = signal(false);
  events     = signal<EventSummary[]>([]);
  totalCount = signal(0);
  activeTab  = signal<StatusFilter>('All');
  searchQuery = signal('');
  currentPage = signal(1);
  readonly pageSize = 20;

  approving     = signal<Record<number, boolean>>({});
  approveErrors = signal<Record<number, string>>({});

  rejectingId   = signal<number | null>(null);
  rejectReason  = signal('');
  rejectLoading = signal(false);
  rejectError   = signal('');

  readonly tabs: StatusFilter[] = ['All', 'PendingApproval', 'Published', 'Rejected', 'Cancelled'];
  readonly tabLabels: Record<StatusFilter, string> = {
    'All':             'All',
    'PendingApproval': 'Pending Approval',
    'Published':       'Published',
    'Rejected':        'Rejected',
    'Cancelled':       'Cancelled'
  };

  filteredEvents = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.events();
    return this.events().filter(e =>
      e.title.toLowerCase().includes(q) ||
      e.organizerName.toLowerCase().includes(q) ||
      e.categoryName.toLowerCase().includes(q)
    );
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));
  pages      = computed(() => {
    const total = this.totalPages();
    if (total <= 1) return [];
    return Array.from({ length: total }, (_, i) => i + 1);
  });

  ngOnInit(): void { this.loadEvents(); }

  private loadEvents(): void {
    this.loading.set(true);
    this.error.set(false);
    const status = this.activeTab() === 'All' ? undefined : this.activeTab();

    this.eventService.adminGetAll({ status, page: this.currentPage(), pageSize: this.pageSize }).subscribe({
      next: result => {
        this.events.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  retry(): void { this.loadEvents(); }

  setTab(tab: StatusFilter): void {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.currentPage.set(1);
    this.searchQuery.set('');
    this.closeRejectPanel();
    this.loadEvents();
  }

  onSearch(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
  }

  clearSearch(): void {
    this.searchQuery.set('');
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    this.currentPage.set(page);
    this.closeRejectPanel();
    this.loadEvents();
  }

  isApproving(id: number): boolean  { return !!this.approving()[id]; }
  getApproveError(id: number): string { return this.approveErrors()[id] ?? ''; }

  approveEvent(event: EventSummary): void {
    if (!confirm(`Approve "${event.title}" and publish it?`)) return;
    this.approving.update(s => ({ ...s, [event.id]: true }));
    this.approveErrors.update(s => { const n = { ...s }; delete n[event.id]; return n; });

    this.eventService.approve(event.id).subscribe({
      next: () => {
        this.approving.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.replaceOrRemoveAfterStatusChange(event.id, 'Published');
      },
      error: (err: HttpErrorResponse) => {
        this.approving.update(s => { const n = { ...s }; delete n[event.id]; return n; });
        this.approveErrors.update(s => ({ ...s, [event.id]: err.error?.message ?? 'Failed to approve.' }));
      }
    });
  }

  openRejectPanel(id: number): void {
    this.rejectingId.set(id);
    this.rejectReason.set('');
    this.rejectError.set('');
    this.rejectLoading.set(false);
  }

  closeRejectPanel(): void {
    this.rejectingId.set(null);
    this.rejectReason.set('');
    this.rejectError.set('');
    this.rejectLoading.set(false);
  }

  onRejectReasonChange(event: Event): void {
    this.rejectReason.set((event.target as HTMLTextAreaElement).value);
  }

  submitReject(id: number): void {
    this.rejectLoading.set(true);
    this.rejectError.set('');

    this.eventService.reject(id, { reason: this.rejectReason() || undefined }).subscribe({
      next: () => {
        this.replaceOrRemoveAfterStatusChange(id, 'Rejected');
        this.closeRejectPanel();
      },
      error: (err: HttpErrorResponse) => {
        this.rejectLoading.set(false);
        this.rejectError.set(err.error?.message ?? 'Failed to reject. Please try again.');
      }
    });
  }

  private replaceOrRemoveAfterStatusChange(id: number, status: 'Published' | 'Rejected'): void {
    if (this.activeTab() === 'All') {
      this.events.update(list => list.map(e => e.id === id ? { ...e, status } : e));
    } else {
      this.events.update(list => list.filter(e => e.id !== id));
      this.totalCount.update(count => Math.max(0, count - 1));
    }
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Published':       return 'status-badge--published';
      case 'PendingApproval': return 'status-badge--pending';
      case 'Rejected':        return 'status-badge--rejected';
      case 'Cancelled':       return 'status-badge--cancelled';
      default:                return 'status-badge--cancelled';
    }
  }

  statusLabel(status: string): string {
    return status === 'PendingApproval' ? 'Pending Approval' : status;
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}