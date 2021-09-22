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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Printing;
using System.Diagnostics;
using System.IO;

namespace Playroom_Kiosk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>  
        /// This method creates a dynamic FlowDocument. You can add anything to this  
        /// FlowDocument that you would like to send to the printer  
        /// </summary>  
        /// <returns></returns>  
        private FlowDocument CreateFlowDocument()
        {
            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            // Create a Section  
            Section sec = new Section();
            // Create first Paragraph  
            Paragraph p1 = new Paragraph();
            // Create and add a new Bold, Italic and Underline  
            Bold bld = new Bold();
            bld.Inlines.Add(new Run("Hello World"));
            Italic italicBld = new Italic();
            italicBld.Inlines.Add(bld);
            Underline underlineItalicBld = new Underline();
            underlineItalicBld.Inlines.Add(italicBld);
            // Add Bold, Italic, Underline to Paragraph  
            p1.Inlines.Add(underlineItalicBld);
            // Add Paragraph to Section  
            sec.Blocks.Add(p1);
            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }

        public MainWindow()
        {
            InitializeComponent();

            DataGrid.ItemsSource = Model.Admissions;
            Model.PopulateAdmissions();
        }

        private void ButtonAddName_Click(object sender, RoutedEventArgs e)
        {
            // Create a PrintDialog  
            PrintDialog printDlg = new PrintDialog();
            // Create a FlowDocument dynamically.  
            FlowDocument doc = CreateFlowDocument();
            doc.Name = "FlowDoc";
            // Create IDocumentPaginatorSource from FlowDocument  
            IDocumentPaginatorSource idpSource = doc;
            // Call PrintDocument method to send document to printer  
            // printDlg.PrintDocument(idpSource.DocumentPaginator, "Hello WPF Printing.");
            Trace.WriteLine("HELLO?");
            Model.InitDB();
            Model.AddNewAdmission(1, "pe pe");
            Model.TestDatabase();

            Trace.WriteLine(
                 Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            Trace.WriteLine(System.IO.Path.GetFullPath("hola"));
        }

        private void ButtonPopulate_Click(object sender, RoutedEventArgs e)
        {
            Model.Admissions.Add(new Admission (id:1, hanger:1, name:"firstname-1", startHour:"00:00", date: "asda"));
            Model.Admissions.Add(new Admission (id: 2, hanger: 2, name: "firasdasname-1", startHour: "00:01", date: "asda"));
        }

        private void ButtonAddAdmission_Click(object sender, RoutedEventArgs e)
        {
            new AddAdmissionForm().Show();
        }
    }
}
