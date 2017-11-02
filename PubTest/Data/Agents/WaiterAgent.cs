using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public class WaiterAgent : BaseAgent
    {
        public override bool IsActive { get; set; }
        public override Action Behaviour { get; set; }
        public override BarStatus CurrentBar { get; set; }
    }
}
