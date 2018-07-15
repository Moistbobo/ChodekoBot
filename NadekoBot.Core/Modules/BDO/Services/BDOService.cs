using NadekoBot.Core.Services;
using System;
using System.Collections.Generic;
using Microsoft.SyndicationFeed;
using System.Text;
using System.Xml;
using Microsoft.SyndicationFeed.Rss;
using System.Threading.Tasks;
using Discord;
using NadekoBot.Extensions;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using System.Text.RegularExpressions;
using NadekoBot.Modules.BDO.Models;

namespace NadekoBot.Modules.BDO.Services
{
    public class BDOService : INService
    {
        private BDOServiceData _data = new BDOServiceData();
        private List<PatchnoteItem> cachedPatchnotes = new List<PatchnoteItem>();
        private DateTime cachedTime = DateTime.Now;

        public Boolean doEnchant(string enhanceLevel, int failstacks)
        {
            int enhLvl = GetEnchantLevel(enhanceLevel);
            float ebc = _data.enhBaseChance[enhLvl - 1];
            float ei = _data.enhIncrement[enhLvl - 1];
            float ecc = _data.enhChanceCap[enhLvl - 1];

            return enchant(ebc, ei, ecc, failstacks);
        }

        public Boolean doEnchant(int enhanceLevel, int failstacks)
        {
            float ebc = _data.enhBaseChance[enhanceLevel - 1];
            float ei = _data.enhIncrement[enhanceLevel - 1];
            float ecc = _data.enhChanceCap[enhanceLevel - 1];

            return enchant(ebc, ei, ecc, failstacks);
        }

        public string EnhanceAppendExtras(string enhanceLevel)
        {
            if (enhanceLevel.Equals("pri"))
                return "PRI I";
            else if (enhanceLevel.Equals("duo"))
                return "DUO II";
            else if (enhanceLevel.Equals("tri"))
                return "TRI III";
            else if (enhanceLevel.Equals("tet"))
                return "TET IV";
            else if (enhanceLevel.Equals("pen"))
                return "PEN V";
            else if (!enhanceLevel.Substring(0, 1).Equals("+"))
                return String.Concat("+", enhanceLevel);
            else
                return enhanceLevel;
        }

        public int GetFailstackMapping(string enhanceLevel)
        {
            if (enhanceLevel.Equals("pri"))
                return 2;
            else if (enhanceLevel.Equals("duo"))
                return 3;
            else if (enhanceLevel.Equals("tri"))
                return 4;
            else if (enhanceLevel.Equals("tet"))
                return 5;
            else if (enhanceLevel.Equals("pen"))
                return 6;
            else if (!enhanceLevel.Substring(0, 1).Equals("+"))
                return 1;
            else
                return 0;
        }

        public string GetEnchantMappingString(int enchant_level)
        {
            switch (enchant_level)
            {
                case (20):
                    return "PEN V";
                case (19):
                    return "TET IV";
                case (18):
                    return "TRI III";
                case (17):
                    return "DUO II";
                case (16):
                    return "PRI I";
                default:
                    return "error_enchant";
            }
        }

        public async Task FetchPatchNotes(int depth, ICommandContext context)
        {

            XmlReader reader = XmlReader.Create("https://community.blackdesertonline.com/index.php?forums/patch-notes.5/index.rss", new XmlReaderSettings() { Async = true });
            var feedReader = new RssFeedReader(reader);

            var embed = new EmbedBuilder()
                                .WithAuthor(String.Format("Past {0} patch notes", depth))
                                .WithOkColor();

            if (cachedPatchnotes.Count == 0 || (DateTime.Now - cachedTime).TotalMinutes > 10 )
            {
                int counter = 0;
                int limit = 10;
                List<PatchnoteItem> patchnotesList = new List<PatchnoteItem>();
                while (await feedReader.Read() && counter != limit)
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            ISyndicationItem item = await feedReader.ReadItem();
                            // Load all the patchnotes and stuff them into a list
                            List<ISyndicationLink> link = item.Links.ToList();

                            PatchnoteItem _patchnotes = new PatchnoteItem();
                            _patchnotes.Patchdate = DateTime.Parse(new Regex(@"(?<=\d)[a-z]{2}").Replace(item.Title.Substring(item.Title.IndexOf('-') + 1).Replace("[UPDATED]","").Trim(), ""));
                            _patchnotes.Patchlink = link[0].Uri.ToString();
                            _patchnotes.Title = item.Title;
                            patchnotesList.Add(_patchnotes);
                            counter++;
                            break;
                    }
                }

                List<PatchnoteItem> sortedPatchnotes = patchnotesList.OrderByDescending(x => x.EpochTime).ToList();
                cachedPatchnotes = sortedPatchnotes.ToList();
            }
            for (int i = 0; i < depth; i++)
            {
                embed.Description = String.Concat(embed.Description, "\n**", cachedPatchnotes[i].Title, "**\n", cachedPatchnotes[i].Patchlink,"\n");
            }
            await context.Channel.EmbedAsync(embed).ConfigureAwait(false);
        }

        public int CalculateNumEnergyRegen(int minutes)
        {
            return minutes / 3;
        }

        public int MapContributionLevel(int contributionLevel)
        {
            if (contributionLevel >= 400)
                return 19;

            foreach (ContributionGrade cg in _data.CPData.Values.ToList())
            {
                if (contributionLevel < cg.LastRange)
                    return _data.CPData.Values.ToList().IndexOf(cg);
            }
            return -1;
        }

        public ContributionGrade GetContributionGrade(int contributionStage)
        {
            ContributionGrade cg = new ContributionGrade();
            _data.CPData.TryGetValue(contributionStage, out cg);
            return cg;
        }

        public int CalculateNumTradeInsContribution(int startcp, int targetcp)
        {
            int turnins = 0;
            for(int i = startcp; i < targetcp; i++)
            {
                turnins += GetContributionGrade(MapContributionLevel(i)).TurninPerCP;
            }

            return turnins;         
        }

        // RNG Boxes
        public bool VerifyBoxExists(string boxtype)
        {
            foreach(string key in _data.RNGBoxData.Keys.ToList())
            {
                if (key.Contains(boxtype))
                    return true;
            }
            return false;
        }

        public RNGBoxItem OpenBox(string boxtype)
        {
            RNGBoxItem[] trngboxArray;
            _data.RNGBoxData.TryGetValue(boxtype, out trngboxArray);
            Random r = new Random();
            int roll = r.Next(1, _data.RNGBoxTotalRolls[_data.RNGBoxData.Keys.ToList().IndexOf(boxtype)]);

            List<RNGBoxItem> itemlist = trngboxArray.ToList();
            return itemlist.FirstOrDefault(x => x.LowValue <= roll && x.HighValue > roll);
        }

        public string OpenBoxMultiple(string boxtype, int numbox)
        {
            Dictionary<RNGBoxItem, int> rollResults = new Dictionary<RNGBoxItem, int>();
            
            for(int i = 0; i < numbox; i++)
            {
                RNGBoxItem rolleditem = OpenBox(boxtype);
                bool found = false;
                foreach (RNGBoxItem item in rollResults.Keys.ToList())
                {
                    if (item.Itemname.Equals(rolleditem.Itemname))
                    {
                        rollResults[rolleditem] += 1;
                        found = true;
                    }
                }
                if (!found)
                    rollResults.Add(rolleditem, 1);
            }

            Dictionary<RNGBoxItem, int> sortedRolls = rollResults.OrderBy(x => x.Key.Dropchance).ToDictionary(x=>x.Key, x=>x.Value);

            string retstr = "";
            foreach (RNGBoxItem s in sortedRolls.Keys.ToList())
            {
                retstr = String.Concat(retstr, String.Format("**x{0}**: {1}\n", rollResults[s], s.Itemname));
            }

            return retstr;
        }

        public string FetchBoxList()
        {
            string retstr = "";
            foreach(string key in _data.RNGBoxData.Keys.ToList())
            {
                retstr = String.Concat(retstr+key + "\n");
            }
            return retstr;
        }

        private bool enchant(float baseChance, float increment, float cap, int failstacks)
        {
            float chance = Clamp(
                baseChance + (failstacks * increment),
                baseChance,
                cap);

            Random r = new Random();
            float chc = (float)r.NextDouble() * 100;
            if (chc <= chance)
                return true;
            else
                return false;
        }

        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public int GetEnchantLevel(string enchantlevel)
        {
            if (enchantlevel.Equals("pen"))
                return 20;
            else if (enchantlevel.Equals("tet"))
                return 19;
            else if (enchantlevel.Equals("tri"))
                return 18;
            else if (enchantlevel.Equals("duo"))
                return 17;
            else if (enchantlevel.Equals("pri"))
                return 16;
            else
            {
                enchantlevel.Replace("+", "");
                return Convert.ToInt32(enchantlevel);
            }
        }

        
    }
}
