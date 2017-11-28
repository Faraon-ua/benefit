using System;

namespace Benefit.Common.CustomEventArgs
{
    public class ButtonEventArgs : EventArgs
    {
        public ButtonEventArgs(string name, string text)
        {
            Name = name;
            Text = text;
        }
        public string Name{ get; set; }
        public string Text{ get; set; }
    }
}
