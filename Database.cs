using System;
using System.Collections.Generic;
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
        public int Hanger { get; set; }
        public string Name { get; set; }
        public string StartHour { get; set; }

        public Admission(int hanger, string name, string start_hour)
        {
            this.Hanger = hanger;
            this.Name = name;
            this.StartHour = start_hour;
        }
    }

    public static class Database
    {
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

                DateTime datetime = DateTime.Now;
                string date = datetime.ToString("MM-dd-yyyy");
                string time = datetime.ToString("HH:mm:ss");

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
