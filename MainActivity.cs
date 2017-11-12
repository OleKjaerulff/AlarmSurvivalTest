
namespace AlarmSurvivalTest
{
    using Android.App;
    using Android.Widget;
    using Android.OS;
    using Android.Content;
    using System;
    using Android.Preferences;
    using Android.Media;
    using System.Threading.Tasks;

    [Activity(Label = "AlarmSurvivalTest", MainLauncher = true)]
    public class MainActivity : Activity
    {
        //protected override void OnCreate(Bundle savedInstanceState)
        protected override void OnCreate(Bundle bundle)
        {
            //base.OnCreate(savedInstanceState);
            base.OnCreate(bundle);

            Toast toast1 = Toast.MakeText(Application.Context, "passed", ToastLength.Long);
            toast1.Show();

            SetContentView(Resource.Layout.Main);
           
        }

        protected override void OnResume()

        {
            base.OnResume();

            TimePicker timePicker = FindViewById<TimePicker>(Resource.Id.timePicker);
            timePicker.SetIs24HourView(Java.Lang.Boolean.True);
            Button timePickerDone = FindViewById<Button>(Resource.Id.timePickerDone);
            Button stopAlarm = FindViewById<Button>(Resource.Id.stopAlarm);
            EditText IntervalField = FindViewById<EditText>(Resource.Id.IntervalField);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            bool alarmTriggered = Intent.GetBooleanExtra("alarmTriggered", false);

            MediaPlayer badinerie;
            badinerie = MediaPlayer.Create(this, Resource.Raw.Badinerie);

            if (alarmTriggered == true)
            {
                alarmTriggered = false;

                badinerie.Start();
            }

            stopAlarm.Click += (object sender, EventArgs e) =>
            {
                badinerie.Stop();
            };

            

            if (prefs.GetString("hour", "") == "") { timePicker.Hour = 0; }
            else timePicker.Hour = int.Parse(prefs.GetString("hour", ""));

            if (prefs.GetString("minute", "") == "") { timePicker.Minute = 0; }
            else timePicker.Minute = int.Parse(prefs.GetString("minute", ""));

            IntervalField.Text = prefs.GetString("intervaltext", "");

            timePickerDone.Click += (object sender, EventArgs e) =>
            {
                string date = DateTime.Today.ToShortDateString();
                string hh = timePicker.Hour.ToString("D2");
                string mm = timePicker.Minute.ToString("D2");
                string dateTimeString = date + " " + hh + ":" + mm + ":" + "00.00";
                DateTime dateTime = Convert.ToDateTime(dateTimeString);

                string intervaltext = IntervalField.Text;
                int interval = int.Parse(intervaltext);

                editor.PutString("hour", hh);
                editor.Apply();
                editor.PutString("minute", mm);
                editor.Apply();
                editor.PutString("dateTimeString", dateTimeString);
                editor.Apply();
                editor.PutString("intervaltext", intervaltext);
                editor.Apply();

                TimeSpan span = dateTime - DateTime.Now;

                long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);
                Intent wake = new Intent(this, typeof(MyTestReceiver));
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, wake, PendingIntentFlags.CancelCurrent);
                AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
                alarmManager.SetInexactRepeating(AlarmType.RtcWakeup, schedule, 1000 * 60 * interval, pendingIntent);

                Toast toast2 = Toast.MakeText(Application.Context, "alarm set", ToastLength.Long);
                toast2.Show();
            };

            bool rebooted = Intent.GetBooleanExtra("rebooted", false);
            if (rebooted == true)
            {
                rebooted = false;
                timePickerDone.PerformClick();
            }
        }
        

        [BroadcastReceiver(Enabled = true, Permission = "RECEIVE_BOOT_COMPLETED")]
        [IntentFilter(new[] {Intent.ActionBootCompleted })]
        public class RebootReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                var intentboot = new Intent(context, typeof(MainActivity));
                intentboot.SetFlags(ActivityFlags.NewTask);
                intentboot.PutExtra("rebooted", true);
                context.StartActivity(intentboot);
            }
        }

        [BroadcastReceiver(Enabled = true, Exported = true)]
        public class MyTestReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent wake)
            {
                var intent = new Intent(context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask);
                intent.PutExtra("alarmTriggered", true);
                context.StartActivity(intent);
            }
        }
        
    }
}