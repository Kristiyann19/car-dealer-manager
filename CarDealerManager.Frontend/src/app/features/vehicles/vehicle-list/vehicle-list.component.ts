import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of, Subject, switchMap, tap } from 'rxjs';

import { VehicleListItem } from '../../../core/models/vehicle.models';
import { UiLabelPipe } from '../../../core/localization/ui-label.pipe';
import { VehicleApiService } from '../../../core/services/vehicle-api.service';

type VehicleListPageState =
  | { status: 'loading' }
  | { status: 'loaded'; vehicles: VehicleListItem[] }
  | { status: 'error'; message: string };

@Component({
  selector: 'app-vehicle-list',
  imports: [CurrencyPipe, DecimalPipe, RouterLink, UiLabelPipe],
  templateUrl: './vehicle-list.component.html',
  styleUrl: './vehicle-list.component.css',
})
export class VehicleListComponent implements OnInit {
  private readonly api = inject(VehicleApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly loadRequests = new Subject<boolean>();

  readonly pageState = signal<VehicleListPageState>({ status: 'loading' });
  readonly includeArchived = signal(false);
  readonly vehicles = computed(() => {
    const state = this.pageState();
    return state.status === 'loaded' ? state.vehicles : [];
  });
  readonly errorMessage = computed(() => {
    const state = this.pageState();
    return state.status === 'error' ? state.message : '';
  });

  constructor() {
    this.loadRequests
      .pipe(
        tap(() => this.pageState.set({ status: 'loading' })),
        switchMap((includeArchived) =>
          this.api.getVehicles(includeArchived).pipe(
            map((vehicles): VehicleListPageState => ({
              status: 'loaded',
              vehicles,
            })),
            catchError(() =>
              of<VehicleListPageState>({
                status: 'error',
                message: 'Автомобилите не могат да бъдат заредени. Проверете дали сървърът работи.',
              }),
            ),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((state) => this.pageState.set(state));
  }

  ngOnInit(): void {
    this.load();
  }

  setIncludeArchived(value: boolean): void {
    this.includeArchived.set(value);
    this.load();
  }

  load(): void {
    this.loadRequests.next(this.includeArchived());
  }
}
