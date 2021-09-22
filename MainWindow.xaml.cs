using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Printing;
using System.Diagnostics;
using System.IO;

namespace Playroom_Kiosk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            DataGrid.ItemsSource = Model.Admissions;
            Model.InitDB();
            Model.PopulateAdmissions();
            Model.LoadSettings();
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonAddAdmission_Click(object sender, RoutedEventArgs e)
        {
            new AddAdmissionForm().Show();
        }

        private void ButtonEndAdmission_Click(object sender, RoutedEventArgs e)
        {
            new EndAdmissionForm().Show();
        }

        private void CashClosing_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.IsPlayRoomEmpty())
            {
                MessageBox.Show("No puede cerrarse la caja hasta que hayan salido todos los niños.", "Atención");
            }
            else
            {
                new CashCloseConfirmationWindow().Show();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonDebug_Click(object sender, RoutedEventArgs e)
        {
            Model.TestDatabase();
        }
    }
}
