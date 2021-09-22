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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CashCloseConfirmationWindow : Window
    {
        public CashCloseConfirmationWindow()
        {
            InitializeComponent();
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            string password = "1234";
            if(passwordTextBox.Text != password)
            {
                MessageBox.Show("La contraseña es incorrecta. Prueba de nuevo.", "Atención");
            }
            else
            {
                Model.PrintFlowDocument(CreateDailyReport());
                Close();
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private FlowDocument CreateDailyReport()
        {
            List<Admission> admissions = Model.GetTodayAdmissions();

            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            // Create a Section  
            Section sec = new Section();

            // TITULO
            Paragraph businessName = new Paragraph();
            businessName.Inlines.Add(new Run("Ludoteca El Rosal"));
            sec.Blocks.Add(businessName);

            // CIF
            Paragraph businessCif = new Paragraph();
            businessCif.Inlines.Add(new Run("1234567G"));
            sec.Blocks.Add(businessCif);

            // DATOS
            Paragraph data = new Paragraph();
            data.Inlines.Add(new Run($"Fecha: {Model.GetTodayDateString()}\n"));
            data.Inlines.Add(new Run($"Hora de cierre de caja: {Model.GetNowHourString()}\n"));
            foreach(Admission admission in admissions)
            {
                data.Inlines.Add(new Run($"{admission.StartHour} {admission.Name}\n"));
            }
            sec.Blocks.Add(data);


            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }
    }
}
