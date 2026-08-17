import { Component, EventEmitter, inject, Input, OnChanges, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  EditableCandidateStatus,
  FuelType,
  TransmissionType,
  VehicleDetail,
  VehicleUpsertRequest,
} from '../../../core/models/vehicle.models';
import { UiLabelPipe } from '../../../core/localization/ui-label.pipe';

@Component({
  selector: 'app-vehicle-form',
  imports: [ReactiveFormsModule, UiLabelPipe],
  templateUrl: './vehicle-form.component.html',
  styleUrl: './vehicle-form.component.css',
})
export class VehicleFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input() vehicle: VehicleDetail | null = null;
  @Input() saving = false;
  @Input() readOnly = false;
  @Input() submitLabel = 'Запази автомобила';
  @Input() allowStatusChange = false;
  @Output() readonly submitted = new EventEmitter<VehicleUpsertRequest>();

  readonly statuses: EditableCandidateStatus[] = [
    'Candidate',
    'Bidding',
    'Rejected',
    'LostAuction',
  ];
  readonly fuelTypes: FuelType[] = [
    'Gasoline',
    'Diesel',
    'Hybrid',
    'PlugInHybrid',
    'Electric',
    'Lpg',
    'Cng',
    'Other',
  ];
  readonly transmissionTypes: TransmissionType[] = [
    'Manual',
    'Automatic',
    'SemiAutomatic',
    'Other',
  ];

  readonly form = this.fb.group({
    status: this.fb.nonNullable.control<EditableCandidateStatus>('Candidate'),
    brand: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    model: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    year: this.fb.nonNullable.control(new Date().getFullYear(), [
      Validators.required,
      Validators.min(1886),
      Validators.max(2200),
    ]),
    vin: this.fb.control<string | null>(null, Validators.maxLength(50)),
    mileage: this.fb.control<number | null>(null, Validators.min(0)),
    engine: this.fb.control<string | null>(null, Validators.maxLength(100)),
    fuelType: this.fb.control<FuelType | null>(null),
    transmissionType: this.fb.control<TransmissionType | null>(null),
    sourcePlatform: this.fb.control<string | null>(null, Validators.maxLength(100)),
    sourceUrl: this.fb.control<string | null>(null, Validators.maxLength(2048)),
    sourceCountry: this.fb.control<string | null>(null, Validators.maxLength(100)),
    physicalLocation: this.fb.control<string | null>(null, Validators.maxLength(200)),
    currentBid: this.fb.control<number | null>(null, Validators.min(0)),
    myBid: this.fb.control<number | null>(null, Validators.min(0)),
    analysisPurchasePrice: this.fb.control<number | null>(null, Validators.min(0)),
    bidIncrement: this.fb.nonNullable.control(1, [Validators.required, Validators.min(0.01)]),
    expectedSalePrice: this.fb.control<number | null>(null, Validators.min(0)),
    conservativeSalePrice: this.fb.control<number | null>(null, Validators.min(0)),
    plannedListingPrice: this.fb.control<number | null>(null, Validators.min(0)),
    minimumAcceptableSalePrice: this.fb.control<number | null>(null, Validators.min(0)),
    minimumProfitAmount: this.fb.control<number | null>(null, Validators.min(0)),
    minimumRoiPercentage: this.fb.control<number | null>(null, Validators.min(0)),
  });

  ngOnChanges(): void {
    if (this.vehicle) {
      this.form.reset({
        status: this.asEditableStatus(this.vehicle.status),
        brand: this.vehicle.brand,
        model: this.vehicle.model,
        year: this.vehicle.year,
        vin: this.vehicle.vin,
        mileage: this.vehicle.mileage,
        engine: this.vehicle.engine,
        fuelType: this.vehicle.fuelType,
        transmissionType: this.vehicle.transmissionType,
        sourcePlatform: this.vehicle.sourcePlatform,
        sourceUrl: this.vehicle.sourceUrl,
        sourceCountry: this.vehicle.sourceCountry,
        physicalLocation: this.vehicle.physicalLocation,
        currentBid: this.vehicle.currentBid,
        myBid: this.vehicle.myBid,
        analysisPurchasePrice: this.vehicle.analysisPurchasePrice,
        bidIncrement: this.vehicle.bidIncrement,
        expectedSalePrice: this.vehicle.expectedSalePrice,
        conservativeSalePrice: this.vehicle.conservativeSalePrice,
        plannedListingPrice: this.vehicle.plannedListingPrice,
        minimumAcceptableSalePrice: this.vehicle.minimumAcceptableSalePrice,
        minimumProfitAmount: this.vehicle.minimumProfitAmount,
        minimumRoiPercentage: this.vehicle.minimumRoiPercentage,
      });
    }

    if (this.readOnly) {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
      if (!this.allowStatusChange) {
        this.form.controls.status.disable({ emitEvent: false });
      }
    }
  }

  submit(): void {
    if (this.form.invalid || this.readOnly) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitted.emit({
      ...value,
      brand: value.brand.trim(),
      model: value.model.trim(),
      vin: this.optionalText(value.vin),
      engine: this.optionalText(value.engine),
      sourcePlatform: this.optionalText(value.sourcePlatform),
      sourceUrl: this.optionalText(value.sourceUrl),
      sourceCountry: this.optionalText(value.sourceCountry),
      physicalLocation: this.optionalText(value.physicalLocation),
    });
  }

  isInvalid(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  private optionalText(value: string | null): string | null {
    const normalized = value?.trim();
    return normalized ? normalized : null;
  }

  private asEditableStatus(status: string): EditableCandidateStatus {
    return this.statuses.includes(status as EditableCandidateStatus)
      ? (status as EditableCandidateStatus)
      : 'Candidate';
  }
}
