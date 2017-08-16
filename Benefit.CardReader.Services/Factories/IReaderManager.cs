using Benefit.CardReader.DataTransfer.Dto;
using Benefit.CardReader.DataTransfer.Dto.Base;
using Benefit.CardReader.DataTransfer.Ingest;

namespace Benefit.CardReader.Services.Factories
{
    public interface IReaderManager
    {
        string GetAuthToken(string licenseKey);
        SellerCashierAuthDto AuthCashier(string cashierNfc);
        BenefitCardUserAuthDto AuthUser(string userNfc);
        BenefitCardUserDto GetUserInfo(string userNfc);
        ResponseResult<PaymentResultDto> ProcessPayment(PaymentIngest paymentIngest);
    }
}
