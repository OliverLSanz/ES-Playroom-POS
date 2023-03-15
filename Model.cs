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

    public class DirectSaleItem
    {
        // Short internal name
        public string Label { get; set; }
        // Full name shown in the ticket
        public string Name { get; set; }
        public double Price { get; set; }
        public double VAT { get; set; }

        public DirectSaleItem(string label, string name, double price, double vat)
        {
            Label = label;
            Name = name;
            Price = price;
            VAT = vat;
        }
    }

    public class DirectSaleCart
    {
        public Dictionary<DirectSaleItem, int> ItemsInCart = new Dictionary<DirectSaleItem, int>();

        public void AddItem(DirectSaleItem item)
        {
            if (!ItemsInCart.ContainsKey(item))
            {
                ItemsInCart.Add(item, 0);
            }
            ItemsInCart[item] += 1;
        }

        public void RemoveItem(DirectSaleItem item)
        {
            if (ItemsInCart.ContainsKey(item) && ItemsInCart[item] > 0)
            {
                ItemsInCart[item] -= 1;
            }
        }
    }

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
            DefaultSettings.Add("LessThan30MinutesBDayFee", "3");
            DefaultSettings.Add("LessThan60MinutesBDayFee", "4,5");
            DefaultSettings.Add("Extra15MinutesBDayFee", "0,5");
            DefaultSettings.Add("OldPrinterCompatibility", "False");
        }

        public static List<DirectSaleItem> GetDirectSaleItems()
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM direct_sale_products;
                ";

                using (var reader = command.ExecuteReader())
                {
                    List<DirectSaleItem> products = new List<DirectSaleItem>();
                    while (reader.Read())
                    {
                        int ordinal = reader.GetOrdinal("label");
                        string label = (string)reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("name");
                        string name = (string)reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("price");
                        double price = (double)reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("vat");
                        double vat = (double)reader.GetValue(ordinal);

                        products.Add(
                            new DirectSaleItem(label: label, name: name, price: price, vat: vat)
                        ); ;
                    }

                    return products;
                }
            }
        }

        public static void SetDirectSaleItems(List<DirectSaleItem> products)
        {
            using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    DELETE FROM direct_sale_products;
                ";

                command.ExecuteNonQuery();

                foreach(DirectSaleItem item in products)
                {
                    command = connection.CreateCommand();
                    command.CommandText =
                        @"
                        INSERT OR REPLACE INTO direct_sale_products (label, name, price, vat)
                        VALUES ($label, $name, $price, $vat);
                        ";

                    command.Parameters.AddWithValue("$label", item.Label);
                    command.Parameters.AddWithValue("$name", item.Name);
                    command.Parameters.AddWithValue("$price", item.Price);
                    command.Parameters.AddWithValue("$vat", item.VAT);

                    command.ExecuteNonQuery();
                }
            }
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

        public static double GetAmountFromTimeSpan(TimeSpan timeSpan, bool isBDay=false)
        {
            double minutesMargin = 10;  // margin in minutes for charges, since we don't want to charge 
                                        // 1 hour for exactly 60 minutes.
            double minutes = timeSpan.TotalMinutes - minutesMargin;

            double lessThan30MinsCharge = isBDay ?
                double.Parse(Settings["LessThan30MinutesBDayFee"]) :
                double.Parse(Settings["LessThan30MinutesFee"]);

            double lessThan60MinsCharge = isBDay ?
                double.Parse(Settings["LessThan60MinutesBDayFee"]) :
                double.Parse(Settings["LessThan60MinutesFee"]);

            double each15minsExtraCharge = isBDay ? 
                double.Parse(Settings["Extra15MinutesBDayFee"]) :  
                double.Parse(Settings["Extra15MinutesFee"]);

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
                amount = lessThan60MinsCharge + (Math.Truncate((minutes - 60) / 15) + 1) * each15minsExtraCharge;
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

                }

                foreach (KeyValuePair<string, string> entry in DefaultSettings)
                {
                    command = connection.CreateCommand();

                    command.CommandText =
                    @"
                        INSERT OR IGNORE INTO settings (id, value)
                        VALUES( $id, $value );
                    ";
                    command.Parameters.AddWithValue("$id", entry.Key);
                    command.Parameters.AddWithValue("$value", entry.Value);

                    command.ExecuteNonQuery();
                    reader.Close();
                };

                command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS direct_sale_products (
                        id INTEGER PRIMARY KEY,
                        label TEXT DEFAULT '',
                        name TEXT DEFAULT '',
                        price REAL DEFAULT 0,
                        vat REAL DEFAULT 0
                    );   
                ";

                command.ExecuteNonQuery();

                command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS direct_sale_history (
                        id INTEGER PRIMARY KEY,
                        timestamp TEXT DEFAULT '',
                        product TEXT DEFAULT '',
                        quantity INT DEFAULT 0,
                        price REAL DEFAULT 0,
                        vat_rate REAL DEFAULT 0,
                        total REAL DEFAULT 0
                    );   
                ";

                command.ExecuteNonQuery();
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

        public static void RecordDirectSale(DirectSaleCart cart)
        {
            using (SqliteConnection connection = new(SQLiteConnectionString))
            {
                connection.Open();
                foreach (KeyValuePair<DirectSaleItem, int> item in cart.ItemsInCart)
                {
                    int unitCount = item.Value;
                    if (unitCount > 0)
                    {
                        string product = item.Key.Name;
                        int quantity = item.Value;
                        double vatRate = item.Key.VAT;
                        double unitPrice = Math.Round(item.Key.Price, 2);
                        double total = Math.Round(unitPrice * unitCount, 2);
                        string timestamp = GetTodayDateString() + " " + GetNowHourString();

                        SqliteCommand command = connection.CreateCommand();

                        command.CommandText =
                        @"
                        INSERT OR IGNORE INTO direct_sale_history (timestamp, product, quantity, price, vat_rate, total)
                        VALUES( $timestamp, $product, $quantity, $price, $vat_rate, $total );
                        ";
                        command.Parameters.AddWithValue("$timestamp", timestamp);
                        command.Parameters.AddWithValue("$product", product);
                        command.Parameters.AddWithValue("$quantity", quantity);
                        command.Parameters.AddWithValue("$price", unitPrice);
                        command.Parameters.AddWithValue("$vat_rate", vatRate);
                        command.Parameters.AddWithValue("$total", total);

                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
        }

        public static void CloseDirectSale(DirectSaleCart cart)
        {
            RecordDirectSale(cart);
            PrintFlowDocument(CreateDirectSaleReceipt(cart));
        }

        private static FlowDocument CreateDirectSaleReceipt(DirectSaleCart cart)
        {
            string date = GetTodayDateString();
            string time = GetNowHourString();

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


            // DATOS
            Paragraph data = new Paragraph();
            data.Inlines.Add(new Run(Model.CompatibleString($"{date}  {time}\n\n\n")));
            // data.Inlines.Add(new Run(Model.CompatibleString($"Número de ticket: {Admission.Id}\n\n")));

            double total = 0;
            double unitPrice, productsCost, unitCount;

            foreach(KeyValuePair<DirectSaleItem, int> item in cart.ItemsInCart)
            {
                unitCount = item.Value;
                if (unitCount > 0)
                {
                    unitPrice = Math.Round(item.Key.Price, 2);
                    productsCost = Math.Round(unitPrice * unitCount, 2);
                    data.Inlines.Add(new Run(Model.CompatibleString($"{item.Value}   {item.Key.Name}\n")));
                    data.Inlines.Add(new Run(Model.CompatibleString($"{productsCost:n2}€        (PVP {unitPrice:n2})\n\n")));
                    total = Math.Round(total + productsCost, 2);
                }
            }

            data.Inlines.Add(new Run(Model.CompatibleString($"\nTOTAL: {total:n2}€\n\n\n")) { FontSize = 15, FontWeight = FontWeights.Bold });


            Dictionary<double, double> vatAndAmount = new();
            data.Inlines.Add(new Run(Model.CompatibleString($"Desglose de IVA\n")));
            data.Inlines.Add(new Run(Model.CompatibleString($"       %    Base     IVA    Total\n")));
            foreach (KeyValuePair<DirectSaleItem, int> item in cart.ItemsInCart)
            {
                if (!vatAndAmount.ContainsKey(item.Key.VAT))
                {
                    vatAndAmount.Add(item.Key.VAT, 0);
                }
                vatAndAmount[item.Key.VAT] += Math.Round(item.Key.Price * item.Value, 2);
            }

            foreach(KeyValuePair<double, double> item in vatAndAmount)
            {
                double vatRate = item.Key;
                double totalAmount = item.Value;
                double baseAmount = Math.Round(totalAmount * (1 - vatRate), 2);
                double vatAmount = Math.Round(totalAmount * vatRate, 2);
                data.Inlines.Add(new Run(Model.CompatibleString($"{Math.Round(100 * vatRate, 0),7:n2} {baseAmount,7:n2} {vatAmount,7:n2} {totalAmount,7:n2}\n")));
            }

            data.Inlines.Add(new Run(Model.CompatibleString("\n\nGracias por su visita")));
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
            // Debug when printer is not available
            // string text = new TextRange(document.ContentStart, document.ContentEnd).Text;
            // MessageBox.Show(text);
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
            MessageBox.Show("Exportando fichero 1:\nDatos de entradas y salidas.\n\nPulsa OK para guardar.");
            ExportAdmissionsData();
            MessageBox.Show("Exportando fichero 2:\nDatos de venta directa.\n\nPulsa OK para guardar.");
            ExportDirectSaleData();
        }

        public static void ExportTableData(
            string tableName, 
            string pageTitle, 
            List<string> sqlColumnNames, 
            string exportHeader
        )
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV|*.csv";
            saveFileDialog.Title = pageTitle;

            if (saveFileDialog.ShowDialog() == true)
            {
                List<string> lines = new List<string>();

                using (SqliteConnection connection = new SqliteConnection(SQLiteConnectionString))
                {
                    connection.Open();

                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText =
                    $@"
                        SELECT *
                        FROM {tableName};
                    ";

                    lines.Add(exportHeader);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string line = "";
                            foreach (string columnName in sqlColumnNames)
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

        public static void ExportDirectSaleData()
        {
            List<string> columns = new List<string>
                {
                    "timestamp", "product", "quantity", "price", "vat_rate", "total"
                };

            ExportTableData(
                "direct_sale_history",
                "Guardar datos venta directa",
                columns,
                "fecha_hora,producto,cantidad,PVP_unitario,IVA,total"
            );
        }

        public static void ExportAdmissionsData()
        {
            List<string> columns = new List<string>
                {
                    "id", "hanger", "name", "date", "start_hour", "end_hour", "amount"
                };

            ExportTableData(
                "admissions",
                "Guardar datos entradas",
                columns,
               "id,numero_entrada,nombre,fecha,hora_entrada,hora_salida,importe_con_IVA"
            );
        }
    }
}
