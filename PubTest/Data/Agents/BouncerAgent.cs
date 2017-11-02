using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubTest
{
    public class BouncerAgent : BaseAgent
    {        
        public static int totalBouncers { get; set; }
        public override bool IsActive { get; set; }
        public override Action Behaviour { get; set; }
        public override BarStatus CurrentBar { get; set; }

        public BouncerAgent(BarStatus currentBar, int minWaitPerGuest, int maxWaitPerGuest)
        {
            totalBouncers++;
            Random rand = new Random();
            //Behaviour = new BlockingCollection<Action>();
            Behaviour = () =>
            {
                while (currentBar.GuestsInBar >= 10)
                {
                    Thread.Sleep(200);
                }
                int waitTime = rand.Next(minWaitPerGuest, maxWaitPerGuest);
                Thread.Sleep(currentBar.AdjustTimeToSimulationSpeed(waitTime));
                var guest = currentBar.CreateGuest(NameGenerator.GetRandomName());
                currentBar.AddBouncerAction("Let guest in " + guest.Name);
            };
        }
    }
}
