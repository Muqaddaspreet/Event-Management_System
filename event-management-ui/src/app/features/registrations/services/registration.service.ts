import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Registration, CreateRegistrationRequest } from '../models/registration.model';

@Injectable({ providedIn: 'root' })
export class RegistrationService {
  private readonly base = `${environment.apiBaseUrl}/registrations`;

  constructor(private http: HttpClient) {}

  getMine(): Observable<Registration[]> {
    return this.http.get<Registration[]>(`${this.base}/mine`);
  }

  register(request: CreateRegistrationRequest): Observable<Registration> {
    return this.http.post<Registration>(this.base, request);
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}