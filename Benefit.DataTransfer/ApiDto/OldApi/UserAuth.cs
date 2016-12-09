using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.OldApi
{
    public class UserAuthIngest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string cardnfc { get; set; } 
    }

    public class UserAuthDto
    {
        public string typeuser { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string card { get; set; }
        public float paybals { get; set; }
        public float paybonus { get; set; }
        public float bonus { get; set; }
        public float balance { get; set; }
        public float hold { get; set; }
        public float charged { get; set; }
    }

}
