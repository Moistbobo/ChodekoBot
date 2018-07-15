using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Modules.BDO.Models
{
    public class PatchnoteItem
    {
        private DateTime _patchdate;
        public DateTime Patchdate
        {
            get=> _patchdate;
            set => _patchdate = value;
        }

        private String _patchlink;
        public String Patchlink
        {
            get => _patchlink;
            set => _patchlink = value;
        }

        private String _title;
        public String Title
        {
            get => _title;
            set => _title = value;
        }

        private int _epochTime;
        public int EpochTime
        {
            get => (int)(_patchdate - new DateTime(1970, 1, 1)).TotalSeconds;
            set => _epochTime = value;
        }
    }
}
