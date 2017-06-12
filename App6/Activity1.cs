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
using Android.Database;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace App6
{
    [Activity(Label = "History")]
    public class Activity1 : Activity
    {
        WeatherDatabase wdb = new WeatherDatabase();
        ListView listView;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout1);
            List<WeatherObservation> listToView = MainActivity.list;
            List<string> stationNameList = new List<string>();
            listView = FindViewById<ListView>(Resource.Id.listView1);
            Button back = FindViewById<Button>(Resource.Id.buttonBack);
            Button delete = FindViewById<Button>(Resource.Id.buttonDelete);
            stationNameList.Clear();
            foreach (var weather in listToView)
            {
                stationNameList.Add(weather.ID.ToString()+" "+weather.stationName);
                if (stationNameList.Count >= 5)
                    break;
            }
            //ArrayAdapter<WeatherObservation> adapter = new ArrayAdapter<WeatherObservation>(this, Android.Resource.Layout.SimpleListItem1, listToView);
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, stationNameList);
            listView.Adapter = adapter;

            
            listView.ItemClick += async (sender, args) =>
            {
                string stringFromList = stationNameList[args.Position];
                string onlyId = Regex.Match(stringFromList, @"\d+").Value;
                int id = int.Parse(onlyId);
                WeatherObservation fdb = await wdb.GetDataFromDatabaseByID(id);
                //MainActivity.SetWeatherLabelsFromHistory(fdb);
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("MyData", JsonConvert.SerializeObject(fdb));
                Console.WriteLine("Retrieve data: " + stringFromList + " " + id);
                StartActivity(intent);
            };
            back.Click += (sender, args) =>
            {
                OnBackPressed();
            };



            delete.Click += (sender, args) =>
            {
                wdb.RemoveDataFromDatabase();
            };

        }
    }
}