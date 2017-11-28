using System;

namespace Benefit.CardReader.DataTransfer.Offline
{
    [Serializable]
    public class Cashier
    {
        public string CardNfc { get; set; }
        public string Name { get; set; }
        public string SellerName { get; set; }
        public bool SellerShowBill { get; set; }
        public bool SellerShowKeyboard { get; set; }

        public override bool Equals(object obj)
        {
            return (obj as Cashier).CardNfc == CardNfc;
        }
    }
}
