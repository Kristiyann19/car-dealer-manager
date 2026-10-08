import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

import { vehicleDetail } from '../../../testing/vehicle-test-data';
import { VehicleDetailComponent } from './vehicle-detail.component';

describe('VehicleDetailComponent', () => {
  let http: HttpTestingController;
  let routeParams: BehaviorSubject<ReturnType<typeof convertToParamMap>>;

  beforeEach(async () => {
    routeParams = new BehaviorSubject(convertToParamMap({ id: '2' }));
    await TestBed.configureTestingModule({
      imports: [VehicleDetailComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: routeParams.asObservable() } },
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('transitions a direct detail route from Loading to Loaded after HTTP 200', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.pageState().status).toBe('loading');
    http.expectOne('/api/vehicles/2').flush(vehicleDetail);
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Peugeot 3008');
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain(
      'Зареждане на оценката',
    );
  });

  it('renders normally with zero cost entries and nullable optional fields', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();

    http.expectOne('/api/vehicles/2').flush(vehicleDetail);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Няма добавени активни прогнозни разходи.');
    expect(text).toContain('Не е зададена');
    expect(fixture.componentInstance.pageState().status).toBe('loaded');
  });

  it('transitions to Error and stops loading after a detail API failure', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();

    http.expectOne('/api/vehicles/2').flush({}, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('error');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Автомобилът не можа да бъде зареден.',
    );
    expect((fixture.nativeElement as HTMLElement).querySelector('.spinner')).toBeNull();
  });

  it('renders a Bulgarian not-found state for a missing vehicle', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();

    const error = new HttpErrorResponse({ status: 404, statusText: 'Not Found' });
    http.expectOne('/api/vehicles/2').error(new ProgressEvent('error'), error);
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('error');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Автомобилът не е намерен',
    );
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Няма автомобил с този идентификатор.',
    );
  });

  it('renders an archived vehicle as a read-only detail page', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();

    http.expectOne('/api/vehicles/2').flush({
      ...vehicleDetail,
      archivedAtUtc: '2026-08-15T10:00:00Z',
    });
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect(element.textContent).toContain(
      'Този автомобил е архивиран. Запазената оценка е достъпна само за преглед.',
    );
    expect(element.querySelector('button[type="submit"]')).toBeNull();
    expect(element.textContent).not.toContain('Добави разход');
  });

  it('reloads deterministically when the router reuses the component for another id', async () => {
    const fixture = TestBed.createComponent(VehicleDetailComponent);
    fixture.detectChanges();
    http.expectOne('/api/vehicles/2').flush(vehicleDetail);
    await fixture.whenStable();

    routeParams.next(convertToParamMap({ id: '3' }));
    expect(fixture.componentInstance.pageState().status).toBe('loading');
    http.expectOne('/api/vehicles/3').flush({
      ...vehicleDetail,
      id: 3,
      brand: 'Toyota',
      model: 'Corolla',
    });
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Toyota Corolla');
  });
});
