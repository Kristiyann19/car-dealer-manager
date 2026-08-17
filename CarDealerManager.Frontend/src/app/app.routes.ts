import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'vehicles' },
  {
    path: 'vehicles',
    loadComponent: () =>
      import('./features/vehicles/vehicle-list/vehicle-list.component').then(
        (module) => module.VehicleListComponent,
      ),
  },
  {
    path: 'vehicles/new',
    loadComponent: () =>
      import('./features/vehicles/create-vehicle/create-vehicle.component').then(
        (module) => module.CreateVehicleComponent,
      ),
  },
  {
    path: 'vehicles/:id',
    loadComponent: () =>
      import('./features/vehicles/vehicle-detail/vehicle-detail.component').then(
        (module) => module.VehicleDetailComponent,
      ),
  },
  { path: '**', redirectTo: 'vehicles' },
];
