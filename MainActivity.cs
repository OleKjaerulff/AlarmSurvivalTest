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

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();

            TimePicker timePicker = FindViewById<TimePicker>(Resource.Id.timePicker);
            Button timePickerDone = FindViewById<Button>(Resource.Id.timePickerDone);

            timePicker.SetIs24HourView(Java.Lang.Boolean.True);

            if (prefs.GetString("hour", "") == "") {timePicker.Hour = 0;}
            else timePicker.Hour = int.Parse(prefs.GetString("hour", ""));

            if (prefs.GetString("minute", "") == "") {timePicker.Minute = 0;}
            else timePicker.Minute = int.Parse(prefs.GetString("minute", ""));

            timePickerDone.Click += (object sender, EventArgs e) =>
            {
                string date = DateTime.Today.ToShortDateString();
                string hh = timePicker.Hour.ToString("D2");
                string mm = timePicker.Minute.ToString("D2");
                string dateTimeString = date +" "+ hh +":"+ mm +":"+"00.00";
                DateTime dateTime = Convert.ToDateTime(dateTimeString);

                editor.PutString("dateTimeString", dateTimeString);
                editor.Apply();
                editor.PutString("hour", hh);
                editor.Apply();
                editor.PutString("minute", mm);
                editor.Apply();

                TimeSpan span = dateTime - DateTime.Now;
                long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);

                Intent wake = new Intent(this, typeof(MyTestReceiver));
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, wake, PendingIntentFlags.CancelCurrent);
                AlarmManager alarmManager = (AlarmManager)this.GetSystemService(Context.AlarmService);
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, schedule, pendingIntent);
            };
        }
    }

    [BroadcastReceiver(Enabled = true)]
    //no intent filter needed here
    public class MyTestReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent wake)
        {

            ISharedPreferences receiverprefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = receiverprefs.Edit();
            string text = receiverprefs.GetString("dateTimeString", "");

            Toast toast = Toast.MakeText(Application.Context, text, ToastLength.Long);
            toast.Show();

            MediaPlayer badinerie;
            badinerie = MediaPlayer.Create(context, Resource.Raw.Badinerie);
            badinerie.Start();
           /*
            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
            */
        }
    }
    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted })]
    public class RebootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            /*
            MediaPlayer badinerie;
            badinerie = MediaPlayer.Create(context, Resource.Raw.Badinerie);
            badinerie.Start();
            */
            ISharedPreferences bootreceiverprefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = bootreceiverprefs.Edit();
            string dateTimeString = bootreceiverprefs.GetString("dateTimeString", "");

            //Toast toastfirst = Toast.MakeText(Application.Context, "rebooted", ToastLength.Long);
            //toastfirst.Show();

            //Toast toast = Toast.MakeText(Application.Context, dateTimeString, ToastLength.Long);    
            //toast.Show();

            DateTime dateTime = Convert.ToDateTime(dateTimeString);
            TimeSpan span = dateTime - DateTime.Now;
            long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);

            Intent wake = new Intent(context, typeof(MyTestReceiver));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, wake, PendingIntentFlags.CancelCurrent);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, schedule, pendingIntent);
           
        }
    }
}