export interface EventDetail {
  id: number;
  title: string;
  description: string | null;
  startTime: string;
  endTime: string;
  capacity: number;
  status: string;
  createdAt: string;
  updatedAt: string;
  organizerId: number;
  organizerName: string;
  categoryId: number;
  categoryName: string;
  venueId: number;
  venueName: string;
  venueAddress: string;
  venueCity: string;
  registrationCount: number;
}

export interface CreateEventRequest {
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  capacity: number;
  categoryId: number;
  venueId: number;
}

export interface UpdateEventRequest {
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  capacity: number;
  categoryId: number;
  venueId: number;
}

export interface RejectEventRequest {
  reason?: string;
}