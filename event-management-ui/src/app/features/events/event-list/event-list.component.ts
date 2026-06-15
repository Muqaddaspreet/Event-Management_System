import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../services/event.service';
import { CategoryService } from '../services/category.service';
import { EventSummary } from '../models/event-summary.model';
import { Category } from '../models/category.model';
import { EventCardComponent } from '../../../shared/components/event-card/event-card.component';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [EventCardComponent],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventListComponent implements OnInit {
  private eventService    = inject(EventService);
  private categoryService = inject(CategoryService);
  private router          = inject(Router);
  private route           = inject(ActivatedRoute);

  events             = signal<EventSummary[]>([]);
  categories         = signal<Category[]>([]);
  loading            = signal(true);
  error              = signal(false);
  totalCount         = signal(0);
  currentPage        = signal(1);
  selectedCategoryId = signal<number | null>(null);
  pendingSearch      = signal('');
  committedSearch    = signal('');

  readonly pageSize = 12;

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize);
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const cur = this.currentPage();
    const pages: number[] = [1];
    if (cur > 3) pages.push(-1);
    for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
    if (cur < total - 2) pages.push(-1);
    if (total > 1) pages.push(total);
    return pages;
  }

  get hasActiveFilters(): boolean {
    return this.selectedCategoryId() !== null || this.committedSearch().length > 0;
  }

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const search = params.get('search') ?? '';
    const catId  = params.get('categoryId') ? Number(params.get('categoryId')) : null;

    this.pendingSearch.set(search);
    this.committedSearch.set(search);
    this.selectedCategoryId.set(catId);

    this.categoryService.getAll().subscribe({
      next:  cats => this.categories.set(cats),
      error: ()   => {}
    });

    this.loadEvents();
  }

  private loadEvents(): void {
    this.loading.set(true);
    this.error.set(false);

    this.eventService.getPublished({
      page:       this.currentPage(),
      pageSize:   this.pageSize,
      categoryId: this.selectedCategoryId() ?? undefined,
      search:     this.committedSearch() || undefined
    }).subscribe({
      next: res => {
        this.events.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  onSearchSubmit(): void {
    this.committedSearch.set(this.pendingSearch().trim());
    this.currentPage.set(1);
    this.syncUrl();
    this.loadEvents();
  }

  selectCategory(catId: number | null): void {
    this.selectedCategoryId.set(catId);
    this.currentPage.set(1);
    this.syncUrl();
    this.loadEvents();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage()) return;
    this.currentPage.set(page);
    this.loadEvents();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  clearFilters(): void {
    this.selectedCategoryId.set(null);
    this.pendingSearch.set('');
    this.committedSearch.set('');
    this.currentPage.set(1);
    this.syncUrl();
    this.loadEvents();
  }

  retry(): void {
    this.loadEvents();
  }

  private syncUrl(): void {
    const queryParams: Record<string, string | number> = {};
    const cat    = this.selectedCategoryId();
    const search = this.committedSearch();
    if (cat)    queryParams['categoryId'] = cat;
    if (search) queryParams['search']     = search;
    this.router.navigate([], { queryParams, replaceUrl: true });
  }
}