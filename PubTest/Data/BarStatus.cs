using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PubTest
{
    public class BarStatus : BaseViewModel
    {

        #region ICommands

        public ICommand StartSimulationCommand { get; set; }
        public bool CanStartSimulation { get { return !IsRunning; } }

        #endregion

        #region Constructor

        public BarStatus(int TotalGlasses, int TotalTables, int SimulationDurationInSeconds)
        {
            SimulationDuration = SimulationDurationInSeconds;
            StartSimulationCommand = new RelayCommand(() =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() => { StartSimulation(SimulationDuration); });
            });

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

        private float TimeMultiplier { get; } = 0.5f;
        public int AdjustTimeToSimulationSpeed(int time)
        {
            return (int)(time * TimeMultiplier);
        }

        #endregion

        #endregion

        #region Private Members

        private int SimulationDuration;

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

        #region Observable Lists
        private ObservableCollection<string> bartenderActions = new ObservableCollection<string>();
        public ObservableCollection<string> BartenderActions { get { return bartenderActions; } set { bartenderActions = value; OnPropertyChanged("BartenderActions"); } }

        private ObservableCollection<string> waiterActions = new ObservableCollection<string>();
        public ObservableCollection<string> WaiterActions
        {
            get { return waiterActions; }
            set
            {
                waiterActions = value; OnPropertyChanged("WaiterActions");
            }
        }

        private ObservableCollection<string> bouncerActions = new ObservableCollection<string>();
        public ObservableCollection<string> BouncerActions { get { return bouncerActions; } set { bouncerActions = value; OnPropertyChanged("BouncerActions"); } }

        private ObservableCollection<string> guestActions = new ObservableCollection<string>();
        public ObservableCollection<string> GuestActions { get { return guestActions; } set { guestActions = value; OnPropertyChanged("GuestActions"); } }


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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                actionText = TotalActions + ": " + actionText;
                BartenderCompletedActions.Add(actionText);
                BartenderActions.Insert(0, actionText);
                OnPropertyChanged("BartenderActions");
                OnBartenderActionEventTrigger(new ActionEventArgs(actionText));
            }));
        }

        public void AddWaiterAction(string actionText)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                actionText = TotalActions + ": " + actionText;
                WaiterCompletedActions.Add(actionText);
                WaiterActions.Insert(0, actionText);
                OnPropertyChanged("WaiterActions");
                OnWaiterActionEventTrigger(new ActionEventArgs(actionText));
            }));
        }

        public void AddBouncerAction(string actionText)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                actionText = TotalActions + ": " + actionText;
                BouncerCompletedActions.Add(actionText);
                BouncerActions.Insert(0, actionText);
                OnPropertyChanged("BouncerActions");
                OnBouncerActionEventTrigger(new ActionEventArgs(actionText));
            }));
        }

        public void AddGuestAction(string actionText)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                actionText = TotalActions + ": " + actionText;
                GuestCompletedActions.Add(actionText);
                GuestActions.Insert(0, actionText);
                OnPropertyChanged("GuestActions");
                OnGuestActionEventTrigger(new ActionEventArgs(actionText));
            }));
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

        public int GuestsInBarQueue { get; set; }
        public int GuestsInTableQueue { get; set; }
        public bool CanTakeTable { get { return TableAvailableCount > 0; } }

        public int TablesWithGlasses { get; set; }
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
