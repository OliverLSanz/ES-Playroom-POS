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
using System.IO;
using Microsoft.Win32;
using Microsoft.Data.Sqlite;

namespace Playroom_Kiosk
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow()
        {
            InitializeComponent();
            ExportAllData();
            Close();
            MessageBox.Show("Los datos se han exportado con éxito.", "Éxito");
        }

        private void ExportAllData()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV|*.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> lines = new List<string>();

                using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    SELECT *
                    FROM admissions;
                ";

                    List<string> columns = new List<string>
                {
                    "id", "hanger", "name", "date", "start_hour", "end_hour", "amount"
                };

                    lines.Add("id, numero_entrada, nombre, fecha, hora_entrada, hora_salida, importe_con_IVA");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string line = "";
                            foreach (string columnName in columns)
                            {
                                int columnOrdinal = reader.GetOrdinal(columnName);
                                if (!reader.IsDBNull(columnOrdinal))
                                {
                                    line += reader.GetString(columnOrdinal);
                                }
                                line += ",";
                            }

                            // Remove the last "," added
                            if (line.Length > 0)
                            {
                                line = line.Remove(line.Length - 1);
                            }

                            lines.Add(line);
                        }
                    }
                }

                File.WriteAllLines(saveFileDialog.FileName, lines);
            }
        }
    }
}
