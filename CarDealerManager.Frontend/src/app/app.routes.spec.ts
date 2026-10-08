import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { of, throwError } from 'rxjs';

import { routes } from './app.routes';
import { VehicleApiService } from './core/services/vehicle-api.service';
import { CreateVehicleComponent } from './features/vehicles/create-vehicle/create-vehicle.component';
import { vehicleDetail, vehicleListItem, vehicleRequest } from './testing/vehicle-test-data';

describe('application routes', () => {
  const api = {
    getVehicles: vi.fn(),
    getVehicle: vi.fn(),
    createVehicle: vi.fn(),
    updateVehicle: vi.fn(),
    addEstimatedCost: vi.fn(),
    updateEstimatedCost: vi.fn(),
    archiveEstimatedCost: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    api.getVehicles.mockReturnValue(of([vehicleListItem]));
    api.getVehicle.mockReturnValue(of(vehicleDetail));
    api.createVehicle.mockReturnValue(of(vehicleDetail));

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideRouter(routes),
        { provide: VehicleApiService, useValue: api },
      ],
    });
  });

  it('supports direct initialization of /vehicles', async () => {
    const harness = await RouterTestingHarness.create('/vehicles');

    expect(harness.routeNativeElement?.textContent).toContain('Peugeot 3008');
    expect(api.getVehicles).toHaveBeenCalledExactlyOnceWith(false);
    expect(api.getVehicle).not.toHaveBeenCalled();
  });

  it('supports direct initialization of /vehicles/new without loading another page', async () => {
    const harness = await RouterTestingHarness.create('/vehicles/new');

    expect(harness.routeNativeElement?.textContent).toContain('Добави автомобил');
    expect(api.getVehicles).not.toHaveBeenCalled();
    expect(api.getVehicle).not.toHaveBeenCalled();
  });

  it('supports direct initialization of /vehicles/2', async () => {
    const harness = await RouterTestingHarness.create('/vehicles/2');

    expect(harness.routeNativeElement?.textContent).toContain('Peugeot 3008');
    expect(api.getVehicle).toHaveBeenCalledExactlyOnceWith(2);
  });

  it('shows the not-found state when a direct detail URL returns 404', async () => {
    api.getVehicle.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404, statusText: 'Not Found' })),
    );

    const harness = await RouterTestingHarness.create('/vehicles/999');

    expect(harness.routeNativeElement?.textContent).toContain('Автомобилът не е намерен');
  });

  it('navigates to the returned detail route after creating a candidate', async () => {
    const harness = await RouterTestingHarness.create('/vehicles/new');
    const component = harness.routeDebugElement?.componentInstance as CreateVehicleComponent;

    component.create(vehicleRequest);
    await harness.fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/vehicles/2');
    expect(api.createVehicle).toHaveBeenCalledExactlyOnceWith(vehicleRequest);
    expect(api.getVehicle).toHaveBeenCalledExactlyOnceWith(2);
  });
});
