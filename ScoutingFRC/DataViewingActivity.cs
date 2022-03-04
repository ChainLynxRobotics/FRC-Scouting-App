using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "Data Viewing", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataViewingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewData);
            var bytes = Intent.GetByteArrayExtra("MatchBytes");
            List<TeamData> MatchList = MatchData.Deserialize<List<TeamData>>(bytes);

            if (MatchList.Count > 0) {
                DisplayData(MatchList);
            }
        }
        
        /// <summary>  
        ///  Update user interface with Team and Match Data
        /// </summary> 
        private void DisplayData(List<TeamData> datas)
        {
            FindViewById<TextView>(Resource.Id.textViewTeamNumber).Text = datas[0].teamNumber.ToString();
            int count = datas.Count;
            string matches = "";
            int matchCount = 0;
            double HighGoals = 0;
            double LowGoals = 0;
            double autoLine = 0;
            double humanPlayer = 0;
            double lowBar = 0;
            double midBar = 0;
            double highBar = 0;
            double traverseBar = 0;

            foreach (var teamData in datas) {
                if (!(teamData is MatchData)) {
                    continue;
                }
                matchCount++;
                MatchData matchData = teamData as MatchData;
                matches += matchData.match + ", ";
                if (matchData.autonomous.oneTimePoints) {
                    autoLine++;
                    humanPlayer++;
                }
                if (matchData.teleoperated.oneTimePoints) {
                    lowBar++;
                    midBar++;
                    highBar++;
                    traverseBar++;
                }
                AddScoringMethod(matchData.autonomous.highGoal, 0, HighGoals);
                AddScoringMethod(matchData.teleoperated.highGoal, 2, HighGoals);
                AddScoringMethod(matchData.autonomous.lowGoal, 0, LowGoals);
                AddScoringMethod(matchData.teleoperated.lowGoal, 2, LowGoals);
            }

            double high = (HighGoals / matchCount);
            double low = (LowGoals / matchCount);
            double human = (humanPlayer / matchCount);
            double autoLinePercentage = (autoLine / matchCount) * 100;
            double lowBarPercentage = (lowBar / matchCount) * 100;
            double midBarPercentage = (midBar / matchCount) * 100;
            double highBarPercentage = (highBar / matchCount) * 100;
            double traverseBarPercentage = (traverseBar / matchCount) * 100;

            UpdateTextView(Resource.Id.textViewAutoLine, $"Taxi - {Math.Round(autoLinePercentage,2)}%", autoLinePercentage);
            UpdateTextView(Resource.Id.textViewAutoHumanPlayer, $"Human Player - {Math.Round(human*100, 2)}%", human);
            UpdateTextView(Resource.Id.textViewAutoHG, $"High Goals - {Math.Round(high, 2)}", high);
            UpdateTextView(Resource.Id.textViewAutoLG, $"Low Goals - {Math.Round(low, 2)}", low);

            UpdateTextView(Resource.Id.textViewTeleHG, $"High Goals - {Math.Round(high, 2)}/{Math.Round(high, 2)}", high);
            UpdateTextView(Resource.Id.textViewTeleLG, $"Low Goals - {Math.Round(low, 2)}/{Math.Round(low, 2)}", low);
            UpdateTextView(Resource.Id.textViewlowBarView, $"Low Bar - {Math.Round(lowBarPercentage,2)}%", lowBarPercentage);
            UpdateTextView(Resource.Id.textViewMidBarView, $"Mid Bar - {Math.Round(midBarPercentage,2)}%", midBarPercentage);
            UpdateTextView(Resource.Id.textViewHighBarView, $"High Bar - {Math.Round(highBarPercentage,2)}%", highBarPercentage);
            UpdateTextView(Resource.Id.textViewTraverseBarView, $"Traverse - {Math.Round(traverseBarPercentage,2)}%", traverseBarPercentage);

            if (matchCount > 0) {
                FindViewById<TextView>(Resource.Id.textView1).Text = ((matchCount>1)? "Matches: " : "Match: ") + matches.Substring(0, matches.Length - 2);
                double autoPoints = (autoLine * 2) + (humanPlayer * 4) + (high * 4) + (low * 2);
                double telePoints = (high * 2) + low + (lowBar * 4) + (midBar * 6) + (highBar * 10) + (traverseBar * 14);
                FindViewById<TextView>(Resource.Id.textViewAutoPts).Text = Math.Round(autoPoints, 3) + " pts";
                FindViewById<TextView>(Resource.Id.textViewTelePts).Text = Math.Round(telePoints, 3) + " pts";

            }
            else {
                FindViewById<TextView>(Resource.Id.textView1).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutAuto).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutTele).Visibility = ViewStates.Gone;
            }

            LinearLayout.LayoutParams textViewLayout = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            foreach (var teamData in datas)  {
                if (!string.IsNullOrEmpty(teamData.notes)) {
                    String note = ($"\"{teamData.notes}\" - {teamData.scoutName}");
                    TextView text = new TextView(this);
                    text.LayoutParameters = textViewLayout;
                    text.Text = note;
                    FindViewById<LinearLayout>(Resource.Id.linearLayoutListNotes).AddView(text);
                }
            }
        }

        /// <summary>  
        ///  given a string and an ID sets that TextView to that string
        /// </summary> 
        private void UpdateTextView(int id, String value, double visible)
        {
            using (TextView textView = FindViewById<TextView>(id)) {
                if (visible > 0) {
                    textView.Text = value;
                }
                else {
                    textView.Visibility = ViewStates.Gone;
                }
            }
        }

        /// <summary>  
        /// divide every element in an array by a given int.
        /// </summary> 
        private double[] DivideArray(int[] ar, int a)
        {
            double[] result = new double[ar.Length];
            for (int j = 0; j < ar.Length; j++) {
                result[j] = ((double) ar[j])/a;
            }
            return result;
        }
        
        /// <summary>  
        ///  add that successes and failure to a specifide index in an array
        /// </summary> 
        private void AddScoringMethod(MatchData.PerformanceData.ScoringMethod method, int start, int[] arr)
        {
            arr[start] += method.successes;
            arr[start + 1] += method.failedAttempts + method.successes;
        }
    }
}
