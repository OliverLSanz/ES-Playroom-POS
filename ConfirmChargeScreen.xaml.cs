﻿using System;
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
        private double BDayAmount { get; set; }


        public ConfirmChargeScreen(Admission admission)
        {
            InitializeComponent();

            EndDate = DateTime.Now;
            StartDate = Model.DateTimeFromStrings(date: admission.Date, time: admission.StartHour);
            Admission = admission;
            TimeElapsed = EndDate.Subtract(StartDate);
            Amount = Model.GetAmountFromTimeSpan(TimeElapsed);
            BDayAmount = Model.GetAmountFromTimeSpan(TimeElapsed, isBDay: true);

            hangerLabel.Content = admission.Hanger;
            nameLabel.Content = admission.Name;
            startHourLabel.Content = admission.StartHour;
            endHourLabel.Content = Model.GetHourStringFromDateTime(EndDate);
            startHourLabel.Content = Model.GetHourStringFromDateTime(StartDate);
            totalTimeLabel.Content = Model.GetStringFromTimeSpan(TimeElapsed);
            amountLabel.Content = Amount + "€";
            amountBDayLabel.Content = BDayAmount + "€";
        }

        private void ButtonCharge_Click(object sender, RoutedEventArgs e)
        {
            Model.CloseAdmission(Admission.Hanger, EndDate, Amount);
            Model.PopulateAdmissions();
            if(Amount > 0)
            {
                Model.PrintFlowDocument(CreateExitReceipt());
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private FlowDocument CreateExitReceipt(bool isBDay = false)
        {
            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Verdana");
            doc.FontSize = 13;

            // Create a Section  
            Section sec = new Section();

            // TITULO
            Paragraph businessName = new Paragraph();
            businessName.Inlines.Add(new Run(Model.CompatibleString(Model.Settings["BusinessName"] + '\n')) { FontSize = 20, FontWeight = FontWeights.Bold });
            businessName.Inlines.Add(new Run(Model.CompatibleString(Model.Settings["BusinessCIF"])));
            sec.Blocks.Add(businessName);

            double amount = isBDay ? BDayAmount : Amount;

            // DATOS
            Paragraph data = new Paragraph();
            data.Inlines.Add(new Run(Model.CompatibleString($"Fecha: {Model.GetDateStringFromDateTime(StartDate)}\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Hora de entrada: {startHourLabel.Content}\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Hora de salida: {endHourLabel.Content}\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Número de ticket: {Admission.Id}\n\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Neto: {Math.Round(amount - Model.GetVAT(amount), 2)}€\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"IVA: {Model.GetVAT(amount)}€\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"TOTAL: {amount}€\n\n")) { FontSize = 15, FontWeight = FontWeights.Bold });
            data.Inlines.Add(new Run(Model.CompatibleString($"Hasta Pronto y Gracias por su visita\n")));
            sec.Blocks.Add(data);


            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }

        private void ButtonBDayCharge_Click(object sender, RoutedEventArgs e)
        {
            Model.CloseAdmission(Admission.Hanger, EndDate, BDayAmount);
            Model.PopulateAdmissions();
            if (BDayAmount > 0)
            {
                Model.PrintFlowDocument(CreateExitReceipt(isBDay: true));
            }
            Close();
        }
    }
}
