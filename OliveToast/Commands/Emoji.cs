using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("이모지")]
    [RequireCategoryEnable(CategoryType.Emoji)]
    public class Emoji : ModuleBase<SocketCommandContext>
    {
        [Command("이모지 변환")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("글자를 이모지로 바꿔줍니다")]
        public async Task ToEmoji([Remainder, Name("입력")] string input)
        {
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string lower = "abcdefghijklmnopqrstuvwxyz";
            string[] lowerEmojis = { "<:a_:632153577992749056>", "<:b_:632153577564930069>", "<:c_:632153577745285151>",
                "<:d_:632153578009657364>", "<:e_:632153578076635149>", "<:f_:632153577984491520>","<:g_:632153578471030785>",
                "<:h_:632153578043080726>", "<:i_:632153577917382687>","<:j_:632153577942679555>", "<:k_:632153578018177025>",
                "<:l_:632153578081091594>", "<:m_:632153578039017472>", "<:n_:632153578101932032>", "<:o_:632153578030497793>",
                "<:p_:632153578101932042>", "<:q_:632153578076635136>", "<:r_:632153578206789632>", "<:s_:632153578290675712>",
                "<:t_:632153577711730695>", "<:u_:632153578177560586>", "<:v_:632153578185818112>", "<:w_:632153577892216863>",
                "<:x_:632153578026565644>", "<:y_:632153578253058048>", "<:z_:632153578286612481>"};

            string special = "0123456789!?+-/*$#<>";
            string[] specialEmojis = { ":zero:", ":one:", ":two:", ":three:", ":four:",":five:", ":six:", ":seven:", ":eight:",
                ":nine:", ":exclamation:", ":question:", ":heavy_plus_sign:", ":heavy_minus_sign:", ":heavy_division_sign:",
                ":heavy_multiplication_x:", ":heavy_dollar_sign:", ":hash:", ":arrow_backward:", ":arrow_forward:" };

            string result = "";

            for (int num = 0; num < input.Length; num++)
            {
                if (upper.Contains(input[num]))
                {
                    result += $":regional_indicator_{input[num]}:";
                }
                else if (lower.Contains(input[num]))
                {
                    result += lowerEmojis[lower.IndexOf(input[num])];
                }
                else if (input[num] == ' ')
                {
                    result += "    ";
                }
                else if (special.Contains(input[num]))
                {
                    result += specialEmojis[special.IndexOf(input[num])];
                }
                else
                {
                    result += ":no_entry_sign:";
                }
            }

            result = result.ToLower();

            await Context.MsgReplyAsync(result);
        }
    }
}
