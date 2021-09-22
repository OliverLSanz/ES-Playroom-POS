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

        static Model()
        {
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
        }

        public static void LoadSettings()
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            double minutes = timeSpan.TotalMinutes;

            double lessThan30MinsCharge  = double.Parse(Settings["LessThan30MinutesFee"]),
                   lessThan60MinsCharge  = double.Parse(Settings["LessThan60MinutesFee"]),
                   each15minsExtraCharge = double.Parse(Settings["Extra15MinutesFee"]);

            double amount;

            if(minutes < 10)
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
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
            {
                connection.Open();

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
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            }
        }

        public static void CloseAdmission(long hanger, DateTime closeDateTime, double amount)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
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
            // Create a PrintDialog  
            PrintDialog printDlg = new PrintDialog();
            // Create a FlowDocument dynamically.  
            document.Name = "FlowDoc";
            // Create IDocumentPaginatorSource from FlowDocument  
            IDocumentPaginatorSource idpSource = document;
            // Call PrintDocument method to send document to printer  
            printDlg.PrintDocument(idpSource.DocumentPaginator, "Hello WPF Printing.");
        }

        public static void TestDatabase()
        {
            using (var connection = new SqliteConnection("Data Source=database.db"))
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
    }
}
