using System;
using System.Collections;
using System.Collections.Generic;

namespace Benefit.CardReader.DataTransfer.Offline
{
    [Serializable]
    public class Cashier
    {
        public string CardNfc { get; set; }
        public string Name { get; set; }
        public string SellerName { get; set; }
        public override bool Equals(object obj)
        {
            return (obj as Cashier).CardNfc == CardNfc;
        }
    }
}
