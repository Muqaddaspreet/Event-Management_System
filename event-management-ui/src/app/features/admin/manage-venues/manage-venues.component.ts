import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { VenueService } from '../../events/services/venue.service';
import { Venue } from '../../events/models/venue.model';

@Component({
  selector: 'app-manage-venues',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './manage-venues.component.html',
  styleUrl: './manage-venues.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageVenuesComponent implements OnInit {
  private venueService = inject(VenueService);

  loading = signal(true);
  error   = signal(false);
  venues  = signal<Venue[]>([]);

  formOpen      = signal(false);
  editingId     = signal<number | null>(null);
  formName      = signal('');
  formAddress   = signal('');
  formCity      = signal('');
  formCapacity  = signal('');
  formError     = signal('');
  saving        = signal(false);

  deletingIds  = signal<Record<number, boolean>>({});
  deleteErrors = signal<Record<number, string>>({});

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.loading.set(true);
    this.error.set(false);

    this.venueService.getAll().subscribe({
      next: venues => {
        this.venues.set(venues);
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
    this.formAddress.set('');
    this.formCity.set('');
    this.formCapacity.set('');
    this.formError.set('');
    this.formOpen.set(true);
  }

  openEditForm(venue: Venue): void {
    this.editingId.set(venue.id);
    this.formName.set(venue.name);
    this.formAddress.set(venue.address);
    this.formCity.set(venue.city);
    this.formCapacity.set(String(venue.capacity));
    this.formError.set('');
    this.formOpen.set(true);
  }

  closeForm(): void {
    this.formOpen.set(false);
    this.editingId.set(null);
    this.formName.set('');
    this.formAddress.set('');
    this.formCity.set('');
    this.formCapacity.set('');
    this.formError.set('');
    this.saving.set(false);
  }

  onNameChange(event: Event): void { this.formName.set((event.target as HTMLInputElement).value); }
  onAddressChange(event: Event): void { this.formAddress.set((event.target as HTMLInputElement).value); }
  onCityChange(event: Event): void { this.formCity.set((event.target as HTMLInputElement).value); }
  onCapacityChange(event: Event): void { this.formCapacity.set((event.target as HTMLInputElement).value); }

  submitForm(): void {
    const name = this.formName().trim();
    const address = this.formAddress().trim();
    const city = this.formCity().trim();
    const capacity = Number(this.formCapacity());

    if (!name || !address || !city) {
      this.formError.set('Name, address, and city are required.');
      return;
    }
    if (!Number.isFinite(capacity) || capacity <= 0) {
      this.formError.set('Capacity must be a number greater than 0.');
      return;
    }

    this.saving.set(true);
    this.formError.set('');
    const id = this.editingId();
    const payload = { name, address, city, capacity };

    const request$ = id === null
      ? this.venueService.create(payload)
      : this.venueService.update(id, payload);

    request$.subscribe({
      next: result => {
        if (id === null) {
          this.venues.update(list => [...list, result]);
        } else {
          this.venues.update(list => list.map(v => v.id === id ? result : v));
        }
        this.closeForm();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.formError.set(err.error?.message ?? 'Failed to save venue. Please try again.');
      }
    });
  }

  isDeleting(id: number): boolean { return !!this.deletingIds()[id]; }
  getDeleteError(id: number): string { return this.deleteErrors()[id] ?? ''; }

  deleteVenue(venue: Venue): void {
    if (!confirm('Delete this venue?')) return;

    this.deletingIds.update(s => ({ ...s, [venue.id]: true }));
    this.deleteErrors.update(s => { const n = { ...s }; delete n[venue.id]; return n; });

    this.venueService.delete(venue.id).subscribe({
      next: () => {
        this.deletingIds.update(s => { const n = { ...s }; delete n[venue.id]; return n; });
        this.venues.update(list => list.filter(v => v.id !== venue.id));
      },
      error: (err: HttpErrorResponse) => {
        this.deletingIds.update(s => { const n = { ...s }; delete n[venue.id]; return n; });
        const message = err.status === 409
          ? 'This venue cannot be deleted because it is used by existing events.'
          : 'Failed to delete venue. Please try again.';
        this.deleteErrors.update(s => ({ ...s, [venue.id]: message }));
      }
    });
  }
}