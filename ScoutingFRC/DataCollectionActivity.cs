using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "Data Collection", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataCollectionActivity : Activity
    {
        private MatchData matchData;
        private bool autonomous = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataCollection);
            matchData = new MatchData();
            FindViewById<Button>(Resource.Id.buttonSubmit).Click += ButtonSubmit_Click;
            FindViewById<Button>(Resource.Id.buttonGearGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.gears.successes, ref matchData.teleoperated.gears.successes);
                };
            FindViewById<Button>(Resource.Id.buttonGearMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.gears.failedAttempts,
                        ref matchData.teleoperated.gears.failedAttempts);
                };
            FindViewById<Button>(Resource.Id.buttonHighGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.highBoiler.successes,
                        ref matchData.teleoperated.highBoiler.successes);
                };
            FindViewById<Button>(Resource.Id.buttonHighMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.highBoiler.failedAttempts,
                        ref matchData.teleoperated.highBoiler.failedAttempts);
                };
            FindViewById<Button>(Resource.Id.buttonLowGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.lowBoiler.successes,
                        ref matchData.teleoperated.lowBoiler.successes);
                };
            FindViewById<Button>(Resource.Id.buttonLowMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.lowBoiler.failedAttempts,
                        ref matchData.teleoperated.lowBoiler.failedAttempts);
                };
            FindViewById<Switch>(Resource.Id.switchAuto).CheckedChange += OnCheckedChange;
            var name = Intent.GetStringExtra("name");
            if (name != null)
            {
                FindViewById<TextView>(Resource.Id.editTextYourName).Text = name;
            }
            var number = Intent.GetIntExtra("match", 0);
            if (number != 0)
            {
                FindViewById<TextView>(Resource.Id.editTextMathcNumber).Text = number.ToString();
            }
            drawTings();
        }

        private void OnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            autonomous = FindViewById<Switch>(Resource.Id.switchAuto).Checked;
            using (var line = FindViewById<CheckBox>(Resource.Id.checkBox1))
            using (var rope = FindViewById<CheckBox>(Resource.Id.checkBoxClimb))
            {
                line.Enabled = autonomous;
                rope.Enabled = !autonomous;
            }
            drawTings();
        }

        private void addAttempt(ref int auto, ref int tele)
        {

            if (autonomous)
            {
                auto++;
            }
            else
            {
                tele++;
            }
            drawTings();
        }

        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                matchData.teamNumber = int.Parse(FindViewById<TextView>(Resource.Id.editTextTeamNumber).Text);
            }
            catch (Exception)
            {
                ComplainAboutFeild("a team number");
                return;
            }

            try
            {
                matchData.match = int.Parse(FindViewById<TextView>(Resource.Id.editTextMathcNumber).Text);
            }
            catch (Exception)
            {
                ComplainAboutFeild("a match number");
                return;
            }

            string name = FindViewById<TextView>(Resource.Id.editTextYourName).Text;
            if (string.IsNullOrEmpty(name))
            {
                ComplainAboutFeild("your name");
                return;
            }
            matchData.automomous.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBox1).Checked;
            matchData.teleoperated.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBoxClimb).Checked;
            matchData.scoutName = name;
            matchData.notes = FindViewById<TextView>(Resource.Id.editTextNotes).Text;

            Intent myIntent = new Intent(this, typeof(MainActivity));

            var bytes = MatchData.Serialize(matchData);
            myIntent.PutExtra("W", bytes);
            SetResult(Result.Ok, myIntent);
            Finish();

        }

        private void drawTings()
        {
            if (autonomous)
            {
                updateCounts(matchData.automomous);
            }
            else
            {
                updateCounts(matchData.teleoperated);
            }
        }

        private void updateCounts(MatchData.PerformanceData data)
        {
            FindViewById<TextView>(Resource.Id.textView4).Text =
                $"{data.highBoiler.successes}/{data.highBoiler.failedAttempts + data.highBoiler.successes}";
            FindViewById<TextView>(Resource.Id.textView5).Text =
                $"{data.lowBoiler.successes}/{data.lowBoiler.failedAttempts + data.lowBoiler.successes}";
            FindViewById<TextView>(Resource.Id.textView6).Text =
                $"{data.gears.successes}/{data.gears.failedAttempts + data.gears.successes}";
        }

        private void ComplainAboutFeild(string missing)
        {
            var builder = new AlertDialog.Builder(this)
                 .SetMessage($"You cannot submit match scouting data without {missing}")
                 .SetPositiveButton("Ok", (sender, args) => {});
            builder.Create().Show();
        }
    }
}