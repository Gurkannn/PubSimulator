using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PubTest
{
    public class BarViewModel : BaseViewModel
    {

        #region ICommands

        public ICommand StartSimulationCommand { get; set; }
        public ICommand StartCountdownCommand { get; set; }
        public ICommand StartViewCountdownCommand { get; set; }

        #endregion

        #region ButtonEvents

        public delegate void StartSimulationButtonEventHandler(object sender, EventArgs e);
        public event StartSimulationButtonEventHandler OnStartSimulationPressed;

        protected virtual void OnStartSimulation()
        {
            OnStartSimulationPressed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Delegates

        public BarStatus.ActionEventHandler UpdateBartenderList;
        public BarStatus.ActionEventHandler UpdateWaiterList;
        public BarStatus.ActionEventHandler UpdateGuestList;

        public BarStatus.StatusEventHandler UpdateStatus;

        #endregion

        #region Constructor

        public BarViewModel(int totalGlassCount, int totalTableCount, int simulationDuration)
        {
            #region Command initialization

            StartSimulationCommand = new RelayCommand(() =>
            {
                StartCountdown();
                OnStartSimulation();
            });

            #endregion

            #region Action initialization

            UpdateBartenderList = (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BartenderActions.Insert(0, e.ActionValue);
                });
            };

            UpdateWaiterList = (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    WaiterActions.Insert(0, e.ActionValue);
                });
            };

            UpdateGuestList = (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GuestActions.Insert(0, e.ActionValue);
                });
            };

            UpdateStatus = (sender, e) => { GlassInUseCount = e.GlassInUseCount; TableInUseCount = e.TableInUseCount; GuestsInBarQueue = e.GuestsInBarQueue; GuestsInTableQueue = e.GuestsInTableQueue; };

            #endregion

            totalDuration = simulationDuration;
            this.totalGlassCount = totalGlassCount;
            this.totalTableCount = totalTableCount;
        }

        #endregion

        #region Timer

        private void StartCountdown()
        {
            TimeLeft = totalDuration;
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
                IsEnabled = true
            };
            timer.Tick += (sender, e) => { SimulationCountdown(); if (!IsRunning) timer.Stop(); };
        }

        private void SimulationCountdown()
        {
            if (TimeLeft > 0)
                TimeLeft--;
        }

        #endregion

        #region Private Members    
        private int totalDuration;    
    
        private bool IsRunning { get { return TimeLeft > 0; } }

        private int totalGlassCount;
        private int totalTableCount;

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

        #endregion


        #region Public Properties

        public bool CanStartSimulation { get { return !IsRunning; } }

        public int TimeLeft { get; set; } = 0;

        public int TotalGlassCount { get { return totalGlassCount; } }
        public int GlassAvailableCount { get { return TotalGlassCount - GlassInUseCount; } }
        public int GlassInUseCount { get; set; }

        public bool CanTakeTable { get { return TableAvailableCount > 0; } }

        public int TotalTableCount { get { return totalTableCount; } }
        public int TableAvailableCount { get { return TotalTableCount - TableInUseCount; } }
        public int TableInUseCount { get; set; }

        public int CurrentGuestCount { get; set; }
        public int GuestsInBarQueue { get; private set; }
        public int GuestsInTableQueue { get; private set; }

        #endregion


    }
}
