using Discord;
using Discord.Net;
using Newtonsoft.Json;

namespace TS3DiscordBridge
{
    internal class SlashCommandConstructors
    {
        //Anything that interacts with the 'slash command' registration flow belongs here.

        SlashCommandBuilder settingsFramework2 = new SlashCommandBuilder()
          .WithName("setup-bot-config")
          .WithDescription("Configure bot to access TS3 server and watch discord channels.")
          .AddOption("teamspeak-hostname", Discord.ApplicationCommandOptionType.String, "IP Address or Hostname. eg:'ts.example.com' or '127.0.0.1'", isRequired: true)
          .AddOption("teamspeak-server-id", Discord.ApplicationCommandOptionType.Integer, "Integer ID of the Virtual Server. Commonly '1'.", isRequired: true)
          .AddOption("discord-user", Discord.ApplicationCommandOptionType.User, "Discord username to watch", isRequired: true)
          .AddOption("discord-channel", Discord.ApplicationCommandOptionType.Channel, "Discord Channel for bot to watch", isRequired: true);

        SlashCommandBuilder runMsgFlow = new SlashCommandBuilder()
            .WithName("trigger-get-last-messages")
            .WithDescription("Runs the GetLastMessageAsync command");

        public async Task registerGuildCommand()
        {
            var guild = Program.client.GetGuild(175936015414984704);
            var slashCommand = runMsgFlow;
            try
            {
                await guild.CreateApplicationCommandAsync(slashCommand.Build()); //build and register the command for use in specific servers. 
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }

}
