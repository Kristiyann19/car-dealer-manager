import { Pipe, PipeTransform } from '@angular/core';

import {
  FuelType,
  MaxBidBindingConstraint,
  MaxBidStatus,
  TransmissionType,
  VehicleCostCategory,
  VehicleStatus,
} from '../models/vehicle.models';

export type UiLabelKind =
  | 'vehicleStatus'
  | 'costCategory'
  | 'fuelType'
  | 'transmissionType'
  | 'maxBidStatus'
  | 'bindingConstraint';

type UiLabelMaps = {
  vehicleStatus: Record<VehicleStatus, string>;
  costCategory: Record<VehicleCostCategory, string>;
  fuelType: Record<FuelType, string>;
  transmissionType: Record<TransmissionType, string>;
  maxBidStatus: Record<MaxBidStatus, string>;
  bindingConstraint: Record<MaxBidBindingConstraint, string>;
};

const LABELS: UiLabelMaps = {
  vehicleStatus: {
    Candidate: 'Кандидат',
    Bidding: 'Наддаване',
    Purchased: 'Закупен',
    InTransit: 'В транспорт',
    InPreparation: 'Подготовка',
    Listed: 'Обявен',
    Sold: 'Продаден',
    Rejected: 'Отказан',
    LostAuction: 'Загубен търг',
  },
  costCategory: {
    AuctionFee: 'Такса търг',
    PlatformFee: 'Такса платформа',
    BrokerFee: 'Комисиона посредник',
    Transport: 'Транспорт',
    Repair: 'Ремонт',
    Part: 'Части',
    Labor: 'Труд',
    Service: 'Обслужване',
    RegistrationDocuments: 'Регистрация и документи',
    DetailingPreparation: 'Детайлинг и подготовка',
    BankPaymentFee: 'Банкова такса',
    SellingFee: 'Разход при продажба',
    Miscellaneous: 'Други',
  },
  fuelType: {
    Gasoline: 'Бензин',
    Diesel: 'Дизел',
    Hybrid: 'Хибрид',
    PlugInHybrid: 'Зареждаем хибрид',
    Electric: 'Електрически',
    Lpg: 'Пропан-бутан',
    Cng: 'Метан',
    Other: 'Друго',
  },
  transmissionType: {
    Manual: 'Ръчна',
    Automatic: 'Автоматична',
    SemiAutomatic: 'Полуавтоматична',
    Other: 'Друга',
  },
  maxBidStatus: {
    Calculated: 'Изчислен',
    InsufficientData: 'Недостатъчно данни',
    NotFinanciallyViable: 'Сделката не е изгодна',
  },
  bindingConstraint: {
    None: 'Няма',
    MinimumProfit: 'Минимална печалба',
    MinimumRoi: 'Минимален ROI',
    Both: 'Минимална печалба и ROI',
  },
};

const API_MESSAGES: Record<string, string> = {
  'A positive sale price is required.': 'Необходима е положителна продажна цена.',
  'At least one financial target is required.': 'Необходима е поне една финансова цел.',
  'No positive purchase price can satisfy the configured targets.':
    'Няма положителна покупна цена, която да покрива зададените цели.',
  'The viable mathematical bid is below the configured bid increment.':
    'Допустимата оферта е под зададената стъпка на наддаване.',
  'The rounded bid did not satisfy all configured constraints.':
    'Закръглената оферта не покрива всички зададени критерии.',
  'Candidate financial inputs are frozen after the vehicle is purchased.':
    'Финансовите данни за кандидата не могат да се променят след покупката.',
  'Archived estimates cannot be edited.': 'Архивираните разходи не могат да се редактират.',
  'Candidate editing supports Candidate, Bidding, Rejected, and LostAuction statuses only.':
    'Редактирането е достъпно само за кандидат, наддаване, отказан и загубен търг.',
  'FuelType is invalid.': 'Невалиден вид гориво.',
  'TransmissionType is invalid.': 'Невалиден вид скоростна кутия.',
  'Brand and model are required.': 'Марката и моделът са задължителни.',
  'Year must be between 1886 and 2200.': 'Годината трябва да е между 1886 и 2200.',
  'Mileage cannot be negative.': 'Пробегът не може да е отрицателен.',
  'BidIncrement must be greater than zero.': 'Стъпката на наддаване трябва да е над нула.',
  'Financial targets cannot be negative.': 'Финансовите цели не могат да са отрицателни.',
  'ConservativeSalePrice cannot exceed ExpectedSalePrice.':
    'Консервативната продажна цена не може да надвишава очакваната.',
  'SourceUrl must be an absolute HTTP or HTTPS URL.':
    'Линкът към обявата трябва да е пълен HTTP или HTTPS адрес.',
  'Cost category is invalid.': 'Невалидна категория разход.',
  'Cost description is required.': 'Описанието на разхода е задължително.',
  'Cost amounts and percentages cannot be negative.':
    'Сумите и процентите на разхода не могат да са отрицателни.',
  'An estimate requires a fixed amount, a purchase price percentage, or both.':
    'Разходът трябва да има фиксирана сума, процент от покупната цена или и двете.',
  'Purchase-price percentages are only supported for acquisition fee categories in V1.':
    'Процент от покупната цена се поддържа само за такси по придобиването във V1.',
  'Estimates used for the acquisition decision are frozen after purchase.':
    'Прогнозните разходи за решението за покупка не могат да се променят след покупката.',
  'One or more validation errors occurred.': 'Едно или повече полета са невалидни.',
};

@Pipe({ name: 'uiLabel' })
export class UiLabelPipe implements PipeTransform {
  transform(value: string | null | undefined, kind: UiLabelKind): string {
    if (!value) {
      return '—';
    }

    const labels = LABELS[kind] as Readonly<Record<string, string>>;
    return labels[value] ?? value;
  }
}

@Pipe({ name: 'uiMessage' })
export class UiMessagePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return translateUiMessage(value);
  }
}

export function translateUiMessage(value: string | null | undefined, fallback = ''): string {
  if (!value) {
    return fallback;
  }

  return API_MESSAGES[value] ?? value;
}
