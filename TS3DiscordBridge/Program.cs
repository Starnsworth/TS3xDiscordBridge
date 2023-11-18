//Discord x Ts3 Bridge by Starns - For shouting at the jips waiting for arma (also can be modded to do more cool stuff maybe?)
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
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
     *  Current thing to do: Take the data in TaskScheduling.requiredDateTime and actually send messages regarding it.
     *      pass the object to some other object in discrodHandler? or do some janky shit where we pass it to disk first? or do a global variable for it omegalul.
     * 
     */




    public class Program
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly FileOperations _fileio;
        private readonly TaskScheduling _taskScheduling;
        private readonly InteractionService _interactionService;

        public Program()
        {
            _services = CreateProvider();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _fileio = _services.GetRequiredService<FileOperations>();
            _taskScheduling = _services.GetRequiredService<TaskScheduling>();
            _interactionService = _services.GetRequiredService<InteractionService>();
        }

        static IServiceProvider CreateProvider()
        {

            var config = new DiscordSocketConfig()
            {
                //GatewayIntents = GatewayIntents.None 
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
                    .AddSingleton<TaskScheduling>()
                    .AddSingleton(interactionConfig)
                    .AddSingleton<InteractionService>()
                    .AddTransient<FileOperations>()
                    .AddTransient<discordHandler>()
                    .AddTransient<UserListComparison>()
                    .AddTransient<SlashCommandModule>()
                    ;
            return collection.BuildServiceProvider();
        }


        //Setting Context for use when needed
        //public static botConfig config;
        //public static DiscordSocketClient client = new DiscordSocketClient();
        //FileOperations fileio = new FileOperations();
        //public static discordHandler discordHandler = new discordHandler();
        //public static TaskScheduling taskScheduling;
        //public static InteractionService interactionService = new InteractionService(client);
        static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();



        public async Task MainAsync(string[] args)
        {
            await instantiateConfigHandler();
            var taskScheduling = _services.GetRequiredService<TaskScheduling>(); //set next instance of requiredDateTime.
            _client.Log += Log;
            _client.InteractionCreated += InteractionCreated;
            _interactionService.Log += Log;
            await _client.LoginAsync(Discord.TokenType.Bot, _services.GetRequiredService<FileOperations>().getBotToken());
            await _client.StartAsync();

            _client.Ready += ClientOnReady;



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
            await _interactionService.RegisterCommandsToGuildAsync(175936015414984704);


        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        } //Log messages from gateway

        private async Task clearRegisteredSlashCommands()
        {
            var guild = _client.GetGuild(175936015414984704);
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