using Android.App;
using Android.Widget;
using Android.OS;

namespace AlarmSurvivalTest
{

    using Android.App;
    using Android.Widget;
    using Android.OS;
    using Android.Content;
    using System;
    using Android.Media;
    using Android.Content.PM;
    using Android.Preferences;

    [Activity(Label = "AlarmSurvivalTest", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            TimePicker timePicker = FindViewById<TimePicker>(Resource.Id.timePicker);
            Button timePickerDone = FindViewById<Button>(Resource.Id.timePickerDone);
            Button stopAlarm = FindViewById<Button>(Resource.Id.stopAlarm);
            EditText IntervalField = FindViewById<EditText>(Resource.Id.IntervalField);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            bool alarmTriggered = prefs.GetBoolean("alarmTriggered", false);

            MediaPlayer badinerie;
            badinerie = MediaPlayer.Create(this, Resource.Raw.Badinerie);

            if (alarmTriggered == true){ 
            badinerie.Start();
                editor.PutBoolean("alarmTriggered", false);
                editor.Apply();
            }

            timePicker.SetIs24HourView(Java.Lang.Boolean.True);

            if (prefs.GetString("hour", "") == "") {timePicker.Hour = 0;}
            else timePicker.Hour = int.Parse(prefs.GetString("hour", ""));

            if (prefs.GetString("minute", "") == "") {timePicker.Minute = 0;}
            else timePicker.Minute = int.Parse(prefs.GetString("minute", ""));

            IntervalField.Text = prefs.GetInt("interval", 0).ToString();

        

            timePickerDone.Click += (object sender, EventArgs e) =>
            {
                string date = DateTime.Today.ToShortDateString();
                string hh = timePicker.Hour.ToString("D2");
                string mm = timePicker.Minute.ToString("D2");
                string dateTimeString = date +" "+ hh +":"+ mm +":"+"00.00";
                DateTime dateTime = Convert.ToDateTime(dateTimeString);

                int interval = int.Parse(IntervalField.Text);

                editor.PutString("hour", hh);
                editor.Apply();
                editor.PutString("minute", mm);
                editor.Apply();
                editor.PutString("dateTimeString", dateTimeString);
                editor.Apply();
                editor.PutInt("interval", interval);
                editor.Apply();

                TimeSpan span = dateTime - DateTime.Now;

                //Toast toast = Toast.MakeText(Application.Context, span.ToString(), ToastLength.Long);
                //toast.Show();

                long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);
                Intent wake = new Intent(this, typeof(MyTestReceiver));
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, wake, PendingIntentFlags.CancelCurrent);
                AlarmManager alarmManager = (AlarmManager)this.GetSystemService(Context.AlarmService);
                alarmManager.SetInexactRepeating(AlarmType.RtcWakeup, schedule, 1000 * 60 * interval, pendingIntent);
            };

            stopAlarm.Click += (object sender, EventArgs e) =>
            {
                badinerie.Stop();
            };

            
        }
    }
    
    [BroadcastReceiver(Enabled = true)]
    public class MyTestReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent wake)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("alarmTriggered", true);
            editor.Apply();

            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted })]
    public class RebootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            string dateTimeString = prefs.GetString("dateTimeString", "");
            int interval = prefs.GetInt("interval", 0);

            editor.PutBoolean("alarmTriggered", true);
            editor.Apply();

            //Toast toastfirst = Toast.MakeText(Application.Context, "rebooted", ToastLength.Long);
            //toastfirst.Show();

            DateTime dateTime = Convert.ToDateTime(dateTimeString);
            TimeSpan span = dateTime - DateTime.Now;

            long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);
            Intent wake = new Intent(context, typeof(MyTestReceiver));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, wake, PendingIntentFlags.CancelCurrent);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            alarmManager.SetInexactRepeating(AlarmType.RtcWakeup, schedule, 1000 * 60 * interval, pendingIntent);
        }
    }
}