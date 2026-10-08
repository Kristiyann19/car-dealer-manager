import { UiLabelKind, UiLabelPipe, translateUiMessage } from './ui-label.pipe';

describe('UiLabelPipe', () => {
  const pipe = new UiLabelPipe();

  const expectedLabels: Record<UiLabelKind, Record<string, string>> = {
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

  for (const [kind, labels] of Object.entries(expectedLabels) as [
    UiLabelKind,
    Record<string, string>,
  ][]) {
    it(`translates every ${kind} value`, () => {
      for (const [value, expected] of Object.entries(labels)) {
        expect(pipe.transform(value, kind)).toBe(expected);
      }
    });
  }

  it('translates financial calculation reasons without changing the API contract', () => {
    expect(
      translateUiMessage('No positive purchase price can satisfy the configured targets.'),
    ).toBe('Няма положителна покупна цена, която да покрива зададените цели.');
  });
});
