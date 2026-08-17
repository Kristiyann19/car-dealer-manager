import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

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

  vehicle: VehicleDetail | null = null;
  loading = true;
  savingVehicle = false;
  savingCost = false;
  error = '';
  costError = '';
  showCostForm = false;
  editingCostId: number | null = null;

  private vehicleId = 0;

  get activeEstimatedCosts(): VehicleCostEntry[] {
    return (this.vehicle?.costEntries ?? []).filter(
      (entry) => entry.kind === 'Estimated' && entry.archivedAtUtc === null,
    );
  }

  get archivedEstimatedCosts(): VehicleCostEntry[] {
    return (this.vehicle?.costEntries ?? []).filter(
      (entry) => entry.kind === 'Estimated' && entry.archivedAtUtc !== null,
    );
  }

  get canEdit(): boolean {
    return !!this.vehicle && this.vehicle.archivedAtUtc === null;
  }

  ngOnInit(): void {
    this.vehicleId = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(this.vehicleId) || this.vehicleId <= 0) {
      this.error = 'Невалиден идентификатор на автомобил.';
      this.loading = false;
      return;
    }
    this.load(true);
  }

  saveVehicle(request: VehicleUpsertRequest): void {
    this.savingVehicle = true;
    this.error = '';
    this.api
      .updateVehicle(this.vehicleId, request)
      .pipe(finalize(() => (this.savingVehicle = false)))
      .subscribe({
        next: () => this.load(false),
        error: (error) =>
          (this.error = this.errorMessage(error, 'Автомобилът не можа да бъде запазен.')),
      });
  }

  startAddCost(): void {
    this.editingCostId = null;
    this.costError = '';
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
    this.showCostForm = true;
  }

  startEditCost(cost: VehicleCostEntry): void {
    this.editingCostId = cost.id;
    this.costError = '';
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
    this.showCostForm = true;
  }

  cancelCostEdit(): void {
    this.showCostForm = false;
    this.editingCostId = null;
    this.costError = '';
  }

  saveCost(): void {
    if (this.costForm.invalid) {
      this.costForm.markAllAsTouched();
      return;
    }

    const request = this.costForm.getRawValue();
    if (request.amount === 0 && (request.purchasePricePercentage ?? 0) === 0) {
      this.costError = 'Въведете фиксирана сума, процент от покупната цена или и двете.';
      return;
    }

    this.savingCost = true;
    this.costError = '';
    const operation = this.editingCostId
      ? this.api.updateEstimatedCost(this.vehicleId, this.editingCostId, request)
      : this.api.addEstimatedCost(this.vehicleId, request);

    operation.pipe(finalize(() => (this.savingCost = false))).subscribe({
      next: () => {
        this.cancelCostEdit();
        this.load(false);
      },
      error: (error) =>
        (this.costError = this.errorMessage(error, 'Разходът не можа да бъде запазен.')),
    });
  }

  toggleIncluded(cost: VehicleCostEntry, includedInAnalysis: boolean): void {
    this.savingCost = true;
    this.costError = '';
    this.api
      .updateEstimatedCost(this.vehicleId, cost.id, {
        ...this.toCostRequest(cost),
        includedInAnalysis,
      })
      .pipe(finalize(() => (this.savingCost = false)))
      .subscribe({
        next: () => this.load(false),
        error: (error) =>
          (this.costError = this.errorMessage(
            error,
            'Включването в сметката не можа да бъде обновено.',
          )),
      });
  }

  archiveCost(cost: VehicleCostEntry): void {
    if (!window.confirm(`Да се архивира ли разходът „${cost.description}“?`)) {
      return;
    }

    this.savingCost = true;
    this.costError = '';
    this.api
      .archiveEstimatedCost(this.vehicleId, cost.id)
      .pipe(finalize(() => (this.savingCost = false)))
      .subscribe({
        next: () => this.load(false),
        error: (error) =>
          (this.costError = this.errorMessage(error, 'Разходът не можа да бъде архивиран.')),
      });
  }

  breakdownFor(costId: number): CostEntryCalculation | null {
    return (
      this.vehicle?.financialAnalysis.estimatedCostsAtAnalysisPrice?.entries.find(
        (entry) => entry.costEntryId === costId,
      ) ?? null
    );
  }

  private load(showSpinner: boolean): void {
    if (showSpinner) {
      this.loading = true;
    }
    this.api
      .getVehicle(this.vehicleId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (vehicle) => {
          this.vehicle = vehicle;
          this.error = '';
        },
        error: (error) =>
          (this.error = this.errorMessage(error, 'Автомобилът не можа да бъде зареден.')),
      });
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

  private errorMessage(
    error: { error?: { detail?: string; title?: string } },
    fallback: string,
  ): string {
    return translateUiMessage(error.error?.detail ?? error.error?.title, fallback);
  }
}
