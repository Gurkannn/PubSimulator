using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubTest
{
    public class BartenderAgent : BaseAgent
    {
        public static int TotalBartenders { get; set; }
        private int gettingGlassWaitTime;
        private int pouringDrinkWaitTime;
        private bool holdingGlass;

        public override bool IsActive { get; set; }
        public bool IsCustomerAvailable { get; set; } = false;
        public override Action Behaviour { get; set; }
        public override BarStatus CurrentBar { get; set; }


        public BartenderAgent(BarStatus currentBar, int getGlassWait, int pourDrinkWait)
        {
            TotalBartenders++;
            gettingGlassWaitTime = getGlassWait;
            pouringDrinkWaitTime = pourDrinkWait;           
            
            Behaviour = () =>
            {
                if (IsActive && currentBar.CanTakeOrder)
                {
                    if (!holdingGlass)
                    {
                        while (!currentBar.CanTakeGlass)
                        {
                            Thread.Sleep(100);
                        }
                        Thread.Sleep(currentBar.AdjustTimeToSimulationSpeed(gettingGlassWaitTime));
                        currentBar.TakeGlassFromShelf();
                        holdingGlass = true;
                        currentBar.AddBartenderAction("Took glass from shelf");
                    }
                    else
                    {
                        Thread.Sleep(currentBar.AdjustTimeToSimulationSpeed(pouringDrinkWaitTime));
                        var nameOfGuest = currentBar.ServeAndRemoveFirstGuestFromQueue();
                        holdingGlass = false;
                        currentBar.AddBartenderAction("Served drink to customer " + nameOfGuest);
                    }
                }
            };            
        }


    }
}
