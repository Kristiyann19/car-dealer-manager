export type VehicleStatus =
  | 'Candidate'
  | 'Bidding'
  | 'Purchased'
  | 'InTransit'
  | 'InPreparation'
  | 'Listed'
  | 'Sold'
  | 'Rejected'
  | 'LostAuction';

export type EditableCandidateStatus = 'Candidate' | 'Bidding' | 'Rejected' | 'LostAuction';

export type FuelType =
  'Gasoline' | 'Diesel' | 'Hybrid' | 'PlugInHybrid' | 'Electric' | 'Lpg' | 'Cng' | 'Other';

export type TransmissionType = 'Manual' | 'Automatic' | 'SemiAutomatic' | 'Other';

export type VehicleCostCategory =
  | 'AuctionFee'
  | 'PlatformFee'
  | 'BrokerFee'
  | 'Transport'
  | 'Repair'
  | 'Part'
  | 'Labor'
  | 'Service'
  | 'RegistrationDocuments'
  | 'DetailingPreparation'
  | 'BankPaymentFee'
  | 'SellingFee'
  | 'Miscellaneous';

export type MaxBidStatus = 'Calculated' | 'InsufficientData' | 'NotFinanciallyViable';
export type MaxBidBindingConstraint = 'None' | 'MinimumProfit' | 'MinimumRoi' | 'Both';

export interface VehicleListItem {
  id: number;
  status: VehicleStatus;
  brand: string;
  model: string;
  year: number;
  vin: string | null;
  sourceCountry: string | null;
  physicalLocation: string | null;
  currentBid: number | null;
  myBid: number | null;
  analysisPurchasePrice: number | null;
  expectedSalePrice: number | null;
  conservativeSalePrice: number | null;
  archivedAtUtc: string | null;
  financialSummary: VehicleListFinancialSummary;
}

export interface VehicleListFinancialSummary {
  expectedMaxBid: VehicleListMaxBid;
  conservativeMaxBid: VehicleListMaxBid;
  expectedRoiPercentage: number | null;
}

export interface VehicleListMaxBid {
  status: MaxBidStatus;
  finalMaxBid: number | null;
}

export interface VehicleCostEntry {
  id: number;
  kind: 'Estimated' | 'Actual';
  category: VehicleCostCategory;
  description: string;
  amount: number;
  purchasePricePercentage: number | null;
  taxPercentage: number | null;
  riskPercentage: number | null;
  entryDate: string | null;
  relatedEstimateId: number | null;
  includedInAnalysis: boolean;
  notes: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  archivedAtUtc: string | null;
}

export interface CostEntryCalculation {
  costEntryId: number;
  category: VehicleCostCategory;
  description: string;
  fixedAmount: number;
  purchasePricePercentage: number;
  taxPercentage: number;
  riskPercentage: number;
  preTaxAmount: number;
  taxAmount: number;
  taxAdjustedAmount: number;
  riskAmount: number;
  riskAdjustedAmount: number;
}

export interface EstimatedCostSummary {
  purchasePrice: number;
  preTaxTotal: number;
  taxTotal: number;
  rawEstimatedTotal: number;
  riskContingencyTotal: number;
  riskAdjustedTotal: number;
  entries: CostEntryCalculation[];
}

export interface SaleScenarioMetrics {
  salePrice: number | null;
  totalEstimatedInvestment: number | null;
  profit: number | null;
  roiPercentage: number | null;
  marginPercentage: number | null;
}

export interface MaxBidCalculation {
  status: MaxBidStatus;
  reason: string | null;
  salePrice: number | null;
  minimumProfitAmount: number | null;
  minimumRoiPercentage: number | null;
  bidIncrement: number;
  maxBidByMinimumProfit: number | null;
  maxBidByMinimumRoi: number | null;
  mathematicalMaxBid: number | null;
  finalMaxBid: number | null;
  bindingConstraint: MaxBidBindingConstraint;
  estimatedCostsAtFinalBid: EstimatedCostSummary | null;
  totalInvestmentAtFinalBid: number | null;
  profitAtFinalBid: number | null;
  roiPercentageAtFinalBid: number | null;
  marginPercentageAtFinalBid: number | null;
  minimumProfitSatisfied: boolean | null;
  minimumRoiSatisfied: boolean | null;
  allConstraintsSatisfied: boolean | null;
}

export interface FinancialCalculationResult {
  reportingCurrency: string;
  analysisPurchasePrice: number | null;
  estimatedCostsAtAnalysisPrice: EstimatedCostSummary | null;
  expectedScenario: SaleScenarioMetrics;
  conservativeScenario: SaleScenarioMetrics;
  expectedMaxBid: MaxBidCalculation;
  conservativeMaxBid: MaxBidCalculation;
}

export interface VehicleDetail extends Omit<VehicleListItem, 'financialSummary'> {
  mileage: number | null;
  engine: string | null;
  fuelType: FuelType | null;
  transmissionType: TransmissionType | null;
  sourcePlatform: string | null;
  sourceUrl: string | null;
  sourceCountry: string | null;
  physicalLocation: string | null;
  bidIncrement: number;
  finalPurchasePrice: number | null;
  purchaseDate: string | null;
  plannedListingPrice: number | null;
  minimumAcceptableSalePrice: number | null;
  actualSalePrice: number | null;
  listingDate: string | null;
  saleDate: string | null;
  minimumProfitAmount: number | null;
  minimumRoiPercentage: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  costEntries: VehicleCostEntry[];
  financialAnalysis: FinancialCalculationResult;
}

export interface VehicleUpsertRequest {
  status: EditableCandidateStatus;
  brand: string;
  model: string;
  year: number;
  vin: string | null;
  mileage: number | null;
  engine: string | null;
  fuelType: FuelType | null;
  transmissionType: TransmissionType | null;
  sourcePlatform: string | null;
  sourceUrl: string | null;
  sourceCountry: string | null;
  physicalLocation: string | null;
  currentBid: number | null;
  myBid: number | null;
  analysisPurchasePrice: number | null;
  bidIncrement: number;
  expectedSalePrice: number | null;
  conservativeSalePrice: number | null;
  plannedListingPrice: number | null;
  minimumAcceptableSalePrice: number | null;
  minimumProfitAmount: number | null;
  minimumRoiPercentage: number | null;
}

export interface EstimatedCostEntryUpsertRequest {
  category: VehicleCostCategory;
  description: string;
  amount: number;
  purchasePricePercentage: number | null;
  taxPercentage: number | null;
  riskPercentage: number | null;
  entryDate: string | null;
  includedInAnalysis: boolean;
  notes: string | null;
}
