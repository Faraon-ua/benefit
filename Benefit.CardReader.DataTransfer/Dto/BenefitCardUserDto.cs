namespace Benefit.CardReader.DataTransfer.Dto
{
    public class BenefitCardUserDto
    {
        public string Name { get; set; }
        public string CardNumber { get; set; }
        public double BonusesAccount { get; set; }
        public CardOrderDto LastCardOrderInfo { get; set; }
    }
}
