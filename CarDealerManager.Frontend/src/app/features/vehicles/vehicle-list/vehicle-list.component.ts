import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { VehicleDetail } from '../../../core/models/vehicle.models';
import { UiLabelPipe } from '../../../core/localization/ui-label.pipe';
import { VehicleApiService } from '../../../core/services/vehicle-api.service';

@Component({
  selector: 'app-vehicle-list',
  imports: [CurrencyPipe, DecimalPipe, RouterLink, UiLabelPipe],
  templateUrl: './vehicle-list.component.html',
  styleUrl: './vehicle-list.component.css',
})
export class VehicleListComponent implements OnInit {
  private readonly api = inject(VehicleApiService);

  vehicles: VehicleDetail[] = [];
  includeArchived = false;
  loading = true;
  error = '';

  ngOnInit(): void {
    this.load();
  }

  setIncludeArchived(value: boolean): void {
    this.includeArchived = value;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api
      .getVehicleCards(this.includeArchived)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (vehicles) => (this.vehicles = vehicles),
        error: () =>
          (this.error = 'Автомобилите не могат да бъдат заредени. Проверете дали сървърът работи.'),
      });
  }
}
