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

        SlashCommandBuilder testTaskScheduling = new SlashCommandBuilder()
            .WithName("test-scheduler")
            .WithDescription("Tests the task scheduler method")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("day")
                .WithDescription("Day of week to test output with.")
                .WithRequired(true)
                .AddChoice("Monday", 1)
                .AddChoice("Tuesday", 2)
                .AddChoice("Wednesday", 3)
                .AddChoice("Thursday", 4)
                .AddChoice("Friday", 5)
                .AddChoice("Saturday", 6)
                .AddChoice("Sunday", 7)
                .WithType(ApplicationCommandOptionType.Integer))
            .AddOption("hour", ApplicationCommandOptionType.Integer, "Hour in 24hr format to fire the ping.", isRequired: true)
            .AddOption("minute", ApplicationCommandOptionType.Integer,"Minute to fire the ping.", isRequired: true)
            ;

        public async Task RegisterGuildCommand()
        {
            var guild = Program.client.GetGuild(175936015414984704);
            var slashCommand = settingsFramework2;
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

        internal static EmbedBuilder constructEmbedForResponse(string description,Color colour, string title = "Notice!")
        {
            var embedBuilder = new EmbedBuilder()
                .WithAuthor("TS3xDiscord Bridge", @"https://cdn.discordapp.com/avatars/144947912063975425/7dad67690c56357e737d9e0c823362bf.webp")
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(colour)
                .WithCurrentTimestamp();
            return embedBuilder;
        }
    }

}
