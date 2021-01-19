using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Linq;

namespace MigrateDatabase
{
    class Program
    {
        private static readonly string _conn = ConfigurationManager.AppSettings["APPDS"];
        private static readonly string _sqllite = ConfigurationManager.AppSettings["SQLLite"];

        static void Main(string[] args)
        {
            ImportData();

            Console.WriteLine("Completed successfully!");

            Console.ReadLine();
        }

        private static void ImportData()
        {
            var tables = FetchMySqlTables();

            foreach (var table in tables.Where(x=>!x.Contains("_")))
            {
                Console.WriteLine($"Importing data for {table}...");

                var dt = FetchData(table);
                using (MySqlConnection connection = new MySqlConnection(_conn))
                {
                    connection.Open();
                    var bulkCopy = new MySqlBulkCopy(connection) { DestinationTableName = table };
                    bulkCopy.WriteToServer(dt);
                }


            }
        }

        private static DataTable FetchData(string tablename)
        {
            var dt = new DataTable();
            try
            {
                var sql = "SELECT * FROM " + tablename + ";";
                using (var connection = new SqliteConnection(_sqllite))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 5 * 60;

                        var dr = cmd.ExecuteReader();
                        dt.Load(dr);
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("I got an error on FetchData: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return dt;
        }

        private static List<string> FetchMySqlTables()
        {
            var tables = new List<string>();
            var sql = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
            try
            {
                using (var connection = new SqliteConnection(_sqllite))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 15 * 60;
                        var dr = cmd.ExecuteReader();
                        while (dr.Read())
                            tables.Add(dr.GetString(0));
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("I got an error on FetchMySqlTables: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return tables;
        }
    }
}
