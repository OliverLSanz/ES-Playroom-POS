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
    /// Interaction logic for EndAdmissionForm.xaml
    /// </summary>
    public partial class EndAdmissionForm : Window
    {
        public EndAdmissionForm()
        {
            InitializeComponent();
            hangerTextBox.Focus();
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            Admission admission = null;
            int hanger;
            bool isNumber;

            isNumber = int.TryParse(hangerTextBox.Text, out hanger);

            if (!isNumber)
            {
                MessageBox.Show("Debes introducir un NÚMERO en el número de entrada.", "Atención");
            }
            else
            {
                foreach (Admission adm in Model.Admissions)
                {
                    if (adm.Hanger == hanger)
                    {
                        admission = adm;
                        break;
                    }
                }

                if(admission is null)
                {
                    MessageBox.Show("Ese número de entrada no corresponde a ningún niño.", "Atención");
                }
                else
                {
                    new ConfirmChargeScreen(admission).Show();
                    Close();
                }
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
