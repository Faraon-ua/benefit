using System;

namespace Benefit.CardReader.DataTransfer.Reader
{
    [Serializable]
    public class BillInfo
    {
        public double Sum { get; set; }
        public string Number { get; set; }
        public bool ChargeBonuses { get; set; }
    }
}
