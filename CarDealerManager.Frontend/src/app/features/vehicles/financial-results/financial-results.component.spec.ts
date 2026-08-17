import { TestBed } from '@angular/core/testing';

import { FinancialCalculationResult, MaxBidCalculation } from '../../../core/models/vehicle.models';
import { FinancialResultsComponent } from './financial-results.component';

describe('FinancialResultsComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FinancialResultsComponent],
    }).compileComponents();
  });

  it('renders the validated Peugeot financial results without recalculating them', async () => {
    const fixture = TestBed.createComponent(FinancialResultsComponent);
    fixture.componentRef.setInput('analysis', peugeotAnalysis);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('€10,000');
    expect(text).toContain('€2,660');
    expect(text).toContain('€12,660');
    expect(text).toContain('€3,340');
    expect(text).toContain('26.3823%');
    expect(text).toContain('€1,840');
    expect(text).toContain('14.534%');
    expect(text).toContain('€10,600');
    expect(text).toContain('€9,350');
    expect(text).toContain('Изчислен');
    expect(text).toContain('Минимален ROI');
    expect(text).not.toContain('Calculated');
    expect(text).not.toContain('MinimumRoi');
  });
});

const expectedMaxBid: MaxBidCalculation = {
  status: 'Calculated',
  reason: null,
  salePrice: 16000,
  minimumProfitAmount: 2500,
  minimumRoiPercentage: 20,
  bidIncrement: 50,
  maxBidByMinimumProfit: 10810.8108,
  maxBidByMinimumRoi: 10649.9356,
  mathematicalMaxBid: 10649.9356,
  finalMaxBid: 10600,
  bindingConstraint: 'MinimumRoi',
  estimatedCostsAtFinalBid: null,
  totalInvestmentAtFinalBid: 13281.6,
  profitAtFinalBid: 2718.4,
  roiPercentageAtFinalBid: 20.4674,
  marginPercentageAtFinalBid: 16.99,
  minimumProfitSatisfied: true,
  minimumRoiSatisfied: true,
  allConstraintsSatisfied: true,
};

const peugeotAnalysis: FinancialCalculationResult = {
  reportingCurrency: 'EUR',
  analysisPurchasePrice: 10000,
  estimatedCostsAtAnalysisPrice: {
    purchasePrice: 10000,
    preTaxTotal: 2400,
    taxTotal: 110,
    rawEstimatedTotal: 2510,
    riskContingencyTotal: 150,
    riskAdjustedTotal: 2660,
    entries: [],
  },
  expectedScenario: {
    salePrice: 16000,
    totalEstimatedInvestment: 12660,
    profit: 3340,
    roiPercentage: 26.3823064771,
    marginPercentage: 20.875,
  },
  conservativeScenario: {
    salePrice: 14500,
    totalEstimatedInvestment: 12660,
    profit: 1840,
    roiPercentage: 14.5339652449,
    marginPercentage: 12.6896551724,
  },
  expectedMaxBid,
  conservativeMaxBid: {
    ...expectedMaxBid,
    salePrice: 14500,
    maxBidByMinimumProfit: 9362.9344,
    maxBidByMinimumRoi: 9443.3719,
    mathematicalMaxBid: 9362.9344,
    finalMaxBid: 9350,
    bindingConstraint: 'MinimumProfit',
    totalInvestmentAtFinalBid: 11986.6,
    profitAtFinalBid: 2513.4,
    roiPercentageAtFinalBid: 20.9684,
    marginPercentageAtFinalBid: 17.3338,
  },
};
