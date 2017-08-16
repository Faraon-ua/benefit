using System;
using System.Linq;
using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Benefit.CardReader.DataTransfer.Ingest;
using Benefit.CardReader.DataTransfer.Offline;
using Benefit.CardReader.Services.Factories;

namespace Benefit.CardReader.Services
{
    public class OfflineService : IReaderManager
    {
        private DataService _dataService;

        public OfflineService()
        {
            _dataService = new DataService();
        }
        public string GetAuthToken(string licenseKey)
        {
            return _dataService.Get<string>().FirstOrDefault();
        }

        public SellerCashierAuthDto AuthCashier(string cashierNfc)
        {
            var cashier = _dataService.Get<Cashier>().FirstOrDefault(entry => entry.CardNfc == cashierNfc);
            if (cashier == null) return null;
            return new SellerCashierAuthDto()
            {
                CashierName = cashier.Name,
                SellerName = cashier.SellerName
            };
        }

        public BenefitCardUserAuthDto AuthUser(string userNfc)
        {
            return new BenefitCardUserAuthDto()
            {
                Name = "Недоступно в автономному режимі",
                CardNumber = "Недоступно в автономному режимі"
            };
        }

        public BenefitCardUserDto GetUserInfo(string userNfc)
        {
            throw new NotImplementedException();
        }

        public ResponseResult<PaymentResultDto> ProcessPayment(PaymentIngest paymentIngest)
        {
            throw new NotImplementedException();
        }
    }
}
