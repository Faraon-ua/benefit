using System;
using System.Runtime.InteropServices;

namespace Benefit.CardReader.Communication
{
    public class CommunicationService
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }
}
