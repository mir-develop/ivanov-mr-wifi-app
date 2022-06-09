using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;
using Android.Content;
using System.Collections.Generic;

namespace IvanovMR_WiFi
{
    [Activity(Label = "IvanovMR_WiFi", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public ListView listview;
        public Button button, enter;
        public EditText password;
        public static List<string> Items;
        private bool start = true;
        private string netSSID;

        WifiManager WifiManager;
        ScanResultBroadcastReceiver m_ScanResultBroadcastReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            button = FindViewById<Button>(Resource.Id.button1);

            enter = FindViewById<Button>(Resource.Id.button2);

            listview = FindViewById<ListView>(Resource.Id.listView1);

            password = FindViewById<EditText>(Resource.Id.editText1);

            password.Visibility = ViewStates.Invisible;
            enter.Visibility = ViewStates.Invisible;

            button.Text = "Начать Поиск!";
            enter.Text = "Подключиться";

            WifiManager = (WifiManager)GetSystemService(WifiService);

            m_ScanResultBroadcastReceiver = new ScanResultBroadcastReceiver();
            m_ScanResultBroadcastReceiver.Receive += M_ScanResultBroadcastReceiver_Receive;

            button.Click += Button_Click;
            enter.Click += Enter_Click;

            listview.ItemClick += Listview_ItemClick;
        }
        void Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            netSSID = Items[e.Position];
            Toast.MakeText(this, "Ты выбрал " + netSSID, ToastLength.Short).Show();
        }
        void Enter_Click(object sender, EventArgs e)
        {
            string netpass = password.Text;

            WifiConfiguration wifiConfig = new WifiConfiguration();
            wifiConfig.Ssid = string.Format("\"{0}\"", netSSID);
            wifiConfig.PreSharedKey = string.Format("\"{0}\"", netpass);

            WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);

            // Use ID
            int netId = wifiManager.AddNetwork(wifiConfig);

            if (start)
            {
                wifiManager.Disconnect();
                wifiManager.EnableNetwork(netId, true);
                wifiManager.Reconnect();

                start = false;
                enter.Text = "Отключиться"; 
            }
            else
            {
                wifiManager.Disconnect();

                start = true;
                enter.Text = "Подключиться";
            }
            
        }
    
        void Button_Click(object sender, EventArgs e)
        {
            RegisterReceiver(m_ScanResultBroadcastReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));
            button.Text = "Идёт поиск...";
            WifiManager.StartScan();

            password.Visibility = ViewStates.Visible;
            enter.Visibility = ViewStates.Visible;
        }

        void M_ScanResultBroadcastReceiver_Receive(Context arg1, Intent arg2)
        {
            var wifiR = WifiManager.ScanResults;
            Items = new List<string>();
            for (int i = 0; i < wifiR.Count; i++)
            {
                if (wifiR[i].Ssid.Length >= 1)
                {
                    int level = wifiR[i].Level;
                    if (level >= -50)
                    {
                        level = 100;
                    }
                    else
                    {
                        if (level <= -100)
                        {
                            level = 0;
                        }
                        else
                        {
                            level = 2 * (level + 100);
                        }
                    }
                    Items.Add(wifiR[i].Ssid/* + ", "  + level + " %"*/);
                }
            }
            listview.Adapter = new ArrayAdapter<string>(arg1, Android.Resource.Layout.SimpleListItem1, Items);
        }

        public class ScanResultBroadcastReceiver : BroadcastReceiver
        {
            public event Action<Context, Intent> Receive;
            public override void OnReceive(Context context, Intent intent)
            {
                if (Receive != null && intent != null && intent.Action == "android.net.wifi.SCAN_RESULTS")
                {
                    Receive(context, intent);
                }
            }
        }
    }
}