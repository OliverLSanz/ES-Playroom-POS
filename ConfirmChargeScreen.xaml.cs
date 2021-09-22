using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Playroom_Kiosk
{
    /// <summary>
    /// Interaction logic for ConfirmChargeScreen.xaml
    /// </summary>
    public partial class ConfirmChargeScreen : Window
    {
        private Admission Admission;
        private DateTime EndDate;
        private DateTime StartDate;
        private TimeSpan TimeElapsed;

        public ConfirmChargeScreen(Admission admission)
        {
            InitializeComponent();

            EndDate = DateTime.Now;
            StartDate = Model.DateTimeFromStrings(date: admission.Date, time: admission.StartHour);
            Admission = admission;
            TimeElapsed = EndDate.Subtract(StartDate);

            hangerLabel.Content = admission.Hanger;
            nameLabel.Content = admission.Name;
            startHourLabel.Content = admission.StartHour;
            endHourLabel.Content = Model.GetHourStringFromDateTime(EndDate);
            startHourLabel.Content = Model.GetHourStringFromDateTime(StartDate);
            totalTimeLabel.Content = Model.GetStringFromTimeSpan(TimeElapsed);
            amountLabel.Content = Model.GetAmountFromTimeSpan(TimeElapsed) + "€";
        }

        private void ButtonCharge_Click(object sender, RoutedEventArgs e)
        {
            Model.CloseAdmission(Admission.Hanger);
            Model.PopulateAdmissions();
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
