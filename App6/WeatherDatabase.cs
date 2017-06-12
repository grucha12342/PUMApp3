using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Android.Database.Sqlite;
using Android.Database;

namespace App6
{
    class WeatherDatabase
    {
        private List<WeatherObservation> history = new List<WeatherObservation>();
        static string docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string pathToDatabase = Path.Combine(docsFolder, "db_weather.db");

        public async Task<string> CreateDatabase()
        {
            try
            {
                if (!File.Exists(pathToDatabase))
                {
                    var connection = new SQLiteAsyncConnection(pathToDatabase);
                    await connection.CreateTableAsync<WeatherObservation>();
                    Console.WriteLine("DB created in: " + pathToDatabase);
                    return "Database created";
                } 
                else 
                {
                    var connection = new SQLiteAsyncConnection(pathToDatabase);
                    Console.WriteLine("DB exist in: " + pathToDatabase);
                    return "Database connected";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while creating database");
                return ex.Message;
            }
        }

        public async Task<string> InsertUpdateData(WeatherObservation data)
        {
            try
            {
                var db = new SQLiteAsyncConnection(pathToDatabase);
                if ((await db.InsertAsync(data)) != 0)
                    await db.UpdateAsync(data);
                Console.WriteLine("Data inserted or updated");
                return "Single data file inserted or updated";
            }
            catch (SQLite.SQLiteException ex)
            {
                Console.WriteLine("Error while inserting");
                return ex.Message;
            }
        }

        public async Task<List<WeatherObservation>> GetDataFromDatabase()
        {
            try
            {
                history.Clear();
                var db = new SQLiteAsyncConnection(pathToDatabase);
                var query = db.Table<WeatherObservation>().Where(v => v.ID > 0);
                await query.ToListAsync().ContinueWith(t => {
                    foreach (var weather in t.Result)
                    {
                        history.Add(weather);
                        Console.WriteLine("Weather addedd CODE: " + weather.countryCode);
                    }
                });
                return history;
            }
            catch (SQLite.SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<WeatherObservation> GetDataFromDatabaseByID(int ID)
        {
            try
            {
                WeatherObservation weatherToReturn = new WeatherObservation();
                var db = new SQLiteAsyncConnection(pathToDatabase);
                var query = db.Table<WeatherObservation>().Where(v => v.ID == ID );
                await query.ToListAsync().ContinueWith(t => {
                    foreach (var weather in t.Result)
                    {
                        weatherToReturn = weather;
                        Console.WriteLine("Weather ID found: " + weather.ID);
                    }
                });
                return weatherToReturn;
            }
            catch (SQLite.SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }


        public async void RemoveDataFromDatabase()
        {
            try
            {
                var db = new SQLiteAsyncConnection(pathToDatabase);
                await db.ExecuteAsync("DELETE FROM WeatherObservation");
                Console.WriteLine("Data deleted from DB");
            }
            catch (SQLite.SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}