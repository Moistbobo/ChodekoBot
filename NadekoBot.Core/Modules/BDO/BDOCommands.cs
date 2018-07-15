using Discord;
using Discord.Commands;
using NadekoBot.Common.Attributes;
using NadekoBot.Modules;
using NadekoBot.Modules.BDO.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Modules.BDO
{
    public partial class BDO : NadekoTopLevelModule<BDOService>
    {
        [NadekoCommand, Usage, Description, Aliases]
        public async Task PatchNotes(int depth = 1)
        {
            if (depth < 1)
            {
                await ReplyErrorLocalized("patchnotes_invalid_value", depth).ConfigureAwait(false);
                return;
            }
            else if (depth > 5)
            {
                await ReplyErrorLocalized("patchnotes_over_value").ConfigureAwait(false);
                return;
            }

            await _service.FetchPatchNotes(depth, Context);
        }

        [NadekoCommand, Usage, Description, Aliases]
        public async Task TaxInfo()
        {
            int totalTax = Convert.ToInt32(System.IO.File.ReadAllText(@"data/tax.txt"));

            await ReplyConfirmLocalized("tax_info", totalTax, _bc.BotConfig.CurrencySign).ConfigureAwait(false);
        }
    }

}
