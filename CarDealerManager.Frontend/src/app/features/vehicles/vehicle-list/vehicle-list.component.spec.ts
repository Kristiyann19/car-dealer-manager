import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { vehicleListItem } from '../../../testing/vehicle-test-data';
import { VehicleListComponent } from './vehicle-list.component';

describe('VehicleListComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleListComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('transitions a hard initialization from Loading to Loaded after a successful response', async () => {
    const fixture = TestBed.createComponent(VehicleListComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.pageState().status).toBe('loading');
    http.expectOne('/api/vehicles?includeArchived=false').flush([vehicleListItem]);
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Peugeot 3008');
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain(
      'Зареждане на автомобилите',
    );
  });

  it('renders a successfully loaded empty list instead of leaving the loading state active', async () => {
    const fixture = TestBed.createComponent(VehicleListComponent);
    fixture.detectChanges();

    http.expectOne('/api/vehicles?includeArchived=false').flush([]);
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Няма добавени автомобили.',
    );
    expect((fixture.nativeElement as HTMLElement).querySelector('.spinner')).toBeNull();
  });

  it('transitions to Error and stops loading after a list API failure', async () => {
    const fixture = TestBed.createComponent(VehicleListComponent);
    fixture.detectChanges();

    http
      .expectOne('/api/vehicles?includeArchived=false')
      .flush({}, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('error');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Възникна грешка');
    expect((fixture.nativeElement as HTMLElement).querySelector('.spinner')).toBeNull();
  });

  it('renders backend financial summaries without requesting vehicle details', async () => {
    const fixture = TestBed.createComponent(VehicleListComponent);
    fixture.detectChanges();

    http.expectOne('/api/vehicles?includeArchived=false').flush([vehicleListItem]);
    await fixture.whenStable();

    http.expectNone('/api/vehicles/2');
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('€10,600');
    expect(text).toContain('€9,350');
    expect(text).toContain('26.3823%');
  });

  it('reloads through the summary endpoint when archived vehicles are requested', async () => {
    const fixture = TestBed.createComponent(VehicleListComponent);
    fixture.detectChanges();
    http.expectOne('/api/vehicles?includeArchived=false').flush([vehicleListItem]);
    await fixture.whenStable();

    fixture.componentInstance.setIncludeArchived(true);
    expect(fixture.componentInstance.pageState().status).toBe('loading');
    http.expectOne('/api/vehicles?includeArchived=true').flush([vehicleListItem]);
    await fixture.whenStable();

    expect(fixture.componentInstance.pageState().status).toBe('loaded');
    expect(fixture.componentInstance.includeArchived()).toBe(true);
    http.expectNone('/api/vehicles/2');
  });
});
