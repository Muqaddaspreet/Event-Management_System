export type UserRole = 'Admin' | 'Organizer' | 'Attendee';

export interface AuthResponse {
  token: string;
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface CurrentUser {
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
}