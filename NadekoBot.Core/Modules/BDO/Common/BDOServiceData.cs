using NadekoBot.Modules.BDO.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NadekoBot.Modules.BDO.Services
{
    public class BDOServiceData
    {

        const string CPDataPath = "data/contribution_grades.json";
        const string RNGBoxItemPath = "data/BDO_boxes.json";

        private List<float> _enhBaseChance;
        public List<float> enhBaseChance => _enhBaseChance?? (_enhBaseChance = new List<float>
            { 100, 100, 100, 100, 100, 100, 100, 20, 17.5f, 15, 12.5f, 10, 7.5f,
            5, 2.5f, 15, 7.5f, 5, 2, 1.5f});

        private List<float> _enhIncrement;
        public List<float> enhIncrement => _enhIncrement??(_enhIncrement = new List<float>
            {100, 100, 100, 100, 100, 100, 100,2.5f, 2, 1.5f, 1.25f, 0.75f, 0.63f, 0.5f, 0.5f, 1.5f, 0.75f, 0.5f, 0.25f, 0.25f});

        private List<float> _enhChanceCap;
        public List<float> enhChanceCap=> _enhChanceCap??(_enhChanceCap = new List<float>
            {100, 100, 100, 100, 100, 100, 100,52.5f, 42.5f, 37.5f, 32.5f, 23.5f, 20, 17.5f, 15, 52.5f, 33.75f, 27, 25, 20.1f});

        public IReadOnlyDictionary<int, ContributionGrade> CPData = new Dictionary<int, ContributionGrade>();
        public IReadOnlyDictionary<string, RNGBoxItem[]> RNGBoxData = new Dictionary<string, RNGBoxItem[]>();
        public List<int> RNGBoxTotalRolls = new List<int>();

        public BDOServiceData()
        {
            CPData = JsonConvert.DeserializeObject<Dictionary<int, ContributionGrade>>(File.ReadAllText(CPDataPath));
            CPData = CPData.ToDictionary(
                x => x.Key,
                x => x.Value);

          
            RNGBoxData = JsonConvert.DeserializeObject<Dictionary<string, RNGBoxItem[]>>(File.ReadAllText(RNGBoxItemPath));
            RNGBoxData = RNGBoxData.ToDictionary(
                x => x.Key.ToLowerInvariant(),
                x => x.Value);

            SeedRNGBoxes();
        }

        private void SeedRNGBoxes()
        {
            foreach (RNGBoxItem[] rbiList in RNGBoxData.Values)
            {
                float totalRolls = 1;
                foreach(RNGBoxItem rbi in rbiList)
                {
                    rbi.LowValue = (int)totalRolls;
                    totalRolls += rbi.Dropchance * 100;
                    rbi.HighValue = (int)totalRolls;
                    //Console.WriteLine(String.Format("{0} will drop between rolls {1}-{2}", rbi.Itemname, rbi.LowValue, rbi.HighValue));
                }
                RNGBoxTotalRolls.Add((int)totalRolls);
            }
        }
    }
}
