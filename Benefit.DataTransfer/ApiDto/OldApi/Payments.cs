using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.OldApi
{
    public class PaymentIngest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string cardKassir { get; set; }
        public string paymentcard { get; set; }
        public string numberchek { get; set; }
        public float summachek { get; set; }
    }

    public class PaymentDto
    {
        public string result { get; set; }
        public float balance { get; set; }
        public float hold { get; set; }
        public float charged { get; set; }
    }
}
