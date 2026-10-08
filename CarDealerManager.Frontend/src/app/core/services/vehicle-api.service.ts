import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  EstimatedCostEntryUpsertRequest,
  VehicleCostEntry,
  VehicleDetail,
  VehicleListItem,
  VehicleUpsertRequest,
} from '../models/vehicle.models';

@Injectable({ providedIn: 'root' })
export class VehicleApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/vehicles';

  getVehicles(includeArchived = false): Observable<VehicleListItem[]> {
    return this.http.get<VehicleListItem[]>(this.baseUrl, {
      params: { includeArchived },
    });
  }

  getVehicle(id: number): Observable<VehicleDetail> {
    return this.http.get<VehicleDetail>(`${this.baseUrl}/${id}`);
  }

  createVehicle(request: VehicleUpsertRequest): Observable<VehicleDetail> {
    return this.http.post<VehicleDetail>(this.baseUrl, request);
  }

  updateVehicle(id: number, request: VehicleUpsertRequest): Observable<VehicleDetail> {
    return this.http.put<VehicleDetail>(`${this.baseUrl}/${id}`, request);
  }

  addEstimatedCost(
    vehicleId: number,
    request: EstimatedCostEntryUpsertRequest,
  ): Observable<VehicleCostEntry> {
    return this.http.post<VehicleCostEntry>(`${this.baseUrl}/${vehicleId}/costs`, request);
  }

  updateEstimatedCost(
    vehicleId: number,
    costEntryId: number,
    request: EstimatedCostEntryUpsertRequest,
  ): Observable<VehicleCostEntry> {
    return this.http.put<VehicleCostEntry>(
      `${this.baseUrl}/${vehicleId}/costs/${costEntryId}`,
      request,
    );
  }

  archiveEstimatedCost(vehicleId: number, costEntryId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${vehicleId}/costs/${costEntryId}`);
  }
}
