using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Common.Attributes;
using NadekoBot.Extensions;
using NadekoBot.Modules.BDO.Models;
using NadekoBot.Modules.BDO.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Modules.BDO
{
    public partial class BDO
    {
        public class RNG : NadekoSubmodule<BDOService>
        {
            private DiscordSocketClient _client;


            public RNG(DiscordSocketClient client)
            {
                _client = client;
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task Enchant(string enchantLevel, int failstacks, [Remainder] string itemname = "")
            {
                if (failstacks < 0)
                {
                    await ReplyErrorLocalized("enchant_invalid_failstacks", failstacks).ConfigureAwait(false);
                    return;
                }

                bool enhanceResult = false;
                enchantLevel = enchantLevel.ToLower();
                try
                {
                    enhanceResult = _service.doEnchant(enchantLevel, failstacks);
                }
                catch (Exception)
                {
                    await ReplyErrorLocalized("enchant_incorrect_input", enchantLevel).ConfigureAwait(false);
                    return;
                }

                enchantLevel = _service.EnhanceAppendExtras(enchantLevel);

                if (enhanceResult)
                {
                    if (itemname.Length < 1)
                        await ReplyConfirmLocalized("enchant_success", enchantLevel).ConfigureAwait(false);
                    else
                        await ReplyConfirmLocalized("enchant_success_item", enchantLevel, itemname).ConfigureAwait(false);
                }
                else
                {
                    if (itemname.Length < 1)
                        await ReplyErrorLocalized("enchant_failure", enchantLevel).ConfigureAwait(false);
                    else
                        await ReplyErrorLocalized("enchant_failure_item", enchantLevel, itemname).ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task EnchantUntilSuccess(string enchantLevel, int failstacks, [Remainder] string itemname="")
            {
                if (failstacks < 0)
                {
                    await ReplyErrorLocalized("enchant_invalid_failstacks", failstacks).ConfigureAwait(false);
                    return;
                }
                int startingFailstacks = failstacks;
                if (itemname.Length < 1)
                    itemname = "Generic Item";
                bool enhanceResult = false;
                int totalFails = 0;
                enchantLevel = enchantLevel.ToLower();
                while (!enhanceResult)
                {
                    try
                    {
                        enhanceResult = _service.doEnchant(enchantLevel, failstacks);
                    }
                    catch (Exception)
                    {
                        await ReplyErrorLocalized("enchant_incorrect_input", enchantLevel).ConfigureAwait(false);
                        return;
                    }

                    if(enhanceResult == false)
                    {
                        failstacks += _service.GetFailstackMapping(enchantLevel);
                        totalFails++;
                    }
                }

                enchantLevel = _service.EnhanceAppendExtras(enchantLevel);
                await ReplyConfirmLocalized("enchantuntilsuccess_successful", itemname, enchantLevel,startingFailstacks, failstacks, totalFails);
            }

            [NadekoCommand,Usage,Description, Aliases]
            public async Task EnchantMarathon(string startingenchant, string endingenchant, [Remainder]string itemname = "")
            {
                int startEnc = _service.GetEnchantLevel(startingenchant);
                int endingEnc = _service.GetEnchantLevel(endingenchant);
                int currentEnc = startEnc;
                if (itemname.Length < 1)
                    itemname = "Generic Item";

                if(endingEnc <= startEnc)
                {
                    await ReplyErrorLocalized("enchantmarathon_ending_invalid_value").ConfigureAwait(false);
                    return;
                }

                if(endingEnc< 1)
                {
                    await ReplyErrorLocalized("bdo_enchant_incorrect_input", endingEnc).ConfigureAwait(false);
                    return;
                }
                if (startEnc < 1)
                {
                    await ReplyErrorLocalized("bdo_enchant_incorrect_input", startEnc).ConfigureAwait(false);
                    return;
                }

                List<int> successes = new List<int>();
                List<int> fails = new List<int>();

                for(int i = 0; i < 20; i++)
                {
                    successes.Add(0);
                    fails.Add(0);
                }
                
                while(currentEnc != endingEnc)
                {
                    try
                    {
                        if(_service.doEnchant(currentEnc,500))
                        {
                            successes[currentEnc]++;
                            currentEnc++;                         
                        }
                        else
                        {
                            fails[currentEnc]++;
                            if (currentEnc > 16)
                                currentEnc--;
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }

                int regularblackstones_success = 0;
                int regularblackstones_fail = 0;
                int concentratedblackstones_success = 0;
                int concentratedblackstones_fail = 0;

                int c = 0;

                string finalmsg = "**Enchant Fails:\n**";
                string failmsg_template = "{1} [**{0}**] attempts failed\n";

                foreach(int i in fails)
                {
                    c++;
                    if(i > 0)
                    {
                        string enchantlevel;
                        if (c > 15 && c <= 20)
                            enchantlevel = _service.GetEnchantMappingString(c);
                        else
                            enchantlevel = String.Format("+{0}", c.ToString());

                        finalmsg = string.Concat(finalmsg, string.Format(failmsg_template,enchantlevel,i));
                    }
                }

                c = 0;
                foreach (int i in successes)
                {
                    if (c < 15)
                        regularblackstones_success+=i;
                    else
                        concentratedblackstones_success+=i;
                    c++;
                }

                c = 0;
                foreach (int i in fails)
                {
                    if (c < 15)
                        regularblackstones_fail+=i;
                    else
                        concentratedblackstones_fail+=i;
                    c++;
                }
                string repaircost_string = "\n**Durability loss and repair costs:**\nTotal durability lost: `{0}`\n\n**Repair cost:**\n`{1}` Memory Fragments\n**OR**\n`{2}` Memory Fragments with `{3}` Artisans";
                string blackstonecost_string = "\n\n**Blackstone usage:**\n";

                int durability_used = (regularblackstones_fail * 5) + (concentratedblackstones_fail * 10);

                finalmsg = string.Concat(finalmsg, string.Format(
                    repaircost_string,
                    durability_used,
                    durability_used,
                    durability_used/4,
                    durability_used/4
                    ));

                finalmsg = string.Concat(finalmsg, blackstonecost_string);

                if(regularblackstones_fail + regularblackstones_success > 0)
                {
                    finalmsg = string.Concat(finalmsg, String.Format("`{0}` regular black stones.\n", regularblackstones_fail + regularblackstones_success));
                }

                if(concentratedblackstones_fail + concentratedblackstones_success > 0)
                {
                    finalmsg = string.Concat(finalmsg, String.Format("`{0}` concentrated black stones.\n", concentratedblackstones_fail + concentratedblackstones_success));
                }

                await Context.Channel.EmbedAsync(new EmbedBuilder()
                        .WithTitle(String.Format("{0}'s [{1}: {2}] Enchant results",Context.User.ToString(),
                        _service.EnhanceAppendExtras(endingenchant).ToUpperInvariant(),
                        itemname))
                        .WithOkColor()
                        .WithDescription(finalmsg)
                        );

            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task OpenBox([Remainder]string boxtype)
            {
                string _boxtype = boxtype.ToLowerInvariant();
                if (!_service.VerifyBoxExists(_boxtype))
                {
                    await ReplyErrorLocalized("rng_box_invalid", boxtype).ConfigureAwait(false);
                    return;
                }

                await ReplyConfirmLocalized("rng_box_roll_result", _service.OpenBox(_boxtype).Itemname, boxtype).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task OpenBoxMultiple(int numbox,[Remainder] string boxtype)
            {
                if(numbox < 1)
                {
                    await ReplyErrorLocalized("rng_box_invalid_quantity", numbox).ConfigureAwait(false);
                    return;
                }
                if(numbox > 25000)
                {
                    await ReplyErrorLocalized("rng_box_invalid_quantity", numbox).ConfigureAwait(false);
                    return;
                }

                string _boxtype = boxtype.ToLowerInvariant();
                if (!_service.VerifyBoxExists(_boxtype))
                {
                    await ReplyErrorLocalized("rng_box_invalid", boxtype).ConfigureAwait(false);
                    return;
                }
                await ReplyConfirmLocalized("rng_box_roll_result_multiple", _service.OpenBoxMultiple(_boxtype, numbox)).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task ShowBoxList()
            {
                await ReplyConfirmLocalized("rng_box_list", _service.FetchBoxList()).ConfigureAwait(false);
            }
        }
    }

}
