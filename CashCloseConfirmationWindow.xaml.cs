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
            if(passwordTextBox.Password != password)
            {
                MessageBox.Show("La contraseña es incorrecta. Prueba de nuevo.", "Atención");
            }
            else
            {
                PrintDailyReport();
                Close();
            }

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private FlowDocument CreatePartialReport(List<Admission> admissions, int startIndex, int endIndex, int pageSize)
        {
            List<Admission> admissionsToPrint = admissions.GetRange(startIndex, endIndex-startIndex);
            double totalAmount = 0;
            int maxLineLength = 38;

            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Verdana");
            doc.FontSize = 13;

            // Create a Section  
            Section sec = new Section();

            Paragraph data = new Paragraph();

            // TITULO
            Paragraph businessName = new Paragraph();
            businessName.Inlines.Add(new Run(Model.Settings["BusinessName"] + "\n"));
            businessName.Inlines.Add(new Run(Model.Settings["BusinessCIF"]));
            sec.Blocks.Add(businessName);

            // DATOS
            data.Inlines.Add(new Run($"Fecha: {Model.GetTodayDateString()}\n"));
            data.Inlines.Add(new Run($"Hora de cierre de caja: {Model.GetNowHourString()}\n\n"));

            int totalTickets = (int) Math.Ceiling((decimal) (admissions.Count / pageSize)) + 1;
            int thisTicket = (int) startIndex / pageSize + 1;
            data.Inlines.Add(new Run($"Ticket {thisTicket} de {totalTickets} para este día\n"));
            data.Inlines.Add(new Run($"Mostrando niños del {(admissions.Count > 0 ? startIndex+1 : 0)} al {endIndex} (de {admissions.Count})\n\n"));
            data.Inlines.Add(new Run($"Hora de entrada, estancia en minutos, ingreso, nombre\n") { FontSize = 9 });

            // calculate total Amount
            foreach (Admission admission in admissions)
            {
                totalAmount += admission.Amount;
            }

            string admissionOut;
            foreach (Admission admission in admissionsToPrint)
            {
                DateTime startDate = Model.DateTimeFromStrings(admission.Date, admission.StartHour);
                DateTime endDate = Model.DateTimeFromStrings(admission.Date, admission.EndHour);
                TimeSpan duration = endDate.Subtract(startDate);
                int minutes = (int) Math.Floor(duration.TotalMinutes);
                admissionOut = $"{admission.StartHour} {minutes}m {admission.Amount}€ {admission.Name}\n";
                admissionOut = admissionOut.Length <= maxLineLength ? admissionOut : admissionOut.Substring(0, maxLineLength) + "\n";
                data.Inlines.Add(new Run(admissionOut) { FontSize = 12 });
            }

            data.Inlines.Add(new Run($"\nNúmero de niños: {admissions.Count}\n"));
            data.Inlines.Add(new Run($"Ingreso total (con IVA): {Math.Round(totalAmount, 2)}\n"));

            sec.Blocks.Add(data);


            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }

        private void PrintDailyReport()
        {
            List<Admission> admissions = Model.GetTodayAdmissions();
            int pageSize = 40;

            if(admissions.Count > 0)
            {

                for (int i = 0; i < admissions.Count; i += pageSize)
                {
                    int endIndex = i + 40 > admissions.Count ? admissions.Count : i + 40;
                    Model.PrintFlowDocument(CreatePartialReport(admissions, i, endIndex, pageSize));
                }
            }
            else
            {
                Model.PrintFlowDocument(CreatePartialReport(admissions, 0, 0, pageSize));
            }
        }
    }
}
