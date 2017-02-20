using System;

namespace Benefit.Common.CustomEventArgs
{
    public class NfcEventArgs : EventArgs
    {
        public NfcEventArgs(string nfcCode)
        {
            NfcCode = nfcCode;
        }
        public string NfcCode { get; set; }
    }
}
