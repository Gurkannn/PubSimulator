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
    public class BarStatus
    {

        //public RelayCommand StartSimulationAction;
        public BarViewModel.StartSimulationButtonEventHandler StartSimulationEventHandler;

        #region Constructor

        public BarStatus(int TotalGlasses, int TotalTables, int SimulationDurationInSeconds)
        {
            SimulationDuration = SimulationDurationInSeconds;
            StartSimulationEventHandler = (sender, e) => { Dispatcher.CurrentDispatcher.Invoke(() => { StartSimulation(SimulationDuration); }); };
            //StartSimulationAction = new RelayCommand(() =>
            //{
            //    Dispatcher.CurrentDispatcher.Invoke(() => { StartSimulation(SimulationDuration); });
            //});

            time = new Time();
            AgentCancellationToken = new CancellationToken(IsRunning);
            RegisterGuestCreatedCallback((sender, e) => { time.StartAgent(e.agent, new CancellationToken(!e.agent.IsActive)); });

            TotalGlassCount = TotalGlasses;
            TotalTableCount = TotalTables;

            BartenderCompletedActions = new BlockingCollection<string>();
            WaiterCompletedActions = new BlockingCollection<string>();
            BouncerCompletedActions = new BlockingCollection<string>();
            GuestCompletedActions = new BlockingCollection<string>();

            BarQueue = new ConcurrentQueue<GuestAgent>();


            //StartSimulation(SimulationDurationInSeconds);
        }

        #endregion

        #region Simulation 

        public void StartSimulation(int SimulationTime)
        {
            SimulationTimeLeft = SimulationDuration;
            IsRunning = true;

            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            timer.Tick += (sender, e) => { SimulationCountdown(); if (!IsRunning) timer.Stop(); };

            CreateBartenderAgent();
            CreateBouncerAgent();
            CreateWaiterAgent();
        }

        //Only stop the bouncer here to stop guest flow and the other agents will stop when work is done
        public void StopSimulation()
        {
            IsRunning = false;
            bouncer.Deactivate();
        }

        private void SimulationCountdown()
        {
            SimulationTimeLeft--;
            if (SimulationTimeLeft <= 0)
                StopSimulation();
        }

        #region SimulationSpeed

        private float TimeMultiplier { get; } = 1f;
        public int AdjustTimeToSimulationSpeed(int time)
        {
            return (int)(time * TimeMultiplier);
        }

        #endregion

        #endregion

        #region Private Members

        private int SimulationDuration;

        private int guestsInBarQueue;
        private int guestsInTableQueue;
        private int glassInUse;
        private int tablesInUse;
        private Time time;
        private CancellationToken AgentCancellationToken;

        private BartenderAgent bartender;
        private WaiterAgent waiter;
        private BouncerAgent bouncer;

        #endregion


        #region Public Properties

        public bool IsRunning { get; private set; } = false;

        public int SimulationTimeLeft { get; private set; }

        public int TotalActions { get { return (BartenderCompletedActions.Count + WaiterCompletedActions.Count + GuestCompletedActions.Count); } }

        #region CompletedActionLists       



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
            OnBartenderActionEventTrigger(new ActionEventArgs(actionText));

        }

        public void AddWaiterAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            WaiterCompletedActions.Add(actionText);
            OnWaiterActionEventTrigger(new ActionEventArgs(actionText));
        }

        public void AddBouncerAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            BouncerCompletedActions.Add(actionText);
            OnBouncerActionEventTrigger(new ActionEventArgs(actionText));
        }

        public void AddGuestAction(string actionText)
        {
            actionText = TotalActions + ": " + actionText;
            GuestCompletedActions.Add(actionText);
            OnGuestActionEventTrigger(new ActionEventArgs(actionText));
        }

        public delegate void ActionEventHandler(object sender, ActionEventArgs e);
        public delegate void StatusEventHandler(object sender, StatusEventArgs e);

        public event StatusEventHandler OnStatusChanged;

        public event ActionEventHandler OnBartenderActionCompleted;
        public event ActionEventHandler OnWaiterActionCompleted;
        public event ActionEventHandler OnBouncerActionCompleted;
        public event ActionEventHandler OnGuestActionCompleted;

        #region Event triggers

        protected virtual void OnStatusChangedTrigger()
        {
            StatusEventArgs args = new StatusEventArgs(GuestsInBarQueue, GuestsInTableQueue, GlassInUseCount, TableInUseCount);
            OnStatusChanged?.Invoke(this, args);
        }

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

        #region CallbackRegister Methods

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

        #endregion


        #region Bartender

        public bool CanTakeOrder { get { return BarQueue.Count > 0; } }

        public bool CanTakeGlass { get { return GlassAvailableCount > 0; } }
        public int TotalGlassCount { get; private set; }
        public int GlassAvailableCount { get { return TotalGlassCount - GlassInUseCount; } }
        public int GlassInUseCount { get { return glassInUse; } set { glassInUse = value; OnStatusChangedTrigger(); } }
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
            var newGuest = new GuestAgent(this, 1000, 4000, name) { IsActive = true };
            var eventArgs = new GuestEventArgs(newGuest);
            OnGuestCreated.Invoke(this, eventArgs);
            return newGuest;
        }

        #endregion

        #region Guest

        ConcurrentQueue<GuestAgent> BarQueue { get; set; }
        public GuestAgent FirstInQueue { get { while (!CanTakeOrder) { Thread.Sleep(200); } return BarQueue.First(); } }
        public bool IsGuestInQueue(GuestAgent guest) { return BarQueue.Contains(guest); }

        public void AddGuestToQueue(GuestAgent guest) { BarQueue.Enqueue(guest); GuestsInBarQueue++; }

        public string ServeAndRemoveFirstGuestFromQueue()
        {
            BarQueue.TryDequeue(out GuestAgent guest);
            guest.GotDrink = true;
            GuestsInBarQueue--;
            return guest.Name;
        }

        public int GuestsInBarQueue { get { return guestsInBarQueue; } set { guestsInBarQueue = value; OnStatusChangedTrigger(); } }
        public int GuestsInTableQueue { get { return guestsInTableQueue; } set { guestsInTableQueue = value; OnStatusChangedTrigger(); } }
        public bool CanTakeTable { get { return TableAvailableCount > 0; } }

        public int TablesWithGlasses { get; set; }
        public int TotalTableCount { get; private set; }
        public int TableAvailableCount { get { return TotalTableCount - TableInUseCount; } }
        public int TableInUseCount { get { return tablesInUse; } set { tablesInUse = value; OnStatusChangedTrigger(); } }

        #endregion

        #endregion

        #region Public Methods

        #region Agent Creation

        public void CreateBartenderAgent()
        {
            bartender = new BartenderAgent(this, 3000, 3000)
            { IsActive = true };

            time.StartAgent(bartender, AgentCancellationToken);
        }

        public void CreateBouncerAgent()
        {
            bouncer = new BouncerAgent(this, 4000, 10000)
            { IsActive = true };

            time.StartAgent(bouncer, AgentCancellationToken);
        }

        public void CreateWaiterAgent()
        {
            waiter = new WaiterAgent(this, 10000, 15000)
            { IsActive = true };

            time.StartAgent(waiter, AgentCancellationToken);
        }

        #endregion

        public void TakeGlassFromShelf()
        {
            GlassInUseCount++;
        }
        public void PutGlassOnShelf()
        {
            if (GlassAvailableCount != TotalGlassCount)
                GlassInUseCount--;
        }
        public void TakeTable()
        {
            TableInUseCount++;
        }
        public void ReturnTable()
        {
            if (TableAvailableCount != TotalTableCount)
                TableInUseCount--;
        }

        #endregion
    }
}
