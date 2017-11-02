using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PubTest
{
    public class BarStatus : BaseViewModel
    {
        #region Constructor

        public BarStatus(int TotalGlasses, int TotalTables, int SimulationDurationInSeconds)
        {
            time = new Time();
            SimulationTimeLeft = SimulationDurationInSeconds;
            AgentCancellationToken = new CancellationToken(IsRunning);
            RegisterGuestCreatedCallback((sender, e) => { time.CreateAndStartAgent(e.agent, AgentCancellationToken); });

            TotalGlassCount = TotalGlasses;
            TotalTableCount = TotalTables;

            BartenderCompletedActions = new BlockingCollection<string>();
            WaiterCompletedActions = new BlockingCollection<string>();
            BouncerCompletedActions = new BlockingCollection<string>();
            GuestCompletedActions = new BlockingCollection<string>();

            BarQueue = new ConcurrentQueue<GuestAgent>();
            StartSimulation(SimulationDurationInSeconds);
        }

        #endregion

        public void StartSimulation(int SimulationTime)
        {
            startTime = DateTime.Now + TimeSpan.FromSeconds(SimulationTime);
            startTime.AddSeconds(SimulationTime);
            IsRunning = true;
            Task.Factory.StartNew(() =>
            {
                var t = new System.Timers.Timer()
                {
                    Interval = 1000,
                    Enabled = true,
                };
                t.Elapsed += (sender, e) => { SimulationCountdown(); };
                SimulationCountdown();
            });
            CreateBartenderAgent();
            CreateBouncerAgent();
        }

        private void SimulationCountdown()
        {
            SimulationTimeLeft = (startTime - DateTime.Now).Seconds;
        }

        public void StopSimulation()
        {
            IsRunning = false;
            bartender.Deactivate();
            waiter.Deactivate();
            bouncer.Deactivate();
        }

        #region Private Members

        private DateTime startTime { get; set; }

        private Time time;
        private CancellationToken AgentCancellationToken;
        private bool IsRunning { get; set; } = false;

        private BartenderAgent bartender;
        private WaiterAgent waiter;
        private BouncerAgent bouncer;

        #endregion


        #region Public Properties

        public int SimulationTimeLeft { get; private set; }

        public int TotalActions { get { return (BartenderCompletedActions.Count + WaiterCompletedActions.Count + GuestCompletedActions.Count); } }

        #region CompletedActionLists       

        #region Observable Lists
        public ObservableCollection<string> BartenderActions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> WaiterActions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> BouncerActions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> GuestActions { get; set; } = new ObservableCollection<string>();
        #endregion

        #region Thread-safe lists
        public BlockingCollection<string> BartenderCompletedActions { get; private set; }
        public BlockingCollection<string> WaiterCompletedActions { get; private set; }
        public BlockingCollection<string> BouncerCompletedActions { get; private set; }
        public BlockingCollection<string> GuestCompletedActions { get; private set; }
        #endregion

        #region ActionText Events

        public void AddBartenderAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            BartenderCompletedActions.Add(actionText);
            BartenderActions.Insert(0, actionText);
            OnPropertyChanged("BartenderActions");
            OnBartenderActionEventTrigger(new ActionEventArgs(actionText));
        }
        public void AddWaiterAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            WaiterCompletedActions.Add(actionText);
            WaiterActions.Insert(0, actionText);
            OnPropertyChanged("WaiterActions");
            OnWaiterActionEventTrigger(new ActionEventArgs(actionText));
        }
        public void AddBouncerAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            BouncerCompletedActions.Add(actionText);
            BouncerActions.Insert(0, actionText);
            OnPropertyChanged("BouncerActions");
            OnBouncerActionEventTrigger(new ActionEventArgs(actionText));
        }
        public void AddGuestAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            GuestCompletedActions.Add(actionText);
            GuestActions.Insert(0, actionText);
            OnPropertyChanged("GuestActions");
            OnGuestActionEventTrigger(new ActionEventArgs(actionText));
        }

        public delegate void ActionEventHandler(object sender, ActionEventArgs e);

        public event ActionEventHandler OnBartenderActionCompleted;
        public event ActionEventHandler OnWaiterActionCompleted;
        public event ActionEventHandler OnBouncerActionCompleted;
        public event ActionEventHandler OnGuestActionCompleted;

        #region Event triggers

        protected virtual void OnBartenderActionEventTrigger(ActionEventArgs args)
        {
            OnBartenderActionCompleted?.Invoke(this, args);
        }
        protected virtual void OnWaiterActionEventTrigger(ActionEventArgs args)
        {
            OnWaiterActionCompleted?.Invoke(this, args);
        }
        protected virtual void OnBouncerActionEventTrigger(ActionEventArgs args)
        {
            OnBouncerActionCompleted?.Invoke(this, args);
        }
        protected virtual void OnGuestActionEventTrigger(ActionEventArgs args)
        {
            OnGuestActionCompleted?.Invoke(this, args);
        }


        #endregion

        public void RegisterBartenderActionCallback(ActionEventHandler del)
        {
            OnBartenderActionCompleted += del;
        }
        public void RegisterWaiterActionCallback(ActionEventHandler del)
        {
            OnWaiterActionCompleted += del;
        }
        public void RegisterBouncerActionCallback(ActionEventHandler del)
        {
            OnBouncerActionCompleted += del;
        }
        public void RegisterGuestActionCallback(ActionEventHandler del)
        {
            OnGuestActionCompleted += del;
        }

        #endregion

        #endregion

        #region SimulationSpeed

        private float TimeMultiplier { get; } = 1.0f;
        public int AdjustTimeToSimulationSpeed(int time)
        {
            return (int)(time * TimeMultiplier);
        }

        #endregion

        #region Bartender

        public bool CanTakeOrder { get { return BarQueue.Count > 0; } }

        public bool CanTakeGlass { get { return GlassAvailableCount > 0; } }
        public int TotalGlassCount { get; private set; }
        public int GlassAvailableCount { get { return TotalGlassCount - GlassInUseCount; } }
        public int GlassInUseCount { get; set; }

        public bool IsDrinkReady { get; set; }

        #endregion

        #region Bouncer

        public delegate void GuestCreatedEventHandler(object sender, GuestEventArgs e);
        public event GuestCreatedEventHandler OnGuestCreated;

        public void RegisterGuestCreatedCallback(GuestCreatedEventHandler del)
        {
            OnGuestCreated += del;
        }

        public GuestAgent CreateGuest(string name)
        {
            var newGuest = new GuestAgent(this, 1000, 4000) { Name = name, IsActive = true };
            var eventArgs = new GuestEventArgs(newGuest);
            OnGuestCreated.Invoke(this, eventArgs);
            return newGuest;
        }

        #endregion

        #region Guest

        ConcurrentQueue<GuestAgent> BarQueue { get; set; }
        public GuestAgent FirstInQueue { get { while (!CanTakeOrder) { Thread.Sleep(200); } return BarQueue.First(); } }
        public bool IsGuestInQueue(GuestAgent guest) { return BarQueue.Contains(guest); }

        public void AddGuestToQueue(GuestAgent guest) { BarQueue.Enqueue(guest); }

        public string ServeAndRemoveFirstGuestFromQueue()
        {
            BarQueue.TryDequeue(out GuestAgent result);
            result.GotDrink = true;
            return result.Name;
        }

        public int GuestsInBar { get { return GuestAgent.TotalGuests; } }
        public bool CanTakeTable { get { return TableAvailableCount > 0; } }

        public int TotalTableCount { get; private set; }
        public int TableAvailableCount { get { return TotalTableCount - TableInUseCount; } }
        public int TableInUseCount { get; set; }

        #endregion

        #endregion

        #region Public Methods

        #region Agent Creation

        public void CreateBartenderAgent()
        {
            bartender = new BartenderAgent(this, 3000, 3000)
            { IsActive = true };
            time.CreateAndStartAgent(bartender, AgentCancellationToken);
        }

        public void CreateBouncerAgent()
        {
            bouncer = new BouncerAgent(this, 4000, 10000)
            { IsActive = true };
            time.CreateAndStartAgent(bouncer, AgentCancellationToken);
        }

        #endregion

        public void TakeGlassFromShelf()
        {
            GlassInUseCount++;
        }
        public void PutGlassOnShelf()
        {
            GlassInUseCount--;
        }
        public void TakeTable()
        {
            TableInUseCount++;
        }
        public void ReturnTable()
        {
            TableInUseCount--;
        }

        #endregion
    }
}
