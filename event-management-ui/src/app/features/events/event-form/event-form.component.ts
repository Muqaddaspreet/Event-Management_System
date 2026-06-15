import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { EventService } from '../services/event.service';
import { CategoryService } from '../services/category.service';
import { VenueService } from '../services/venue.service';
import { Category } from '../models/category.model';
import { Venue } from '../models/venue.model';

function endAfterStart(control: AbstractControl): ValidationErrors | null {
  const start = control.parent?.get('startTime')?.value;
  const end   = control.value;
  if (start && end && new Date(end) <= new Date(start)) {
    return { endBeforeStart: true };
  }
  return null;
}

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './event-form.component.html',
  styleUrl: './event-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventFormComponent implements OnInit {
  private fb           = inject(FormBuilder);
  private route        = inject(ActivatedRoute);
  private router       = inject(Router);
  private eventService = inject(EventService);
  private catService   = inject(CategoryService);
  private venueService = inject(VenueService);

  isEditMode = false;
  eventId    = 0;

  loadingData   = signal(true);
  loadError     = signal(false);
  submitting    = signal(false);
  submitError   = signal<string | null>(null);
  submitSuccess = signal(false);

  categories = signal<Category[]>([]);
  venues     = signal<Venue[]>([]);

  form = this.fb.group({
    title:       ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    startTime:   ['', Validators.required],
    endTime:     ['', [Validators.required, endAfterStart]],
    capacity:    [null as number | null, [Validators.required, Validators.min(1)]],
    categoryId:  [null as number | null, Validators.required],
    venueId:     [null as number | null, Validators.required]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.isEditMode = id !== null;
    this.eventId    = id !== null ? +id : 0;
    this.loadDropdowns();
  }

  private loadDropdowns(): void {
    forkJoin({
      categories: this.catService.getAll(),
      venues:     this.venueService.getAll()
    }).subscribe({
      next: ({ categories, venues }) => {
        this.categories.set(categories);
        this.venues.set(venues);
        if (this.isEditMode) {
          this.loadEvent();
        } else {
          this.loadingData.set(false);
        }
      },
      error: () => {
        this.loadError.set(true);
        this.loadingData.set(false);
      }
    });
  }

  private loadEvent(): void {
    this.eventService.getById(this.eventId).subscribe({
      next: event => {
        this.form.patchValue({
          title:       event.title,
          description: event.description ?? '',
          startTime:   this.toDateTimeLocal(event.startTime),
          endTime:     this.toDateTimeLocal(event.endTime),
          capacity:    event.capacity,
          categoryId:  event.categoryId,
          venueId:     event.venueId
        });
        this.loadingData.set(false);
      },
      error: () => {
        this.loadError.set(true);
        this.loadingData.set(false);
      }
    });
  }

  submit(): void {
    this.form.get('endTime')?.updateValueAndValidity();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    const payload = {
      title:       this.form.value.title!,
      description: this.form.value.description ?? '',
      startTime:   new Date(this.form.value.startTime!).toISOString(),
      endTime:     new Date(this.form.value.endTime!).toISOString(),
      capacity:    this.form.value.capacity!,
      categoryId:  this.form.value.categoryId!,
      venueId:     this.form.value.venueId!
    };

    const req$ = this.isEditMode
      ? this.eventService.update(this.eventId, payload)
      : this.eventService.create(payload);

    req$.subscribe({
      next: () => {
        this.submitting.set(false);
        this.submitSuccess.set(true);
        setTimeout(() => this.router.navigate(['/my-events']), 2000);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting.set(false);
        this.submitError.set(
          err.error?.message ?? 'Submission failed. Please check your inputs and try again.'
        );
      }
    });
  }

  retryLoad(): void {
    this.loadError.set(false);
    this.loadingData.set(true);
    this.loadDropdowns();
  }

  get title()      { return this.form.controls.title; }
  get description(){ return this.form.controls.description; }
  get startTime()  { return this.form.controls.startTime; }
  get endTime()    { return this.form.controls.endTime; }
  get capacity()   { return this.form.controls.capacity; }
  get categoryId() { return this.form.controls.categoryId; }
  get venueId()    { return this.form.controls.venueId; }

  private toDateTimeLocal(iso: string): string {
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}