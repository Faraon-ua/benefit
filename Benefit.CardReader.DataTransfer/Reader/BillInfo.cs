using System;

namespace Benefit.CardReader.DataTransfer.Reader
{
    [Serializable]
    public class BillInfo
    {
        public double Sum { get; set; }
        public string Number { get; set; }
        public bool ChargeBonuses { get; set; }

        public override string ToString()
        {
            return string.Format("Sum: {0}, BillNumber: {1}, ChargeBonuses: {2}", Sum, Number, ChargeBonuses);
        }
    }
}
