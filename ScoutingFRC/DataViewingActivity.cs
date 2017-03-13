using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "DataViewingActivity")]
    public class DataViewingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewData);
            var bytes = Intent.GetByteArrayExtra("MatchBytes");
            var mStream = new MemoryStream();
            var binFormatter = new BinaryFormatter();
            
            mStream.Write(bytes, 0, bytes.Length);
            mStream.Position = 0;

            var MachList = binFormatter.Deserialize(mStream) as List<MatchData>;

            if (MachList.Count > 0)
            {
                displayData(MachList);
            }

        }

        private void displayData(List<MatchData> datas)
        {
            FindViewById<TextView>(Resource.Id.textViewTeamNumber).Text = datas[0].teamNumber.ToString();
            string matches = "Matches: ";
            int[] gears = new int[4];
            int[] HighGoals = new int[4];
            int[] LowGoals = new int[4];
            foreach (var matchData in datas)
            {
                matches += matchData.match + ", ";

            }
        }

        private double[] divide(int[] ar, int a)
        {
            double[] ret = new double[ar.Length];
            for (int j = 0; j < ar.Length; j++)
            {
                ret[j] = ((double) ar[j])/a;
            }
            return ret;
        }

        private void addScoringMethod(MatchData.PreformaceData.ScoringMethod method, int start, int[] arr)
        {
            
        }
    }
}