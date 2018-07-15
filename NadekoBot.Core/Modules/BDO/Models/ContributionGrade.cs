using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Modules.BDO.Models
{
    public class ContributionGrade
    {
        [JsonProperty("lastrange")]
        private int _lastRange;
        public int LastRange
        {
            get => _lastRange;
            set => _lastRange = value;
        }

        [JsonProperty("xpperlevel")]
        private int _xpPerLevel;
        public int XpPerLevel
        {
            get => _xpPerLevel;
            set => _xpPerLevel = value;
        }


        [JsonProperty("turninpercp")]
        private int _turninPerCP;
        public int TurninPerCP
        {
            get => _turninPerCP;
            set => _turninPerCP = value;
        }

        [JsonProperty("totalturnins")]
        private int _totalTurnIns;
        public int TotalTurnIns
        {
            get => _totalTurnIns;
            set => _totalTurnIns = value;
        }
    }
}
