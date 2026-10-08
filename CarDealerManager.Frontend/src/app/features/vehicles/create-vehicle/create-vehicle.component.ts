import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { VehicleUpsertRequest } from '../../../core/models/vehicle.models';
import { translateUiMessage } from '../../../core/localization/ui-label.pipe';
import { VehicleApiService } from '../../../core/services/vehicle-api.service';
import { VehicleFormComponent } from '../vehicle-form/vehicle-form.component';

@Component({
  selector: 'app-create-vehicle',
  imports: [RouterLink, VehicleFormComponent],
  templateUrl: './create-vehicle.component.html',
})
export class CreateVehicleComponent {
  private readonly api = inject(VehicleApiService);
  private readonly router = inject(Router);

  readonly saving = signal(false);
  readonly error = signal('');

  create(request: VehicleUpsertRequest): void {
    this.saving.set(true);
    this.error.set('');
    this.api
      .createVehicle(request)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (vehicle) => void this.router.navigate(['/vehicles', vehicle.id]),
        error: (error) => this.error.set(this.errorMessage(error)),
      });
  }

  private errorMessage(error: { error?: { detail?: string; title?: string } }): string {
    return translateUiMessage(
      error.error?.detail ?? error.error?.title,
      'Автомобилът не можа да бъде създаден.',
    );
  }
}
