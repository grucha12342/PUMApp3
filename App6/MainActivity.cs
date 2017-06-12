using Android.App;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Linq;
using Android.Util;
using Newtonsoft.Json;
using System.Json;
using Android.Content;
using Java.Util;
using System.Globalization;

namespace App6
{
    [Activity(Label = "WeatherApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        Location currentLocation;
        LocationManager locationManager;
        TextView addressName;
        string locationProvider;
        string lng;
        string lat;
        static readonly string TAG = "X:" + typeof(Activity).Name;
        RootObject obj;
        TextView date;
        TextView station;
        TextView temperature;
        TextView pressure;
        TextView wind;
        Address address;
        Button button;
        WeatherDatabase data;
        public static List<WeatherObservation> list;

        public async void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (currentLocation == null)
            {
                addressName.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                addressName.Text = string.Format("{0:f6},{1:f6}", currentLocation.Latitude, currentLocation.Longitude);
                address = await ReverseGeocodeCurrentLocation();
            }
        }

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            InitializeLocationManager();
            data = new WeatherDatabase();
            await data.CreateDatabase();
            button = FindViewById<Button>(Resource.Id.getWeatherButton);
            Button history = FindViewById<Button>(Resource.Id.buttonHistory);
            addressName = FindViewById<TextView>(Resource.Id.addressName);
            date = FindViewById<TextView>(Resource.Id.textDate);
            station = FindViewById<TextView>(Resource.Id.textStation);
            temperature = FindViewById<TextView>(Resource.Id.textTemp);
            pressure = FindViewById<TextView>(Resource.Id.textPressure);
            wind = FindViewById<TextView>(Resource.Id.textWind);
            if (Intent.GetStringExtra("MyData") != null)
            {
                WeatherObservation xd = JsonConvert.DeserializeObject<WeatherObservation>(Intent.GetStringExtra("MyData"));
                SetWeatherLabelsFromHistory(xd);
            }
            button.Click += AddressButton_OnClick;
            history.Click += async delegate
            {
                list = await data.GetDataFromDatabase();
                list.Reverse();
                StartActivity(typeof(Activity1));
            };

        }

        void InitializeLocationManager()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            locationManager.RemoveUpdates(this);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            date.Text = savedInstanceState.GetString("label");
            station.Text = savedInstanceState.GetString("label2");
            temperature.Text = savedInstanceState.GetString("label3");
            pressure.Text = savedInstanceState.GetString("label4");
            wind.Text = savedInstanceState.GetString("label5");
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try
            {
                outState.PutString("label", date.Text);
                outState.PutString("label2", station.Text);
                outState.PutString("label3", temperature.Text);
                outState.PutString("label4", pressure.Text);
                outState.PutString("label5", wind.Text);
                base.OnSaveInstanceState(outState);
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message); 
            }
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                button.Enabled = false;
                if (currentLocation == null)
                {
                    addressName.Text = "Can't determine the current address. Try again in a few minutes.";
                    button.Enabled = true;
                    return;
                }
                lng = currentLocation.Longitude.ToString(new CultureInfo("en-US"));
                lat = currentLocation.Latitude.ToString(new CultureInfo("en-US"));
                Console.WriteLine(lat + " - " + lng);
                JsonValue value = await GetWeatherAsync("http://api.geonames.org/findNearByWeatherJSON?lat=" + lat + "&lng=" + lng + "&username=grucha12342");
                obj = JsonConvert.DeserializeObject<RootObject>(value.ToString());
                SetWeatherLabels();
                await data.InsertUpdateData(obj.weatherObservation);
                button.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                button.Enabled = true;
            }

        }

        private void SetWeatherLabels()
        {
            if (obj == null)
            {
                Console.Out.WriteLine("Weather is not obtained");
            }
            else
            {
                //Console.Out.WriteLine("Station name : " + obj.weatherObservation.stationName);
                date.Text = "Date: " + obj.weatherObservation.datetime;
                station.Text = "Station: " + obj.weatherObservation.stationName + "; Country: " + obj.weatherObservation.countryCode;
                temperature.Text = "Temperature: " + obj.weatherObservation.temperature;
                pressure.Text = "Pressure: " + obj.weatherObservation.hectoPascAltimeter + "; Humidity: " + obj.weatherObservation.humidity;
                wind.Text = "Wind speed: " + obj.weatherObservation.windSpeed + "; Direction: " + obj.weatherObservation.windDirection;
            }
        }

        public void SetWeatherLabelsFromHistory(WeatherObservation weather)
        {
            if (weather == null)
            {
                Console.Out.WriteLine("Weather is not obtained");
            }
            else
            {
                //Console.Out.WriteLine("Station name : " + obj.weatherObservation.stationName);
                date.Text = "Date: " + weather.datetime;
                station.Text = "Station: " + weather.stationName + "; Country: " + weather.countryCode;
                temperature.Text = "Temperature: " + weather.temperature;
                pressure.Text = "Pressure: " + weather.hectoPascAltimeter + "; Humidity: " + weather.humidity;
                wind.Text = "Wind speed: " + weather.windSpeed + "; Direction: " + weather.windDirection;
            }
        }

        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(currentLocation.Latitude, currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        private async Task<JsonValue> GetWeatherAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            // Send the request to the server and wait for the response:
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());
                    // Return the JSON document:
                    return jsonDoc;
                }
            }
        }
    }
}

