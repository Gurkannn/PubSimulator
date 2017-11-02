using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public class ActionEventArgs : EventArgs
    {
        public string ActionValue { get; set; }
        public ActionEventArgs(string value)
        {
            ActionValue = value;
        }
    }
}
