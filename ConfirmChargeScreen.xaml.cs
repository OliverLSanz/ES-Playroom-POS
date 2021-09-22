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
        private Admission Admission { get; set; }
        private DateTime EndDate { get; set; }
        private DateTime StartDate { get; set; }
        private TimeSpan TimeElapsed { get; set; }
        private double Amount { get; set; }

        public ConfirmChargeScreen(Admission admission)
        {
            InitializeComponent();

            EndDate = DateTime.Now;
            StartDate = Model.DateTimeFromStrings(date: admission.Date, time: admission.StartHour);
            Admission = admission;
            TimeElapsed = EndDate.Subtract(StartDate);
            Amount = Model.GetAmountFromTimeSpan(TimeElapsed);

            hangerLabel.Content = admission.Hanger;
            nameLabel.Content = admission.Name;
            startHourLabel.Content = admission.StartHour;
            endHourLabel.Content = Model.GetHourStringFromDateTime(EndDate);
            startHourLabel.Content = Model.GetHourStringFromDateTime(StartDate);
            totalTimeLabel.Content = Model.GetStringFromTimeSpan(TimeElapsed);
            amountLabel.Content = Amount + "€";
        }

        private void ButtonCharge_Click(object sender, RoutedEventArgs e)
        {
            Model.CloseAdmission(Admission.Hanger, EndDate, Amount);
            Model.PopulateAdmissions();
            Model.PrintFlowDocument(CreateReceipt());
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private FlowDocument CreateReceipt()
        {
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
            data.Inlines.Add(new Run($"Hora de entrada: {startHourLabel.Content}\n"));
            data.Inlines.Add(new Run($"Hora de salida: {endHourLabel.Content}\n"));
            data.Inlines.Add(new Run($"Fecha: {Model.GetDateStringFromDateTime(StartDate)}\n"));
            data.Inlines.Add(new Run($"Ticket Número: {Admission.Id}\n"));
            data.Inlines.Add(new Run($"Neto: {Amount - Model.GetVAT(Amount)}€\n"));
            data.Inlines.Add(new Run($"IVA: {Model.GetVAT(Amount)}€\n"));
            data.Inlines.Add(new Run($"TOTAL: {Amount}€\n"));
            data.Inlines.Add(new Run($"Hasta Pronto y Gracias por su visita\n"));
            sec.Blocks.Add(data);


            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }
    }
}
