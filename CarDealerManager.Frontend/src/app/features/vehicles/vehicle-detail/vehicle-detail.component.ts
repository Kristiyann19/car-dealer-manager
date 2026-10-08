import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  catchError,
  distinctUntilChanged,
  finalize,
  map,
  Observable,
  of,
  startWith,
  Subject,
  switchMap,
} from 'rxjs';

import {
  CostEntryCalculation,
  EstimatedCostEntryUpsertRequest,
  VehicleCostCategory,
  VehicleCostEntry,
  VehicleDetail,
  VehicleUpsertRequest,
} from '../../../core/models/vehicle.models';
import { translateUiMessage, UiLabelPipe } from '../../../core/localization/ui-label.pipe';
import { VehicleApiService } from '../../../core/services/vehicle-api.service';
import { FinancialResultsComponent } from '../financial-results/financial-results.component';
import { VehicleFormComponent } from '../vehicle-form/vehicle-form.component';

type VehicleDetailPageState =
  | { status: 'loading' }
  | { status: 'loaded'; vehicle: VehicleDetail }
  | { status: 'error'; message: string; notFound: boolean };

@Component({
  selector: 'app-vehicle-detail',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    FinancialResultsComponent,
    ReactiveFormsModule,
    RouterLink,
    UiLabelPipe,
    VehicleFormComponent,
  ],
  templateUrl: './vehicle-detail.component.html',
  styleUrl: './vehicle-detail.component.css',
})
export class VehicleDetailComponent implements OnInit {
  private readonly api = inject(VehicleApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reloadRequests = new Subject<void>();

  readonly categories: VehicleCostCategory[] = [
    'AuctionFee',
    'PlatformFee',
    'BrokerFee',
    'Transport',
    'Repair',
    'Part',
    'Labor',
    'Service',
    'RegistrationDocuments',
    'DetailingPreparation',
    'BankPaymentFee',
    'SellingFee',
    'Miscellaneous',
  ];

  readonly costForm = this.fb.group({
    category: this.fb.nonNullable.control<VehicleCostCategory>('Transport', Validators.required),
    description: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(500)]),
    amount: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0)]),
    purchasePricePercentage: this.fb.control<number | null>(null, Validators.min(0)),
    taxPercentage: this.fb.control<number | null>(null, Validators.min(0)),
    riskPercentage: this.fb.control<number | null>(null, Validators.min(0)),
    entryDate: this.fb.control<string | null>(null),
    includedInAnalysis: this.fb.nonNullable.control(true),
    notes: this.fb.control<string | null>(null, Validators.maxLength(2000)),
  });

  readonly pageState = signal<VehicleDetailPageState>({ status: 'loading' });
  readonly vehicle = computed(() => {
    const state = this.pageState();
    return state.status === 'loaded' ? state.vehicle : null;
  });
  readonly pageError = computed(() => {
    const state = this.pageState();
    return state.status === 'error' ? state : null;
  });
  readonly activeEstimatedCosts = computed(() =>
    (this.vehicle()?.costEntries ?? []).filter(
      (entry) => entry.kind === 'Estimated' && entry.archivedAtUtc === null,
    ),
  );
  readonly archivedEstimatedCosts = computed(() =>
    (this.vehicle()?.costEntries ?? []).filter(
      (entry) => entry.kind === 'Estimated' && entry.archivedAtUtc !== null,
    ),
  );
  readonly canEdit = computed(() => this.vehicle()?.archivedAtUtc === null);
  readonly savingVehicle = signal(false);
  readonly savingCost = signal(false);
  readonly actionError = signal('');
  readonly costError = signal('');
  readonly showCostForm = signal(false);
  readonly editingCostId = signal<number | null>(null);

  private vehicleId = 0;

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map((params) => params.get('id')),
        distinctUntilChanged(),
        switchMap((idValue) => {
          const id = Number(idValue);
          if (!Number.isInteger(id) || id <= 0) {
            this.vehicleId = 0;
            return of<VehicleDetailPageState>({
              status: 'error',
              message: 'Невалиден идентификатор на автомобил.',
              notFound: true,
            });
          }

          this.vehicleId = id;
          return this.reloadRequests.pipe(
            startWith(undefined),
            switchMap(() => this.loadVehicle(id)),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((state) => this.pageState.set(state));
  }

  saveVehicle(request: VehicleUpsertRequest): void {
    this.savingVehicle.set(true);
    this.actionError.set('');
    this.api
      .updateVehicle(this.vehicleId, request)
      .pipe(finalize(() => this.savingVehicle.set(false)))
      .subscribe({
        next: (vehicle) => this.pageState.set({ status: 'loaded', vehicle }),
        error: (error) =>
          this.actionError.set(this.errorMessage(error, 'Автомобилът не можа да бъде запазен.')),
      });
  }

  startAddCost(): void {
    this.editingCostId.set(null);
    this.costError.set('');
    this.costForm.reset({
      category: 'Transport',
      description: '',
      amount: 0,
      purchasePricePercentage: null,
      taxPercentage: null,
      riskPercentage: null,
      entryDate: null,
      includedInAnalysis: true,
      notes: null,
    });
    this.showCostForm.set(true);
  }

  startEditCost(cost: VehicleCostEntry): void {
    this.editingCostId.set(cost.id);
    this.costError.set('');
    this.costForm.reset({
      category: cost.category,
      description: cost.description,
      amount: cost.amount,
      purchasePricePercentage: cost.purchasePricePercentage,
      taxPercentage: cost.taxPercentage,
      riskPercentage: cost.riskPercentage,
      entryDate: cost.entryDate,
      includedInAnalysis: cost.includedInAnalysis,
      notes: cost.notes,
    });
    this.showCostForm.set(true);
  }

  cancelCostEdit(): void {
    this.showCostForm.set(false);
    this.editingCostId.set(null);
    this.costError.set('');
  }

  saveCost(): void {
    if (this.costForm.invalid) {
      this.costForm.markAllAsTouched();
      return;
    }

    const request = this.costForm.getRawValue();
    if (request.amount === 0 && (request.purchasePricePercentage ?? 0) === 0) {
      this.costError.set('Въведете фиксирана сума, процент от покупната цена или и двете.');
      return;
    }

    this.savingCost.set(true);
    this.costError.set('');
    const editingCostId = this.editingCostId();
    const operation = editingCostId
      ? this.api.updateEstimatedCost(this.vehicleId, editingCostId, request)
      : this.api.addEstimatedCost(this.vehicleId, request);

    operation.pipe(finalize(() => this.savingCost.set(false))).subscribe({
      next: () => {
        this.cancelCostEdit();
        this.reloadRequests.next();
      },
      error: (error) =>
        this.costError.set(this.errorMessage(error, 'Разходът не можа да бъде запазен.')),
    });
  }

  toggleIncluded(cost: VehicleCostEntry, includedInAnalysis: boolean): void {
    this.savingCost.set(true);
    this.costError.set('');
    this.api
      .updateEstimatedCost(this.vehicleId, cost.id, {
        ...this.toCostRequest(cost),
        includedInAnalysis,
      })
      .pipe(finalize(() => this.savingCost.set(false)))
      .subscribe({
        next: () => this.reloadRequests.next(),
        error: (error) =>
          this.costError.set(
            this.errorMessage(error, 'Включването в сметката не можа да бъде обновено.'),
          ),
      });
  }

  archiveCost(cost: VehicleCostEntry): void {
    if (!window.confirm(`Да се архивира ли разходът „${cost.description}“?`)) {
      return;
    }

    this.savingCost.set(true);
    this.costError.set('');
    this.api
      .archiveEstimatedCost(this.vehicleId, cost.id)
      .pipe(finalize(() => this.savingCost.set(false)))
      .subscribe({
        next: () => this.reloadRequests.next(),
        error: (error) =>
          this.costError.set(this.errorMessage(error, 'Разходът не можа да бъде архивиран.')),
      });
  }

  breakdownFor(costId: number): CostEntryCalculation | null {
    return (
      this.vehicle()?.financialAnalysis.estimatedCostsAtAnalysisPrice?.entries.find(
        (entry) => entry.costEntryId === costId,
      ) ?? null
    );
  }

  private loadVehicle(id: number): Observable<VehicleDetailPageState> {
    this.pageState.set({ status: 'loading' });
    this.actionError.set('');

    return this.api.getVehicle(id).pipe(
      map((vehicle): VehicleDetailPageState => ({ status: 'loaded', vehicle })),
      catchError((error: unknown) => {
        const notFound = error instanceof HttpErrorResponse && error.status === 404;
        return of<VehicleDetailPageState>({
          status: 'error',
          message: notFound
            ? 'Няма автомобил с този идентификатор.'
            : this.errorMessage(error, 'Автомобилът не можа да бъде зареден.'),
          notFound,
        });
      }),
    );
  }

  private toCostRequest(cost: VehicleCostEntry): EstimatedCostEntryUpsertRequest {
    return {
      category: cost.category,
      description: cost.description,
      amount: cost.amount,
      purchasePricePercentage: cost.purchasePricePercentage,
      taxPercentage: cost.taxPercentage,
      riskPercentage: cost.riskPercentage,
      entryDate: cost.entryDate,
      includedInAnalysis: cost.includedInAnalysis,
      notes: cost.notes,
    };
  }

  private errorMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    return translateUiMessage(error.error?.detail ?? error.error?.title, fallback);
  }
}
