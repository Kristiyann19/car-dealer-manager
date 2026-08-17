import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';

import { FinancialCalculationResult } from '../../../core/models/vehicle.models';
import { UiLabelPipe } from '../../../core/localization/ui-label.pipe';
import { MaxBidCardComponent } from './max-bid-card.component';

@Component({
  selector: 'app-financial-results',
  imports: [CurrencyPipe, DecimalPipe, MaxBidCardComponent, UiLabelPipe],
  templateUrl: './financial-results.component.html',
  styleUrl: './financial-results.component.css',
})
export class FinancialResultsComponent {
  @Input({ required: true }) analysis!: FinancialCalculationResult;
}
