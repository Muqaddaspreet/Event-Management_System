export interface Venue {
  id: number;
  name: string;
  address: string;
  city: string;
  capacity: number;
}

export interface CreateVenueRequest {
  name: string;
  address: string;
  city: string;
  capacity: number;
}

export interface UpdateVenueRequest {
  name: string;
  address: string;
  city: string;
  capacity: number;
}