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
    /// Interaction logic for AddAdmissionForm.xaml
    /// </summary>
    
    
    public class DirectSalePreviewRow
    {
        public string Name { get; set; }
        public double UnitPriceWithVAT { get; set; }
        public double TotalCost { get; set; }
        public int Units { get; set; }

        public DirectSalePreviewRow(string name, double unitPriceWithVAT, double totalCost, int units)
        {
            Name = name;
            UnitPriceWithVAT = unitPriceWithVAT;
            TotalCost = totalCost;
            Units = units;
        }
    }

    public partial class DirectSale : Window
    {
        DirectSaleCart Cart = new DirectSaleCart();
        public ObservableCollection<DirectSalePreviewRow> SalePreview { get; set; }
        private void UpdatePreview()
        {
            double totalCost = 0;
            SalePreview.Clear();
            foreach (KeyValuePair<DirectSaleItem, int> item in Cart.ItemsInCart)
            {
                int units = item.Value;
                if (units > 0)
                {
                    double unitPriceWithVat = Math.Round(item.Key.Price, 2);
                    double totalItemCost = Math.Round(unitPriceWithVat * units, 2);
                    SalePreview.Add(new DirectSalePreviewRow(item.Key.Name, unitPriceWithVat, totalItemCost, units));
                    totalCost += totalItemCost;
                }
            }

            totalCostLabel.Content = "TOTAL: " + string.Format("{0:N2}€", Math.Round(totalCost, 2));
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            Model.CloseDirectSale(Cart);
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public DirectSale()
        {
            InitializeComponent();

            SalePreview = new ObservableCollection<DirectSalePreviewRow>();
            dataGrid.ItemsSource = SalePreview;

            List<DirectSaleItem> directSaleItems = Model.GetDirectSaleItems();

            if(directSaleItems.Count == 0)
            {
                Label noItemsLabel = new Label();
                noItemsLabel.Content = "No hay ningún producto configurado.\nAñade artículos en Administración > Configuración > Venta Directa";
                noItemsLabel.FlowDirection = FlowDirection.LeftToRight;
                stackPanel.Children.Add(noItemsLabel);
            }

            foreach(DirectSaleItem item in directSaleItems)
            {
                StackPanel buttonPair = new StackPanel();
                buttonPair.Orientation = Orientation.Horizontal;
                buttonPair.Margin = new Thickness { Top = 10, Right = 25, Bottom = 0, Left = 0 };
                buttonPair.FlowDirection = FlowDirection.LeftToRight;

                Button addButton = new Button();
                addButton.Content = item.Label;
                addButton.Height = 80;
                addButton.Width = 150;
                addButton.Margin = new Thickness { Top = 0, Right = 10, Bottom = 0, Left = 0 };

                void OnClickAdd(object sender, RoutedEventArgs e)
                {
                    Cart.AddItem(item);
                    UpdatePreview();
                }

                addButton.Click += new RoutedEventHandler(OnClickAdd);
                buttonPair.Children.Add(addButton);

                Button removeButton = new Button();
                removeButton.Content = "-";
                removeButton.Height = 80;
                removeButton.Width = 60;

                void OnClickRemove(object sender, RoutedEventArgs e)
                {
                    Cart.RemoveItem(item);
                    UpdatePreview();
                }

                removeButton.Click += new RoutedEventHandler(OnClickRemove);
                buttonPair.Children.Add(removeButton);

                stackPanel.Children.Add(buttonPair);
            }

            UpdatePreview();
        }
    }
}
