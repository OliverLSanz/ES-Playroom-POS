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
            string password = Model.Settings["WorkerPassword"];
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
            double totalAmount = 0;
            int maxLineLength = 38;

            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            // Create a Section  
            Section sec = new Section();

            // TITULO
            Paragraph businessName = new Paragraph();
            businessName.Inlines.Add(new Run(Model.Settings["BusinessName"]));
            sec.Blocks.Add(businessName);

            // CIF
            Paragraph businessCif = new Paragraph();
            businessCif.Inlines.Add(new Run(Model.Settings["BusinessCIF"]));
            sec.Blocks.Add(businessCif);

            // DATOS
            Paragraph data = new Paragraph();
            data.Inlines.Add(new Run($"Fecha: {Model.GetTodayDateString()}\n"));
            data.Inlines.Add(new Run($"Hora de cierre de caja: {Model.GetNowHourString()}\n\n"));

            string admissionOut;
            foreach(Admission admission in admissions)
            {
                admissionOut = $"{admission.StartHour} {admission.Name}\n";
                admissionOut = admissionOut.Length <= maxLineLength ? admissionOut : admissionOut.Substring(0, maxLineLength) + "\n";
                data.Inlines.Add(new Run(admissionOut));
                totalAmount += admission.Amount;
            }
            data.Inlines.Add(new Run($"\nNúmero de niños: {admissions.Count}\n"));
            data.Inlines.Add(new Run($"Ingreso total (con IVA): {totalAmount}\n"));
            sec.Blocks.Add(data);


            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }
    }
}
