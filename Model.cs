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

        static Model()
        {
            Admissions = new ObservableCollection<Admission>();
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
                    while (reader.Read())
                    {

                        int ordinal = reader.GetOrdinal("id");
                        long id = (long) reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("hanger");
                        long hanger = (long) reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("name");
                        string name = (string) reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("date");
                        string date = (string) reader.GetValue(ordinal);

                        ordinal = reader.GetOrdinal("start_hour");
                        string startHour = (string) reader.GetValue(ordinal);

                        string endHour;
                        ordinal = reader.GetOrdinal("end_hour");
                        if (!reader.IsDBNull(ordinal))
                        {
                            endHour = (string) reader.GetValue(ordinal);
                        }
                        else
                        {
                            endHour = "";
                        }

                        double amount;
                        ordinal = reader.GetOrdinal("amount");
                        if (!reader.IsDBNull(ordinal))
                        {
                            amount = (double) reader.GetValue(ordinal);
                        }
                        else
                        {
                            amount = 0;
                        }

                        Admissions.Add(
                            new Admission(id: id, hanger: hanger, name: name, date: date, startHour: startHour, endHour: endHour, amount: amount)
                        );
                    }
                }
            }
        }

        private static string GetDateStringFromDateTime(DateTime datetime)
        {
            return datetime.ToString("MM-dd-yyyy");
        }

        private static string GetTodayDateString()
        {
            DateTime datetime = DateTime.Now;
            return GetDateStringFromDateTime(datetime);
        }

        private static string GetHourStringFromDateTime(DateTime datetime)
        {
            return datetime.ToString("HH:mm");
        }

        private static string GetNowHourString()
        {
            DateTime datetime = DateTime.Now;
            return GetHourStringFromDateTime(datetime);
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

            }

        }

        public static void AddNewAdmission(int hanger, string name)
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

        public static void CloseAdmission(int hanger)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=database.db"))
            {
                connection.Open();

                DateTime datetime = DateTime.Now;
                string end_hour = "endooo";
                double amount = 1.99;

                SqliteCommand command = connection.CreateCommand();
                command.CommandText =
                @"
                    UPDATE admissions
                    SET end_hour = $end_hour,
                        amount = $amount, 
                    WHERE hanger = $hanger AND end_hour IS NULL;
                ";
                command.Parameters.AddWithValue("$end_hour", end_hour);
                command.Parameters.AddWithValue("$amount", amount);
                command.Parameters.AddWithValue("$hanger", hanger);

                command.ExecuteNonQuery();
            }
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
