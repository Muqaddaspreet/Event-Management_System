export interface Registration {
  id: number;
  eventId: number;
  eventTitle: string;
  eventStartTime: string;
  userId: number;
  userFullName: string;
  status: string;
  registeredAt: string;
  updatedAt: string;
}

export interface CreateRegistrationRequest {
  eventId: number;
}

export interface AdminRegistration {
  id: number;
  eventId: number;
  eventTitle: string;
  userId: number;
  userFullName: string;
  userEmail: string;
  status: string;
  registeredAt: string;
  updatedAt: string;
}