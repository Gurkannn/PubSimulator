using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubTest
{
    public class BarViewModel : BaseViewModel
    {

        public BarViewModel(BarStatus status)
        {
            totalGlassCount = status.TotalGlassCount;
            totalTableCount = status.TotalTableCount;
        }

        #region Private Members        

        private int totalGlassCount;
        private int totalTableCount;

        ObservableCollection<string> bartenderCompletedActions;
        ObservableCollection<string> waiterCompletedActions;
        ObservableCollection<string> guestCompletedActions;

        #endregion


        #region Public Properties

        public int TotalActions { get { return BartenderCompletedActions.Count; } }

        public ObservableCollection<string> BartenderCompletedActions
        {
            get
            {
                if (bartenderCompletedActions == null)
                    bartenderCompletedActions = new ObservableCollection<string>();
                return bartenderCompletedActions;
            }
            set
            {
                bartenderCompletedActions = value;
                OnPropertyChanged(nameof(BartenderCompletedActions));
            }
        }

        public ObservableCollection<string> WaiterCompletedActions
        {
            get
            {
                if (waiterCompletedActions == null)
                    waiterCompletedActions = new ObservableCollection<string>();
                return waiterCompletedActions;
            }
            set
            {
                waiterCompletedActions = value;
                OnPropertyChanged(nameof(WaiterCompletedActions));
            }
        }



        public ObservableCollection<string> GuestCompletedActions
        {
            get
            {
                if (guestCompletedActions == null)
                    guestCompletedActions = new ObservableCollection<string>();
                return guestCompletedActions;
            }
            set
            {
                guestCompletedActions = value;
                OnPropertyChanged(nameof(GuestCompletedActions));
            }
        }


        public bool CanTakeGlass { get { return GlassAvailableCount > 0; } }

        public int TotalGlassCount { get { return totalGlassCount; } }
        public int GlassAvailableCount { get { return totalGlassCount - GlassInUseCount; } }
        public int GlassInUseCount { get; set; }

        public bool CanTakeTable { get { return TableAvailableCount > 0; } }

        public int TotalTableCount { get { return totalTableCount; } }
        public int TableAvailableCount { get { return TotalTableCount - TableInUseCount; } }
        public int TableInUseCount { get; set; }

        public int CurrentGuestCount { get; set; }        

        #endregion


    }
}
