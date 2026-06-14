import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../shared/models/pagination.model';
import { EventSummary } from '../models/event-summary.model';
import { EventDetail, CreateEventRequest, UpdateEventRequest, RejectEventRequest } from '../models/event-detail.model';

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly base = `${environment.apiBaseUrl}/events`;
  private readonly adminBase = `${environment.apiBaseUrl}/admin/events`;

  constructor(private http: HttpClient) {}

  getPublished(params?: { categoryId?: number; venueId?: number; search?: string; page?: number; pageSize?: number }): Observable<PagedResult<EventSummary>> {
    let p = new HttpParams();
    if (params?.categoryId) p = p.set('categoryId', params.categoryId);
    if (params?.venueId)    p = p.set('venueId', params.venueId);
    if (params?.search)     p = p.set('search', params.search);
    if (params?.page)       p = p.set('page', params.page);
    if (params?.pageSize)   p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<EventSummary>>(this.base, { params: p });
  }

  getById(id: number): Observable<EventDetail> {
    return this.http.get<EventDetail>(`${this.base}/${id}`);
  }

  getMine(params?: { page?: number; pageSize?: number }): Observable<PagedResult<EventSummary>> {
    let p = new HttpParams();
    if (params?.page)     p = p.set('page', params.page);
    if (params?.pageSize) p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<EventSummary>>(`${this.base}/mine`, { params: p });
  }

  create(request: CreateEventRequest): Observable<EventDetail> {
    return this.http.post<EventDetail>(this.base, request);
  }

  update(id: number, request: UpdateEventRequest): Observable<EventDetail> {
    return this.http.put<EventDetail>(`${this.base}/${id}`, request);
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  adminGetAll(params?: { status?: string; organizerId?: number; page?: number; pageSize?: number }): Observable<PagedResult<EventSummary>> {
    let p = new HttpParams();
    if (params?.status)      p = p.set('status', params.status);
    if (params?.organizerId) p = p.set('organizerId', params.organizerId);
    if (params?.page)        p = p.set('page', params.page);
    if (params?.pageSize)    p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<EventSummary>>(this.adminBase, { params: p });
  }

  approve(id: number): Observable<EventDetail> {
    return this.http.post<EventDetail>(`${this.adminBase}/${id}/approve`, {});
  }

  reject(id: number, request: RejectEventRequest): Observable<EventDetail> {
    return this.http.post<EventDetail>(`${this.adminBase}/${id}/reject`, request);
  }
}