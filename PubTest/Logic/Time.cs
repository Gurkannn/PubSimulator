using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace PubTest
{
    public class Time
    {
        BlockingCollection<BaseAgent> activeAgents;
        BlockingCollection<BaseAgent> ActiveAgents
        {
            get { return activeAgents; }
            set { activeAgents = value; }
        }

        private void AddAgent(BaseAgent agent) { ActiveAgents.Add(agent); }

        #region Constructor

        public Time()
        {
            ActiveAgents = new BlockingCollection<BaseAgent>();
        }

        #endregion

        public void StartAgent(BaseAgent agent, CancellationToken Cancel)
        {
            Task.Factory.StartNew(() =>
            {      
                ExecuteBehaviour(agent, Cancel);
            });
            AddAgent(agent);
        }

        private async void ExecuteBehaviour(BaseAgent b, CancellationToken token)
        {
            while (b.IsActive)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine(b.GetType() + " Thread Cancelled");
                    token.ThrowIfCancellationRequested();
                    b.Deactivate();
                }
                b.Behaviour();
                await Task.Delay(1000);
            }
        }
    }
}
