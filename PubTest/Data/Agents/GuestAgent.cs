using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubTest
{
    public class GuestAgent : BaseAgent
    {
        public static int TotalGuests;
        public string Name { get; set; }
        public override bool IsActive { get; set; }
        public bool GotDrink { get; set; } = false;
        public bool IsSitting { get; set; } = false;
        public override Action Behaviour { get; set; }
        public override BarStatus CurrentBar { get; set; }
        public override void Deactivate()
        {
            base.Deactivate();
            GotDrink = false;
            IsSitting = false;
        }
        public GuestAgent(BarStatus currentBar, int TravelTimeToBar, int TravelTimeToTable, string Name)
        {
            TotalGuests++;

            this.Name = Name;

            Random rand = new Random();
            int drinkTime = currentBar.AdjustTimeToSimulationSpeed(rand.Next(10000, 20000));

            currentBar.AddGuestAction(Name + " entered the pub");

            Behaviour = () =>
            {
                if (IsActive)
                {

                    if (!currentBar.IsGuestInQueue(this) && !GotDrink)
                    {
                        Thread.Sleep(currentBar.AdjustTimeToSimulationSpeed(TravelTimeToBar));
                        currentBar.AddGuestToQueue(this);
                        currentBar.AddGuestAction(Name + " walked up to bar");
                    }
                    while (!GotDrink)
                    {
                        Thread.Sleep(200);
                    }
                    if (GotDrink && !IsSitting)
                    {
                        currentBar.GuestsInTableQueue++;
                        while (!currentBar.CanTakeTable)
                        {
                            Thread.Sleep(200);
                        }
                        currentBar.TakeTable();
                        currentBar.GuestsInTableQueue--;
                        Thread.Sleep(currentBar.AdjustTimeToSimulationSpeed(TravelTimeToTable));
                        IsSitting = true;
                        currentBar.AddGuestAction(Name + " sat down at table");
                    }
                    if (GotDrink && IsSitting)
                    {
                        Thread.Sleep(drinkTime);
                        currentBar.AddGuestAction(Name + " Finished drink and left bar");
                        currentBar.TablesWithGlasses++;
                        currentBar.ReturnTable();
                        Deactivate();
                    }
                }
            };
        }
    }
}
