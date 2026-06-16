import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { CategoryService } from '../../events/services/category.service';
import { Category } from '../../events/models/category.model';

@Component({
  selector: 'app-manage-categories',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './manage-categories.component.html',
  styleUrl: './manage-categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageCategoriesComponent implements OnInit {
  private categoryService = inject(CategoryService);

  loading    = signal(true);
  error      = signal(false);
  categories = signal<Category[]>([]);

  formOpen   = signal(false);
  editingId  = signal<number | null>(null);
  formName   = signal('');
  formError  = signal('');
  saving     = signal(false);

  deletingIds  = signal<Record<number, boolean>>({});
  deleteErrors = signal<Record<number, string>>({});

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.error.set(false);

    this.categoryService.getAll().subscribe({
      next: categories => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  retry(): void { this.load(); }

  openCreateForm(): void {
    this.editingId.set(null);
    this.formName.set('');
    this.formError.set('');
    this.formOpen.set(true);
  }

  openEditForm(category: Category): void {
    this.editingId.set(category.id);
    this.formName.set(category.name);
    this.formError.set('');
    this.formOpen.set(true);
  }

  closeForm(): void {
    this.formOpen.set(false);
    this.editingId.set(null);
    this.formName.set('');
    this.formError.set('');
    this.saving.set(false);
  }

  onNameChange(event: Event): void {
    this.formName.set((event.target as HTMLInputElement).value);
  }

  submitForm(): void {
    const name = this.formName().trim();
    if (!name) {
      this.formError.set('Name is required.');
      return;
    }

    this.saving.set(true);
    this.formError.set('');
    const id = this.editingId();

    const request$ = id === null
      ? this.categoryService.create({ name })
      : this.categoryService.update(id, { name });

    request$.subscribe({
      next: result => {
        if (id === null) {
          this.categories.update(list => [...list, result]);
        } else {
          this.categories.update(list => list.map(c => c.id === id ? result : c));
        }
        this.closeForm();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        if (err.status === 409) {
          this.formError.set('A category with this name already exists.');
        } else {
          this.formError.set(err.error?.message ?? 'Failed to save category. Please try again.');
        }
      }
    });
  }

  isDeleting(id: number): boolean { return !!this.deletingIds()[id]; }
  getDeleteError(id: number): string { return this.deleteErrors()[id] ?? ''; }

  deleteCategory(category: Category): void {
    if (!confirm('Delete this category?')) return;

    this.deletingIds.update(s => ({ ...s, [category.id]: true }));
    this.deleteErrors.update(s => { const n = { ...s }; delete n[category.id]; return n; });

    this.categoryService.delete(category.id).subscribe({
      next: () => {
        this.deletingIds.update(s => { const n = { ...s }; delete n[category.id]; return n; });
        this.categories.update(list => list.filter(c => c.id !== category.id));
      },
      error: (err: HttpErrorResponse) => {
        this.deletingIds.update(s => { const n = { ...s }; delete n[category.id]; return n; });
        const message = err.status === 409
          ? 'This category cannot be deleted because it is used by existing events.'
          : 'Failed to delete category. Please try again.';
        this.deleteErrors.update(s => ({ ...s, [category.id]: message }));
      }
    });
  }
}