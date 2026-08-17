import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { EstimatedCostEntryUpsertRequest } from '../models/vehicle.models';
import { VehicleApiService } from './vehicle-api.service';

describe('VehicleApiService', () => {
  let service: VehicleApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [VehicleApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(VehicleApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('passes the archived filter to the vehicle list endpoint', () => {
    service.getVehicles(true).subscribe((vehicles) => expect(vehicles).toEqual([]));

    const request = http.expectOne(
      (candidate) =>
        candidate.url === '/api/vehicles' && candidate.params.get('includeArchived') === 'true',
    );
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('updates an estimate through the existing nested endpoint', () => {
    const body: EstimatedCostEntryUpsertRequest = {
      category: 'AuctionFee',
      description: 'Auction fee',
      amount: 250,
      purchasePricePercentage: 3,
      taxPercentage: 20,
      riskPercentage: 0,
      entryDate: null,
      includedInAnalysis: true,
      notes: null,
    };

    service.updateEstimatedCost(7, 12, body).subscribe();

    const request = http.expectOne('/api/vehicles/7/costs/12');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush({});
  });
});
