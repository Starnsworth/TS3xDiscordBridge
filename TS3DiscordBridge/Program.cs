//Discord x Ts3 Bridge by Starns - For shouting at the jips waiting for arma (also can be modded to do more cool stuff maybe?)
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TS3DiscordBridge
{

    /*
     * 
     * Discord x TS3 Bridge - For the jips late to ARMA
     * Author: Starns - 2023
     * 
     * Intent: Provides a connection between an enrolled discord server and an appropriately setup TS3 server.
     *          The connection to TS3 is done via SSH on port 10022 and makes use of the below listed TS3 permissions & their powers.
     *          
     * Teamspeak Permissions: b_virtualserver_select,  b_virtualserver_client_list,
     *                        i_channel_subscribe_power =10 , i_channel_needed_subscribe_power = 10
     * 
     *  More Details in README.MD
     *  
     *  Current thing to do: Take the data in OperationTimer.requiredDateTime and actually send messages regarding it.
     *      pass the object to some other object in discrodHandler? or do some janky shit where we pass it to disk first? or do a global variable for it omegalul.
     * 
     */


    //TODO: implement super user settings that can only be implemented in the filesystem.
    //TODO: Guild ID retrieval from the same place as the bot token
    //TODO: Role ID retrival from the same place as the bot token.
    //TODO: Stop users without the correct role running commands they shouldnt.

    //TODO: Impliment a manual alias verification using modals or something.

    public class Program
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly FileOperations _fileio;
        private readonly OperationTimer _taskScheduling;
        private readonly InteractionService _interactionService;
        private readonly adminInternalConfig _adminConfig;

        public Program()
        {
            _services = CreateProvider();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _fileio = _services.GetRequiredService<FileOperations>();
            _taskScheduling = _services.GetRequiredService<OperationTimer>();
            _interactionService = _services.GetRequiredService<InteractionService>();
            _adminConfig = _services.GetRequiredService<adminInternalConfig>();
        }

        static IServiceProvider CreateProvider()
        {

            var config = new DiscordSocketConfig()
            {
                //GatewayIntents = GatewayIntents.none;
            };
            var interactionConfig = new InteractionServiceConfig()
            {

            };
            //var servConfig = new XServiceConfig()
            //{
            //    //...
            //};

            var collection = new ServiceCollection()
                    .AddSingleton(config)
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<botConfig>()
                    .AddSingleton<OperationTimer>()
                    .AddSingleton(interactionConfig)
                    .AddSingleton<InteractionService>()
                    .AddSingleton<adminInternalConfig>()
                    .AddTransient<FileOperations>()
                    .AddTransient<discordHandler>()
                    .AddTransient<UserListComparison>()
                    .AddTransient<SlashCommandModule>()
                    .AddTransient<DatabaseHandler>()
                    ;
            return collection.BuildServiceProvider();
        }


        //Setting Context for use when needed
        //public static botConfig config;
        //public static DiscordSocketClient client = new DiscordSocketClient();
        //FileOperations fileio = new FileOperations();
        //public static discordHandler discordHandler = new discordHandler();
        //public static OperationTimer taskScheduling;
        //public static InteractionService interactionService = new InteractionService(client);
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();



        public async Task MainAsync(string[] args)
        {

            await instantiateConfigHandler();
            var taskScheduling = _services.GetRequiredService<OperationTimer>(); //set next instance of requiredDateTime.
            _client.Log += Log;
            _client.InteractionCreated += InteractionCreated;
            _interactionService.Log += Log;
            await _client.LoginAsync(Discord.TokenType.Bot, _adminConfig.botToken);
            await _client.StartAsync();

            _client.Ready += ClientOnReady;
            _client.SelectMenuExecuted += _services.GetRequiredService<SlashCommandModule>().AliasFlowResponse;


            //Block task until closed
            await Task.Delay(-1);
        }

        private async Task InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_client, arg);
            await _services.GetRequiredService<InteractionService>().ExecuteCommandAsync(context, _services);
        }
        private async Task ClientOnReady()
        {
            //await _clearRegisteredSlashCommands();
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(_adminConfig.discordGuildID));


        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        } //Log messages from gateway

        private async Task clearRegisteredSlashCommands()
        {
            var guild = _client.GetGuild(Convert.ToUInt64(_adminConfig.discordGuildID));
            await guild.DeleteApplicationCommandsAsync();
        }
        private async Task instantiateConfigHandler() //Check for valid config. If no valid config, create one for user to populate.
        {
            if (_fileio.checkIfConfigExist())
            {
                _fileio.retrieveStoredConfig();
            }
            else
            {
                await _fileio.createConfig();

            }
        }
    }
}