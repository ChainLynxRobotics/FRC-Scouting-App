using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "Data Collection", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataCollectionActivity : Activity
    {
        private MatchData matchData;
        private bool autonomous = true;

        private Stack<Action> highGoal = new Stack<Action>();
        private Stack<Action> lowGoal = new Stack<Action>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataCollection);
            matchData = new MatchData();

            FindViewById<Button>(Resource.Id.buttonSubmit).Click += ButtonSubmit_Click;

            FindViewById<Button>(Resource.Id.buttonHighGoal).Click += (sender, args) =>
            {
                AddAttempt(matchData.autonomous.highGoal, matchData.teleoperated.highGoal, true, highGoal);
            };

            FindViewById<Button>(Resource.Id.buttonHighMiss).Click += (sender, args) =>
            {
                AddAttempt(matchData.autonomous.highGoal, matchData.teleoperated.highGoal, false, highGoal);
            };
            FindViewById<Button>(Resource.Id.buttonHighBounce).Click += (sender, args) =>
            {
                AddAttempt(matchData.autonomous.highGoal)
            }

            FindViewById<Button>(Resource.Id.buttonLowGoal).Click += (sender, args) =>
            {
                AddAttempt(matchData.autonomous.lowGoal, matchData.teleoperated.lowGoal, true, lowGoal);
            };

            FindViewById<Button>(Resource.Id.buttonLowMiss).Click += (sender, args) =>
            {
                AddAttempt(matchData.autonomous.lowGoal, matchData.teleoperated.lowGoal, false, lowGoal);
            };

            FindViewById<Switch>(Resource.Id.switchAuto).CheckedChange += OnCheckedChange;

            var name = Intent.GetStringExtra("name");
            if (name != null) {
                FindViewById<TextView>(Resource.Id.editTextYourName).Text = name;
            }

            var number = Intent.GetIntExtra("match", 0);
            if (number != 0) {
                FindViewById<TextView>(Resource.Id.editTextMatchNumber).Text = number.ToString();
            }

            FindViewById<ImageButton>(Resource.Id.buttonUndoHighBoiler).Click += (sender, args) =>
            {
                Undo(highGoal);
            };

            FindViewById<ImageButton>(Resource.Id.buttonUndoLowBoiler).Click += (sender, args) =>
            {
                Undo(lowGoal);
            };
            RedrawLayout();
        }


        /// <summary>
        /// Gets called when the autonomous/teleporated checkbox gets changed
        /// </summary>
        private void OnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            autonomous = FindViewById<Switch>(Resource.Id.switchAuto).Checked;
            using (var line = FindViewById<CheckBox>(Resource.Id.checkBox1)) {
                using (var rope = FindViewById<CheckBox>(Resource.Id.checkBoxClimb)) {
                    line.Enabled = autonomous;
                    rope.Enabled = !autonomous;
                }
            }

            RedrawLayout();
        }

        /// <summary>
        /// Adds a new attempt to the given auto or teleoperated scoring method.
        /// </summary>
        private void AddAttempt(MatchData.PerformanceData.ScoringMethod auto, MatchData.PerformanceData.ScoringMethod tele, bool successful ,Stack<Action> undoList)
        {
            if (autonomous) {
                auto.IncrementAttempt(successful);
                undoList.Push(() => auto.DecrementAttempt(successful));
            }
            else {
                tele.IncrementAttempt(successful);
                undoList.Push(() => tele.DecrementAttempt(successful));
            }

            RedrawLayout();
        }


        /// <summary>
        /// Undoes an action
        /// </summary>
        private void Undo(Stack<Action> pastActions)
        {
            if (pastActions.Count > 0) {
                var action = pastActions.Pop();
                action();
            }

            RedrawLayout();
        }

        /// <summary>
        /// Submit button event handler
        /// </summary>
        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            try {
                matchData.teamNumber = int.Parse(FindViewById<TextView>(Resource.Id.editTextTeamNumber).Text);
            }
            catch {
                ComplainAboutField("a team number");
                return;
            }

            try {
                matchData.match = int.Parse(FindViewById<TextView>(Resource.Id.editTextMatchNumber).Text);
            }
            catch {
                ComplainAboutField("a match number");
                return;
            }

            string name = FindViewById<TextView>(Resource.Id.editTextYourName).Text;
            if (string.IsNullOrEmpty(name)) {
                ComplainAboutField("your name");
                return;
            }

            matchData.autonomous.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBox1).Checked;
            matchData.teleoperated.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBoxMowBar).Checked;
            matchData.teleoperated.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBoxMidBar).Checked;
            matchData.teleoperated.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBoxHighBar).Checked;
            matchData.teleoperated.oneTimePoints = FindViewById<CheckBox>(Resource.Id.checkBoxTraverseBar).Checked;
            matchData.scoutName = name;
            matchData.notes = FindViewById<TextView>(Resource.Id.editTextNotes).Text;

            Intent myIntent = new Intent(this, typeof(MainActivity));

            var bytes = MatchData.Serialize(matchData);
            myIntent.PutExtra("newMatch", bytes);
            SetResult(Result.Ok, myIntent);
            Finish();
        }


        /// <summary>
        /// Refreshes the UI.
        /// </summary>
        private void RedrawLayout()
        {
            FindViewById<TextView>(Resource.Id.textViewAutoHighGoal).Text = "Auto " + matchData.autonomous.highGoal;
            FindViewById<TextView>(Resource.Id.textViewTeleHighGoal).Text = "Tele " + matchData.teleoperated.highGoal;
            FindViewById<TextView>(Resource.Id.textViewAutoLowGoal).Text = "Auto " + matchData.autonomous.lowGoal;
            FindViewById<TextView>(Resource.Id.textViewAutoLowGoal).Text = "Tele " + matchData.teleoperated.lowGoal;
        }

        /// <summary>
        /// Shows a warning for a missing field.
        /// </summary>
        private void ComplainAboutField(string missing)
        {
            var builder = new AlertDialog.Builder(this)
                 .SetTitle("Cannot Submit Match Data")
                 .SetMessage($"You cannot submit match scouting data without {missing}")
                 .SetPositiveButton("Ok", (sender, args) => {});
            builder.Create().Show();
        }
    }
}