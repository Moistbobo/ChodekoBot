using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Modules.BDO.Models
{
   public  class RNGBoxItem
    {
        [JsonProperty("itemname")]
        private String _itemname;
        public String Itemname
        {
            get => _itemname;
            set => _itemname = value;
        }

        [JsonProperty("dropchance")]
        private float _dropchance;
        public float Dropchance
        {
            get => _dropchance;
            set => _dropchance = value;
        }

        [JsonProperty("img")]
        private String _img;
        public String Img
        {
            get => _img;
            set => _img = value;
        }

        private int _lowValue;
        public int LowValue
        {
            get => _lowValue;
            set => _lowValue = value;
        }

        private int _highValue;
        public int HighValue
        {
            get => _highValue;
            set => _highValue = value;
        }
    }
}
