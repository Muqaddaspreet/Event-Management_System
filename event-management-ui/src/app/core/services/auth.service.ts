import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, UserRole } from '../models/user.model';

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

const TOKEN_KEY = 'em_token';
const USER_KEY  = 'em_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _currentUser = signal<CurrentUser | null>(this.loadUser());

  readonly currentUser  = this._currentUser.asReadonly();
  readonly isLoggedIn   = computed(() => this._currentUser() !== null);
  readonly currentRole  = computed(() => this._currentUser()?.role ?? null);

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(tap(res => this.saveSession(res)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/register`, request)
      .pipe(tap(res => this.saveSession(res)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._currentUser.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(role: UserRole): boolean {
    return this._currentUser()?.role === role;
  }

  private saveSession(res: AuthResponse): void {
    const user: CurrentUser = {
      userId:   res.userId,
      fullName: res.fullName,
      email:    res.email,
      role:     res.role
    };
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._currentUser.set(user);
  }

  private loadUser(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as CurrentUser) : null;
    } catch {
      return null;
    }
  }
}