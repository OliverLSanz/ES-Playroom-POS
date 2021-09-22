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
    /// Interaction logic for AddAdmissionForm.xaml
    /// </summary>
    public partial class AddAdmissionForm : Window
    {
        public AddAdmissionForm()
        {
            InitializeComponent();
        }


        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            int hanger;
            bool isNumeric, isHangerFree = true;

            isNumeric = int.TryParse(hangerTextBox.Text, out hanger);

            foreach(Admission admission in Model.Admissions)
            {
                if(admission.Hanger == hanger)
                {
                    isHangerFree = false;
                    break;
                }
            }

            if (!isNumeric)
            {
                MessageBox.Show("Por favor, introduce un NÚMERO en el número de percha.", "Atención");
            }
            else if(string.IsNullOrEmpty(nameTextBox.Text))
            {
                MessageBox.Show("Por favor, introduce el nombre del niño.", "Atención");
            }
            else if (!isHangerFree)
            {
                MessageBox.Show("Ese número está ocupado, introduce otro.", "Atención");
            }
            else
            {
                Model.AddNewAdmission(hanger: int.Parse(hangerTextBox.Text), name: nameTextBox.Text);
                Model.PopulateAdmissions();
            }
            Close();
        }
    }
}
