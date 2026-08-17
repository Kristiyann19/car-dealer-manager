import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';

import { MaxBidCalculation } from '../../../core/models/vehicle.models';
import { UiLabelPipe, UiMessagePipe } from '../../../core/localization/ui-label.pipe';

@Component({
  selector: 'app-max-bid-card',
  imports: [CurrencyPipe, DecimalPipe, UiLabelPipe, UiMessagePipe],
  templateUrl: './max-bid-card.component.html',
  styleUrl: './max-bid-card.component.css',
})
export class MaxBidCardComponent {
  @Input({ required: true }) title = '';
  @Input({ required: true }) calculation!: MaxBidCalculation;

  get statusClass(): string {
    return `max-bid-status status-${this.calculation.status.toLowerCase()}`;
  }
}
