using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public class GuestEventArgs : EventArgs
    {
        public GuestAgent agent;
        public GuestEventArgs(GuestAgent agent)
        {
            this.agent = agent;
        }
    }
}
