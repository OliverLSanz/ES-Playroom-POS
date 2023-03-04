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
using System.Windows.Shapes;

namespace Playroom_Kiosk
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public ObservableCollection<DirectSaleItem> DirectSaleItems { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            businessNameTextBox.Text = Model.Settings["BusinessName"];
            businessCIFTextBox.Text = Model.Settings["BusinessCIF"];
            fee30TextBox.Text = Model.Settings["LessThan30MinutesFee"];
            fee60TextBox.Text = Model.Settings["LessThan60MinutesFee"];
            feeExtra15TextBox.Text = Model.Settings["Extra15MinutesFee"];
            fee30BDayTextBox.Text = Model.Settings["LessThan30MinutesBDayFee"];
            fee60BDayTextBox.Text = Model.Settings["LessThan60MinutesBDayFee"];
            feeExtra15BDayTextBox.Text = Model.Settings["Extra15MinutesBDayFee"];
            vatTextBox.Text = Model.Settings["VAT"];
            oldPrinterCheckbox.IsChecked = Model.Settings["OldPrinterCompatibility"] == "True" ? true : false;

            DirectSaleItems = new ObservableCollection<DirectSaleItem>(Model.GetDirectSaleItems());
            dataGrid.ItemsSource = DirectSaleItems;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            string errors = "";

            // BUSINESS NAME
            if(businessNameTextBox.Text != Model.Settings["BusinessName"] && businessNameTextBox.Text.Length != 0)
            {
               Model.SetSetting("BusinessName", businessNameTextBox.Text.Replace(@"\n", "\n"));
            }

            // BUSINESS CIF
            if(businessCIFTextBox.Text != Model.Settings["BusinessCIF"] && businessCIFTextBox.Text.Length != 0)
            {
                Model.SetSetting("BusinessCIF", businessCIFTextBox.Text);
            }

            // FEE FOR LESS THAN 30 MINUTES
            if(fee30TextBox.Text != Model.Settings["LessThan30MinutesFee"] && fee30TextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(fee30TextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("LessThan30MinutesFee", fee.ToString());
                }
                else
                {
                    errors += "Precio de 10 a 29 minutos: debes introducir un valor numérico.\n";
                }
            }

            // FEE FOR LESS THAN 60 MINUTES
            if (fee60TextBox.Text != Model.Settings["LessThan60MinutesFee"] && fee60TextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(fee60TextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("LessThan60MinutesFee", fee.ToString());
                }
                else
                {
                    errors += "Precio de 30 a 60 minutos: debes introducir un valor numérico.\n";
                }
            }

            // FEE EVERY 15 MIN OVER 60
            if (feeExtra15TextBox.Text != Model.Settings["Extra15MinutesFee"] && feeExtra15TextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(feeExtra15TextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("Extra15MinutesFee", fee.ToString());
                }
                else
                {
                    errors += "Precio 15 min extras: debes introducir un valor numérico.\n";
                }
            }

            // BDAY FEE FOR LESS THAN 30 MINUTES
            if (fee30BDayTextBox.Text != Model.Settings["LessThan30MinutesBDayFee"] && fee30BDayTextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(fee30BDayTextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("LessThan30MinutesBDayFee", fee.ToString());
                }
                else
                {
                    errors += "Precio cumpleaños de 10 a 29 minutos: debes introducir un valor numérico.\n";
                }
            }

            // BDAY FEE FOR LESS THAN 60 MINUTES
            if (fee60BDayTextBox.Text != Model.Settings["LessThan60MinutesBDayFee"] && fee60BDayTextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(fee60BDayTextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("LessThan60MinutesBDayFee", fee.ToString());
                }
                else
                {
                    errors += "Precio cumpleaños de 30 a 60 minutos: debes introducir un valor numérico.\n";
                }
            }

            // BDAY FEE EVERY 15 MIN OVER 60
            if (feeExtra15BDayTextBox.Text != Model.Settings["Extra15MinutesBDayFee"] && feeExtra15BDayTextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(feeExtra15BDayTextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("Extra15MinutesBDayFee", fee.ToString());
                }
                else
                {
                    errors += "Precio cumpleaños 15 min extras: debes introducir un valor numérico.\n";
                }
            }

            //VAT
            if (vatTextBox.Text != Model.Settings["VAT"] && vatTextBox.Text.Length != 0)
            {
                double fee;
                bool isParsable = double.TryParse(vatTextBox.Text.Replace('.', ','), out fee);

                if (isParsable)
                {
                    fee = Math.Round(fee, 2);
                    Model.SetSetting("VAT", fee.ToString());
                }
                else
                {
                    errors += "IVA de hora de ludoteca: debes introducir un valor numérico.\n";
                }
            }

            // WORKER PASSWORD
            if(workerPasswordTextBox.Password.Length > 0)
            {
                if(workerPasswordTextBox.Password == confirmWorkerPasswordTextBox.Password)
                {
                    Model.SetSetting("WorkerPassword", workerPasswordTextBox.Password);
                }
                else
                {
                    errors += "Contraseña de Monitor: Las contraseñas no coinciden.\n";
                }
            }

            // ADMIN PASSWORD
            if (adminPasswordTextBox.Password.Length > 0)
            {
                if (adminPasswordTextBox.Password == confirmAdminPasswordTextBox.Password)
                {
                    Model.SetSetting("AdminPassword", adminPasswordTextBox.Password);
                }
                else
                {
                    errors += "Contraseña de Administrador: Las contraseñas no coinciden.\n";
                }
            }

            // OLD PRINTER COMPATIBILITY
            Model.SetSetting("OldPrinterCompatibility", (bool)oldPrinterCheckbox.IsChecked ? "True" : "False");

            // DIRECT SALE ITEMS
            Model.SetDirectSaleItems(new List<DirectSaleItem>(DirectSaleItems));

            if (errors.Length > 0)
            {
                errors = "SE HAN ENCONTRADO LOS SIGUIENTES ERRORES:\n\n" + errors + "\nLos campos correctos han sido actualizados.";
                MessageBox.Show(errors, "Atención");
            }
            else
            {
                Close();
                MessageBox.Show("La configuración se ha editado con éxito.", "Éxito");
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            DirectSaleItem clickedItem = (DirectSaleItem)((Button)e.Source).DataContext;
            DirectSaleItems.Remove(clickedItem);
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            DirectSaleItems.Add(new DirectSaleItem("", "", 0, 0));
        }
    }
}
