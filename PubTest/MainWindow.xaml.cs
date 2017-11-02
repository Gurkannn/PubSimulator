using System;
using System.Threading;
using System.Threading.Tasks;
//using System.Timers;
using System.Windows;

namespace PubTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields
        private int glassAmount = 15;
        private int tableAmount = 15;
        private int simualtionDuration = 120;
        #endregion

        //BarViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            BarStatus bStatus = new BarStatus(glassAmount, tableAmount, simualtionDuration);
            BarViewModel bViewModel = new BarViewModel(glassAmount, tableAmount, simualtionDuration);
            bViewModel.OnStartSimulationPressed += bStatus.StartSimulationEventHandler;

            bStatus.OnStatusChanged += bViewModel.UpdateStatus;            

            bStatus.RegisterBartenderActionCallback(bViewModel.UpdateBartenderList);
            bStatus.RegisterWaiterActionCallback(bViewModel.UpdateWaiterList);
            bStatus.RegisterGuestActionCallback(bViewModel.UpdateGuestList);

            this.DataContext = bViewModel;

            #region comment
            //Not MVVM
            //bs.RegisterBartenderActionCallback((sender, e) =>
            //{
            //    Dispatcher.Invoke(() => { BartenderActionList.Items.Insert(0, e.ActionValue); });
            //});

            //bs.RegisterWaiterActionCallback((sender, e) =>
            //{
            //    Dispatcher.Invoke(() => { WaiterActionList.Items.Insert(0, e.ActionValue); });
            //});

            //bs.RegisterGuestActionCallback((sender, e) =>
            //{
            //    Dispatcher.Invoke(() => { GuestActionList.Items.Insert(0, e.ActionValue); });
            //});
            #endregion

        }
    }
}