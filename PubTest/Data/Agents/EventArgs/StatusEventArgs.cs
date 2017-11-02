using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public class StatusEventArgs : EventArgs
    {
        public StatusEventArgs(int guestsInBarQueue, int guestsInTableQueue, int glassInUseCount, int tableInUseCount)
        {
            GuestsInBarQueue = guestsInBarQueue;
            GuestsInTableQueue = guestsInTableQueue;
            GlassInUseCount = glassInUseCount;
            TableInUseCount = tableInUseCount;
        }

        public int GuestsInBarQueue { get; set; }
        public int GuestsInTableQueue { get; set; }
        public int GlassInUseCount { get; set; }
        public int TableInUseCount { get; set; }
        
    }
}
