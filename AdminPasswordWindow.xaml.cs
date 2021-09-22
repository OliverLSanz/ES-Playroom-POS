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
    /// Interaction logic for AdminPasswordWindow.xaml
    /// </summary>
    public partial class AdminPasswordWindow : Window
    {
        private Type NextWindow;
        public AdminPasswordWindow(Type nextWindow)
        {
            InitializeComponent();

            NextWindow = nextWindow;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            string password = Model.Settings["AdminPassword"];
            
            if(passwordTextBox.Text == password)
            {
                Window nextWindowInstance = (Window)Activator.CreateInstance(NextWindow);
                try
                {
                    nextWindowInstance.Show();
                }
                catch (System.InvalidOperationException)
                {
                    // The window is closed itself during its construction
                }
                Close();
            }
            else
            {
                MessageBox.Show("La contraseña es incorrecta.", "Atención");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
