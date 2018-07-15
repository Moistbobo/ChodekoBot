using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Common.Attributes;
using NadekoBot.Modules.BDO.Models;
using NadekoBot.Modules.BDO.Services;
using System.Threading.Tasks;

namespace NadekoBot.Modules.BDO
{
    public partial class BDO
    {
        [Group]
        public class QuickMaffs: NadekoSubmodule<BDOService>
        {
            private DiscordSocketClient _client;

            public QuickMaffs(DiscordSocketClient client)
            {
                _client = client;
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task CalcTradeInForLevel(int contributionLevel)
            {
                if(contributionLevel <= 1)
                {
                    await ReplyErrorLocalized("contribution_invalid_value").ConfigureAwait(false);
                    return;
                }

                int contributionStage = _service.MapContributionLevel(contributionLevel);

                if (contributionStage == -1)
                {
                    await ReplyErrorLocalized("contribution_invalid_mapping").ConfigureAwait(false);
                    return;
                }

                ContributionGrade cg = _service.GetContributionGrade(contributionStage);
                await ReplyConfirmLocalized("contribution_tradeinsforlevel", System.String.Format("{0:n0}", cg.TurninPerCP), contributionLevel, contributionLevel + 1).ConfigureAwait(false);      
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task CalcTradeInForLevelRange(int startcp, int targetcp)
            {
                if(startcp < 1 || targetcp <= 1)
                {
                    await ReplyErrorLocalized("contribution_invalid_value_range").ConfigureAwait(false);
                    return;
                }

                if(targetcp <= startcp)
                {
                    await ReplyErrorLocalized("contribution_target_lessthanorequaltostart").ConfigureAwait(false);
                    return;
                }

                int tradeinsNeeded = _service.CalculateNumTradeInsContribution(startcp, targetcp);

                await ReplyConfirmLocalized("contribution_tradeinsforlevel", System.String.Format("{0:n0}", tradeinsNeeded), startcp, targetcp).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task Value(float saleprice)
            {
                if (saleprice <= 0)
                {
                    await ReplyErrorLocalized("value_invalid_value", saleprice).ConfigureAwait(false);
                    return;
                }

                await ReplyConfirmLocalized("value_calculate_sales", System.String.Format("{0:n0}", saleprice), System.String.Format("{0:n0}", (saleprice * 0.65f)), System.String.Format("{0:n0}", (saleprice * 0.65f) * 1.3f)).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task EnergyRegen(int hours = 0, [Remainder]int minutes = 0)
            {
                if ((hours == 0 && minutes == 0) ||
                    hours < 0 ||
                    minutes < 0)
                {
                    await ReplyErrorLocalized("energy_invalid_value").ConfigureAwait(false);
                    return;
                }

                int numEnergyCycles = _service.CalculateNumEnergyRegen(minutes + (hours * 60));

                if (hours == 0)
                {
                    await ReplyConfirmLocalized("energy_calculated_value_minutesonly", minutes, numEnergyCycles, numEnergyCycles * 3).ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("energy_calculated_value", hours, minutes, numEnergyCycles, numEnergyCycles * 3).ConfigureAwait(false);
                }
            }
        }
    }

}
