using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public abstract class BaseAgent
    {
        public abstract bool IsActive { get; set; }
        public abstract BarStatus CurrentBar { get; set; }
        public virtual void SetActive() { IsActive = true; }
        public virtual void Deactivate() { IsActive = false; }
        public abstract Action Behaviour { get; set; }
    }
}
