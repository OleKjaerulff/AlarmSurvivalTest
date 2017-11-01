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

    [Activity(Label = "AlarmSurvivalTest", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            TimePicker timePicker = FindViewById<TimePicker>(Resource.Id.timePicker);
            Button timePickerDone = FindViewById<Button>(Resource.Id.timePickerDone);

            timePicker.SetIs24HourView(Java.Lang.Boolean.True);
            timePicker.Hour = 24;
            timePicker.Minute = 0;

            timePickerDone.Click += (object sender, EventArgs e) =>
            {
                string date = DateTime.Today.ToShortDateString();
                string hh = timePicker.Hour.ToString("D2");
                string mm = timePicker.Minute.ToString("D2");
                string dateTimeString = date +" "+ hh +":"+ mm +":"+"00.00";
                DateTime dateTime = Convert.ToDateTime(dateTimeString);
                TimeSpan span = dateTime - DateTime.Now;
                long schedule = (long)(Java.Lang.JavaSystem.CurrentTimeMillis() + span.TotalMilliseconds);

                Intent intent = new Intent(this, typeof(MyTestReceiver));
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.CancelCurrent);
                AlarmManager alarmManager = (AlarmManager)this.GetSystemService(Context.AlarmService);
                alarmManager.Set(AlarmType.RtcWakeup, schedule, pendingIntent);
            };
        }
    }

    [BroadcastReceiver(Enabled = true)]
    //no intent filter needed here
    public class MyTestReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast toast = Toast.MakeText(Application.Context, "Hello", ToastLength.Long);
            toast.Show();
        }
    }

}