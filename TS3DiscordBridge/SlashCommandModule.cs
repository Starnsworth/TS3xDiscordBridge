using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using System.Text.Json;
using Colour = Discord.Color;
using Summary = Discord.Interactions.SummaryAttribute;

namespace TS3DiscordBridge
{
    public class SlashCommandModule : InteractionModuleBase
    {
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _DiscordSocketClient;
        private readonly botConfig _botConfig;
        private readonly FileOperations _fileio;
        private readonly UserListComparison _userListComparison;
        public SlashCommandModule(DiscordSocketClient discordSocketClient, InteractionService interactionService, botConfig botConfig, FileOperations fileio, UserListComparison userListComparison)
        {
            _DiscordSocketClient = discordSocketClient;
            _interactionService = interactionService;
            _botConfig = botConfig;
            _fileio = fileio;
            _userListComparison = userListComparison;
            
        }

        public class botConfigOptions
        {
            public string teamspeakHostname { get; set; }
            public int teamspeakServerID { get; set; }
            public IUser discordUser { get; set; }
            public IChannel discordWatchChannel { get; set; }
            public IChannel discordShoutChannel { get; set; }

            [ComplexParameterCtor]
            public botConfigOptions(
                [Summary(description: "IP Address or Hostname. eg:'ts.example.com' or '127.0.0.1'")] string TeamspeakServerHostname,
                [Summary(description: "Integer ID of the Virtual Server. Commonly '1'.")] int TeamspeakVirtualServerID,
                [Summary(description: "Discord username to watch")] IUser watchedUser,
                [Summary(description: "Discord Channel for bot to watch")][ChannelTypes(ChannelType.Text)] IChannel watchedChannel,
                [Summary(description: "Discord Channel for bot to yell in.")][ChannelTypes(ChannelType.Text)] IChannel shoutChannel)
            {
                teamspeakHostname = TeamspeakServerHostname;
                teamspeakServerID = TeamspeakVirtualServerID;
                discordUser = watchedUser;
                discordWatchChannel = watchedChannel;
                discordShoutChannel = shoutChannel;
            }
        }

        [SlashCommand("setup-bot-config", "Configure bot to access TS3 server and watch discord channels.")]
        public async Task SetupBotConfig([ComplexParameter] botConfigOptions botConfigOptions)
        {
            await DeferAsync(true); //ack the command but Defer response until actually handled.
            //order = HOSTNAME -> SERVER_ID -> DS_USER -> DS_CHAN -> DS_SHOUTCHAN
            //---------------------------------------------------//
            string newHostname = botConfigOptions.teamspeakHostname;
            int newServerID = Convert.ToInt32(botConfigOptions.teamspeakServerID);


            string newChanHR = botConfigOptions.discordWatchChannel.Name;
            ulong? newChannelID = botConfigOptions.discordWatchChannel.Id;

            string newUsernameHR = botConfigOptions.discordUser.Username;
            ulong newUserID = botConfigOptions.discordUser.Id;

            var shoutChannelHR = botConfigOptions.discordShoutChannel.Name;
            ulong discShoutChannelID = botConfigOptions.discordShoutChannel.Id;
            //-------------------------------------------------//

            string[] configArray = { newHostname, newServerID.ToString(), newChanHR, newChannelID.ToString(), newUsernameHR, newUserID.ToString(), shoutChannelHR, discShoutChannelID.ToString() };
            _botConfig.setConfigValues(configArray);
            await _fileio.DumpConfigToJSON(_botConfig);

            //Build the message that is shown to the user.
            string message = "Updated Settings:\nHostname: " + "`" + _botConfig.StrSavedTeamspeakHostName + "` "
            + "\nTS3 Virtual Server ID: " + "`" + _botConfig.IntSavedTeamspeakVirtualServerID + "`"
            + "\nWatched Discord User: " + "`" + _botConfig.StrWatchedDiscordUserName + "`"
            + "\nWatched User UUID: " + "`" + _botConfig.UlongWatchedDiscordUserID + "`"
            + "\nWatched Channel: " + "`" + _botConfig.StrWatchedDiscordChannelName + "`"
            + "\nWatched Channel UUID: `" + _botConfig.UlongWatchedDiscordChannelID + "`"
            + "\nShout Channel: " + "`" + _botConfig.StrDiscShoutChannel + "`"
            + "\nShout Channel UUID: " + "`" + _botConfig.UlongDiscShoutChannelID + "`";

            //Build the embed thats shown to the user
            var embedBuild = SlashCommandModule.constructEmbedForResponse(message, Colour.Green, "Config Updated Successfully!");
            await ModifyOriginalResponseAsync(x => x.Embed = embedBuild.Build()); //Use original response position to reply to user.

        }

        [SlashCommand("test-command", "Runs a test command.")]
        public async Task testcommand()
        {
            await RespondAsync("Running Test Command.", ephemeral: true);
            await _userListComparison.runUserComparison();
            //await DeferAsync(true);
            //var embedBuild = SlashCommandModule.constructEmbedForResponse("Test DB Connection", Color.Green, "Test DB Connection");
            //await ModifyOriginalResponseAsync(x => x.Embed = embedBuild.Build());
        }




        SlashCommandBuilder settingsFramework2 = new SlashCommandBuilder()
          .WithName("setup-bot-config")
          .WithDescription("Configure bot to access TS3 server and watch discord channels.")
          .AddOption("teamspeak-hostname", Discord.ApplicationCommandOptionType.String, "IP Address or Hostname. eg:'ts.example.com' or '127.0.0.1'", isRequired: true)
          .AddOption("teamspeak-server-id", Discord.ApplicationCommandOptionType.Integer, "Integer ID of the Virtual Server. Commonly '1'.", isRequired: true)
          .AddOption("discord-user", Discord.ApplicationCommandOptionType.User, "Discord username to watch", isRequired: true)
          .AddOption("discord-channel", Discord.ApplicationCommandOptionType.Channel, "Discord Channel for bot to watch", isRequired: true)
          .AddOption("shout-channel", ApplicationCommandOptionType.Channel, "Discord Channel for bot to yell in.", isRequired: true);


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
            .AddOption("minute", ApplicationCommandOptionType.Integer, "Minute to fire the ping.", isRequired: true)
            ;

        public async Task RegisterGuildCommand()
        {
            var guild = _DiscordSocketClient.GetGuild(175936015414984704);
            var slashCommand = settingsFramework2;
            try
            {
                await guild.CreateApplicationCommandAsync(slashCommand.Build()); //build and register the command for use in specific servers. 
                await guild.DeleteApplicationCommandsAsync();
            }
            catch (ApplicationCommandException exception)
            {
                var options = new JsonSerializerOptions { WriteIndented = true, };
                var json = JsonSerializer.Serialize(exception.Errors, options);
                Console.WriteLine(json);
            }
        }

        internal static EmbedBuilder constructEmbedForResponse(string description, Colour colour, string title = "Notice!")
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
