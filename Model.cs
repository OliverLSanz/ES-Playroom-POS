using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;

namespace Playroom_Kiosk
{

    public class Admission
    {
        public long Hanger { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string StartHour { get; set; }
        public long Id { get; set; }
        public string EndHour { get; set; }
        public double Amount { get; set; }

        // Below attributes are just to show computed values in the DataGrid
        public string CurrentDuration { 
            get 
            {
                DateTime start = Model.DateTimeFromStrings(Date, StartHour);
                DateTime now = DateTime.Now;
                TimeSpan duration = now.Subtract(start);
                int hours   = (int) Math.Floor(duration.TotalHours);
                int minutes = (int) Math.Floor(duration.TotalMinutes%60);

                string hoursString = hours > 0? hours.ToString() + "h " : "";
                string minutesString = minutes.ToString() + "m";
                return $"{hoursString}{minutesString}";
            } 
        }
        public string CurrentAmount 
        { 
            get
            {
                DateTime start = Model.DateTimeFromStrings(Date, StartHour);
                DateTime now = DateTime.Now;
                TimeSpan duration = now.Subtract(start);
                double amount = Model.GetAmountFromTimeSpan(duration);
                return amount.ToString() + "€";
            } 
        }


        public Admission(long id, long hanger, string name, string date, string startHour, string endHour = "", double amount = 0)
        {
            Hanger = hanger;
            Name = name;
            Date = date;
            StartHour = startHour;
            EndHour = endHour;
            Id = id;
            Amount = amount;
        }
    }

    public static class Model
    {
        public static ObservableCollection<Admission> Admissions { get; set; }
        public static Dictionary<string, string> Settings { get; set; }

        public static Dictionary<string, string> DefaultSettings { get; set; }
        public static string SQLiteConnectionString { get; set; }

        static Model()
        {
            SQLiteConnectionString =  new SqliteConnectionStringBuilder("Data Source=database.db")
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = "placeholder_password"
            }.ToString();

            Admissions = new ObservableCollection<Admission>();
            Settings = new Dictionary<string, string>();
            DefaultSettings = new Dictionary<string, string>();
            DefaultSettings.Add("VAT", "0,21");
            DefaultSettings.Add("BusinessName", "Ludoteca");
            DefaultSettings.Add("BusinessCIF", "000000000");
            DefaultSettings.Add("WorkerPassword", "1234");
            DefaultSettings.Add("AdminPassword", "admin");
            DefaultSettings.Add("LessThan30MinutesFee", "4");
            DefaultSettings.Add("LessThan60MinutesFee", "5,5");
            DefaultSettings.Add("Extra15MinutesFee", "1");
            DefaultSettings.Add("OldPrinterCompatibility", "False");
        }

        public static void LoadSettings()
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                Settings.Clear();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM settings;
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(reader.GetOrdinal("id"));
                        string value = reader.GetString(reader.GetOrdinal("value"));
                        Settings.Add(id, value);
                    }
                }
            }
        }

        public static void SetSetting(string id, string value)
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT OR REPLACE INTO settings (id, value)
                    VALUES ($key, $value);
                ";
                command.Parameters.AddWithValue("$key", id);
                command.Parameters.AddWithValue("$value", value);

                command.ExecuteNonQuery();
            }

            LoadSettings();
        }

        public static List<Admission> GetAdmissionsFromReader(SqliteDataReader reader)
        {
            List<Admission> admissions = new List<Admission>();
            while (reader.Read())
            {
                int ordinal = reader.GetOrdinal("id");
                long id = (long)reader.GetValue(ordinal);

                ordinal = reader.GetOrdinal("hanger");
                long hanger = (long)reader.GetValue(ordinal);

                ordinal = reader.GetOrdinal("name");
                string name = (string)reader.GetValue(ordinal);

                ordinal = reader.GetOrdinal("date");
                string date = (string)reader.GetValue(ordinal);

                ordinal = reader.GetOrdinal("start_hour");
                string startHour = (string)reader.GetValue(ordinal);

                string endHour;
                ordinal = reader.GetOrdinal("end_hour");
                if (!reader.IsDBNull(ordinal))
                {
                    endHour = (string)reader.GetValue(ordinal);
                }
                else
                {
                    endHour = "";
                }

                double amount;
                ordinal = reader.GetOrdinal("amount");
                if (!reader.IsDBNull(ordinal))
                {
                    amount = (double)reader.GetValue(ordinal);
                }
                else
                {
                    amount = 0;
                }

                admissions.Add(
                    new Admission(id: id, hanger: hanger, name: name, date: date, startHour: startHour, endHour: endHour, amount: amount)
                );
            }
            return admissions;
        }

        public static List<Admission> GetTodayAdmissions()
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM admissions
                    WHERE date = $today;
                ";
                string today = GetTodayDateString();
                command.Parameters.AddWithValue("$today", today);

                using (var reader = command.ExecuteReader())
                {
                    return GetAdmissionsFromReader(reader);
                }
            }
        }
        public static void PopulateAdmissions()
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                Admissions.Clear();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM admissions
                    WHERE date = $today AND end_hour IS NULL;
                ";
                string today = GetTodayDateString();
                command.Parameters.AddWithValue("$today", today);

                using (var reader = command.ExecuteReader())
                {
                    foreach(Admission admission in GetAdmissionsFromReader(reader))
                    {
                        Admissions.Add(admission);
                    }
                }
            }
        }

        public static string GetDateStringFromDateTime(DateTime datetime)
        {
            return datetime.ToString("MM-dd-yyyy");
        }

        public static string GetTodayDateString()
        {
            DateTime datetime = DateTime.Now;
            return GetDateStringFromDateTime(datetime);
        }

        public static string GetHourStringFromDateTime(DateTime datetime)
        {
            return datetime.ToString("HH:mm");
        }

        public static string GetNowHourString()
        {
            DateTime datetime = DateTime.Now;
            return GetHourStringFromDateTime(datetime);
        }

        public static DateTime DateTimeFromStrings(string date, string time)
        {
            string dateTimeString = date + " " + time;
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(dateTimeString, "MM-dd-yyyy HH:mm", provider);
        }

        public static string GetStringFromTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        public static double GetAmountFromTimeSpan(TimeSpan timeSpan)
        {
            double minutesMargin = 10;  // margin in minutes for charges, since we don't want to charge 
                                        // 1 hour for exactly 60 minutes.
            double minutes = timeSpan.TotalMinutes - minutesMargin;

            double lessThan30MinsCharge  = double.Parse(Settings["LessThan30MinutesFee"]),
                   lessThan60MinsCharge  = double.Parse(Settings["LessThan60MinutesFee"]),
                   each15minsExtraCharge = double.Parse(Settings["Extra15MinutesFee"]);

            double amount;

            if(minutes < 0)
            {
                amount = 0;
            }
            else if(minutes < 30)
            {
                amount = lessThan30MinsCharge;
            }
            else if(minutes < 60)
            {
                amount = lessThan60MinsCharge;
            }
            else
            {
                amount = lessThan60MinsCharge + Math.Truncate((minutes - 60) / 15) * each15minsExtraCharge;
            }

            Trace.WriteLine(amount);
            Trace.WriteLine(minutes);

            return amount;
        }

        public static void InitDB()
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                try
                {
                    connection.Open();
                } 
                catch (SqliteException e)
                {
                    MessageBox.Show("No se pudo abrir la base de datos. Probablemente la contraseña de la base de datos es incorrecta.", "Error crítico");
                }

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS admissions (
                        id INTEGER PRIMARY KEY,
                        hanger INTEGER NOT NULL,
                        name TEXT NOT NULL,
                        date TEXT NOT NULL,
                        start_hour TEXT NOT NULL,
                        end_hour TEXT,
                        amount REAL
                    );   
                ";

                command.ExecuteNonQuery();

                command.CommandText =
                @"
                    SELECT name FROM sqlite_master WHERE type='table' AND name='settings';
                ";

                SqliteDataReader reader = command.ExecuteReader();

                bool settingsTableExists;

                if (reader.HasRows)
                {
                    settingsTableExists = true;
                }
                else
                {
                    settingsTableExists = false;
                }

                reader.Close();

                if (!settingsTableExists)
                {
                    command.CommandText =
                    @"
                        CREATE TABLE IF NOT EXISTS settings (
                            id STRING PRIMARY KEY,
                            value STRING NOT NULL
                        );
                    ";
                    
                    command.ExecuteNonQuery();

                    foreach(KeyValuePair<string, string> entry in DefaultSettings)
                    {
                        command = connection.CreateCommand();

                        command.CommandText =
                        @"
                            INSERT INTO settings (id, value)
                            VALUES( $id, $value);
                        ";
                        command.Parameters.AddWithValue("$id", entry.Key);
                        command.Parameters.AddWithValue("$value", entry.Value);

                        command.ExecuteNonQuery();
                    };

                }


            }

        }

        public static void AddNewAdmission(long hanger, string name)
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                string date = GetTodayDateString();
                string time = GetNowHourString();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO admissions (hanger, name, date, start_hour)
                    VALUES( $hanger, $name, $date, $time );
                ";

                command.Parameters.AddWithValue("$date", date);
                command.Parameters.AddWithValue("$time", time);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$hanger", hanger);

                command.ExecuteNonQuery();
                
                PrintFlowDocument(CreateAdmissionTicket(date, time, hanger));
            }

        }

        private static FlowDocument CreateAdmissionTicket(string date, string hour, long hanger)
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
            sec.Blocks.Add(businessName);

            // DATOS
            Paragraph data = new Paragraph();
            data.Inlines.Add(new Run(Model.CompatibleString($"{date}\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Hora de entrada: {hour}\n\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"Número: {hanger}\n\n")) { FontSize = 30 });
            sec.Blocks.Add(data);

            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);
            return doc;
        }

        public static void CloseAdmission(long hanger, DateTime closeDateTime, double amount)
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                string end_hour = GetHourStringFromDateTime(closeDateTime);

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE admissions
                    SET end_hour = $end_hour,
                        amount = $amount
                    WHERE hanger = $hanger AND end_hour IS NULL;
                ";
                command.Parameters.AddWithValue("$end_hour", end_hour);
                command.Parameters.AddWithValue("$amount", amount);
                command.Parameters.AddWithValue("$hanger", hanger);

                command.ExecuteNonQuery();
            }
        }

        public static bool IsPlayRoomEmpty()
        {
            return Admissions.Count == 0;
        }

        public static double GetVAT(double amount)
        {
            double vat = double.Parse(Settings["VAT"]);
            return Math.Round((amount / (1 + vat)) * vat, 2);
        }

        public static void PrintFlowDocument(FlowDocument document)
        {
            Print(document);

            if (Settings["OldPrinterCompatibility"] == "True"){
                Print(CreateEmptyDocument());
            }
        }

        private static void Print(FlowDocument document)
        {
            // Create a PrintDialog  
            PrintDialog printDlg = new PrintDialog();
            // Create a FlowDocument dynamically.  
            document.Name = "FlowDoc";
            // Create IDocumentPaginatorSource from FlowDocument  
            IDocumentPaginatorSource idpSource = document;
            // Call PrintDocument method to send document to printer  
            printDlg.PrintDocument(idpSource.DocumentPaginator, "Hello WPF Printing.");
        }

        // This function prints a small empty document to fix a problem with Bixolon printers:
        // the end of the printed document is not printed until you send another print job
        public static FlowDocument CreateEmptyDocument()
        {
            // Create a FlowDocument  
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Verdana");
            doc.FontSize = 13;

            // Create a Section  
            Section sec = new Section();

            // TITULO
            Paragraph emptySpace = new Paragraph();
            emptySpace.Inlines.Add(new Run(Model.CompatibleString("\n\n\n\n\n\n\n\n\n\n")));
            emptySpace.Inlines.Add(new Run(Model.CompatibleString(".")) { FontSize = 1 });

            sec.Blocks.Add(emptySpace);

            // Add Section to FlowDocument  
            doc.Blocks.Add(sec);

            return (doc);
        }

        public static void TestDatabase()
        {
            using (var connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM admissions;
                ";

                Trace.WriteLine("A ver si hay alguien...");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var one = reader.IsDBNull(0) ? "null" : reader.GetString(0);
                        var two = reader.IsDBNull(1) ? "null" : reader.GetString(1);
                        var three = reader.IsDBNull(2) ? "null" : reader.GetString(2);
                        var four = reader.IsDBNull(3) ? "null" : reader.GetString(3);
                        var five = reader.IsDBNull(4) ? "null" : reader.GetString(4);
                        var six = reader.IsDBNull(5) ? "null" : reader.GetString(5);
                        var seven = reader.IsDBNull(6) ? "null" : reader.GetString(6);


                        Trace.WriteLine($"Row, {one} {two} {three} {four} {five} {six} {seven}!");
                    }
                }
            }
        }

        public static string CompatibleString(string str)
        {
            if(Settings["OldPrinterCompatibility"] == "False")
            {
                return str;
            }
            else
            {
                return str
                    .Replace("€", " EUR")
                    .Replace("á", "a")
                    .Replace("é", "e")
                    .Replace("í", "i")
                    .Replace("ó", "o")
                    .Replace("ú", "u")
                    .Replace("ñ", "n")
                    .Replace("-", "/");
            }
        }
        public static void ExportAllData()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV|*.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> lines = new List<string>();

                using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
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
