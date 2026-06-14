import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Venue, CreateVenueRequest, UpdateVenueRequest } from '../models/venue.model';

@Injectable({ providedIn: 'root' })
export class VenueService {
  private readonly base = `${environment.apiBaseUrl}/venues`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Venue[]> {
    return this.http.get<Venue[]>(this.base);
  }

  create(request: CreateVenueRequest): Observable<Venue> {
    return this.http.post<Venue>(this.base, request);
  }

  update(id: number, request: UpdateVenueRequest): Observable<Venue> {
    return this.http.put<Venue>(`${this.base}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}