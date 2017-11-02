using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubTest
{
    public class WaiterAgent : BaseAgent
    {
        public override bool IsActive { get; set; }
        public override Action Behaviour { get; set; }
        public override BarStatus CurrentBar { get; set; }

        public override void Deactivate()
        {
            CurrentBar.AddWaiterAction("Waiter went home");
            base.Deactivate();
        }

        public WaiterAgent(BarStatus currentBar, int timeToGetGlasses, int timeToWashGlasses)
        {
            CurrentBar = currentBar;
            Behaviour = () => 
            {
                while (currentBar.GlassAvailableCount >= currentBar.TotalGlassCount)
                {
                    Thread.Sleep(400);
                }
                Thread.Sleep(timeToGetGlasses);
                currentBar.AddWaiterAction("Picked all the glasses from the tables");
                var amountOfGlasses = currentBar.TablesWithGlasses;
                Thread.Sleep(timeToWashGlasses);
                currentBar.AddWaiterAction("Washed all the glasses and put them on shelf");
                currentBar.GlassInUseCount -= amountOfGlasses;
                currentBar.TablesWithGlasses -= amountOfGlasses;
                if(currentBar.GuestsInBar == 0 && !currentBar.IsRunning && currentBar.GlassInUseCount == 0)
                {
                    Deactivate();
                }
            };
        }
    }

}
