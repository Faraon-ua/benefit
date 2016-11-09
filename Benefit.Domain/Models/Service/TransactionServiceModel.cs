using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels
{
    public class TransactionServiceModel
    {
        public Transaction Transaction { get; set; }
        public double PointsAmount { get; set; }
    }
}
