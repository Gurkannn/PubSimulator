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


        //BarViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            BarStatus bs = new BarStatus(10, 10, 120);
            StatusPanel.DataContext = bs;

            //Not MVVM
            bs.RegisterBartenderActionCallback((sender, e) =>
            {
                Dispatcher.Invoke(() => { BartenderActionList.Items.Insert(0, e.ActionValue); });
            });
            bs.RegisterWaiterActionCallback((sender, e) =>
            {
                Dispatcher.Invoke(() => { WaiterActionList.Items.Insert(0, e.ActionValue); });
            });
            bs.RegisterGuestActionCallback((sender, e) =>
            {
                Dispatcher.Invoke(() => { GuestActionList.Items.Insert(0, e.ActionValue); });
            });
        }
    }
}