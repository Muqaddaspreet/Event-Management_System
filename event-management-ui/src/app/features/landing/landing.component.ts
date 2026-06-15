import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { EventService } from '../events/services/event.service';
import { CategoryService } from '../events/services/category.service';
import { EventSummary } from '../events/models/event-summary.model';
import { Category } from '../events/models/category.model';
import { EventCardComponent } from '../../shared/components/event-card/event-card.component';

const CATEGORY_ICONS: Record<string, string> = {
  'Tech': '💻', 'Technology': '💻',
  'Music': '🎵',
  'Business': '💼',
  'Sports': '🏆',
  'Art & Culture': '🎨', 'Art': '🎨',
  'Education': '📚',
  'Health': '🏥',
  'Food': '🍕',
  'Photography': '📷',
  'Conference': '🎤',
  'Workshop': '🔧',
};

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, EventCardComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingComponent implements OnInit {
  private eventService    = inject(EventService);
  private categoryService = inject(CategoryService);
  private router          = inject(Router);

  categories        = signal<Category[]>([]);
  events            = signal<EventSummary[]>([]);
  loadingCategories = signal(true);
  loadingEvents     = signal(true);
  searchQuery       = signal('');

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next:  cats => { this.categories.set(cats); this.loadingCategories.set(false); },
      error: ()   => this.loadingCategories.set(false)
    });

    this.eventService.getPublished({ page: 1, pageSize: 6 }).subscribe({
      next:  res => { this.events.set(res.items); this.loadingEvents.set(false); },
      error: ()  => this.loadingEvents.set(false)
    });
  }

  getCategoryIcon(name: string): string {
    return CATEGORY_ICONS[name] ?? '🎯';
  }

  goSearch(): void {
    const q = this.searchQuery().trim();
    if (q) {
      this.router.navigate(['/events'], { queryParams: { search: q } });
    } else {
      this.router.navigate(['/events']);
    }
  }

  goCategory(categoryId: number): void {
    this.router.navigate(['/events'], { queryParams: { categoryId } });
  }
}