using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Commands
{
    public class Default : ModuleBase<SocketCommandContext>
    {
        [Command("안녕")]
        public async Task Hello()
        {
            await ReplyAsync("안녕하세요!");
        }
    }
}
