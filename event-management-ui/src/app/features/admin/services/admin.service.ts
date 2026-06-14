import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../shared/models/pagination.model';
import { AdminDashboardResponse } from '../models/admin-dashboard.model';
import { UserResponse } from '../models/user-response.model';
import { AdminRegistration } from '../../registrations/models/registration.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly base = `${environment.apiBaseUrl}/admin`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<AdminDashboardResponse> {
    return this.http.get<AdminDashboardResponse>(`${this.base}/dashboard`);
  }

  getUsers(params?: { role?: string; search?: string; page?: number; pageSize?: number }): Observable<PagedResult<UserResponse>> {
    let p = new HttpParams();
    if (params?.role)     p = p.set('role', params.role);
    if (params?.search)   p = p.set('search', params.search);
    if (params?.page)     p = p.set('page', params.page);
    if (params?.pageSize) p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<UserResponse>>(`${this.base}/users`, { params: p });
  }

  getUserById(id: number): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.base}/users/${id}`);
  }

  getRegistrations(params?: { eventId?: number; userId?: number; status?: string; page?: number; pageSize?: number }): Observable<PagedResult<AdminRegistration>> {
    let p = new HttpParams();
    if (params?.eventId)  p = p.set('eventId', params.eventId);
    if (params?.userId)   p = p.set('userId', params.userId);
    if (params?.status)   p = p.set('status', params.status);
    if (params?.page)     p = p.set('page', params.page);
    if (params?.pageSize) p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<AdminRegistration>>(`${this.base}/registrations`, { params: p });
  }
}