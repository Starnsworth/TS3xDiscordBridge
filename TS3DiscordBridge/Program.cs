//Discord x Ts3 Bridge by Starns - For shouting at the jips waiting for arma (also can be modded to do more cool stuff maybe?)
using Discord;
using Discord.WebSocket;
using System.Text.Json;

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
        //Setting Context for use when needed
        public static botConfigHandler config;
        ts3Handler.serverConnectionData? connectionData;
        bool isconfigured;
        public static DiscordSocketClient client = new DiscordSocketClient();
        FileOperations fileio = new FileOperations();
        public static discordHandler discordHandler = new discordHandler();
        public static TaskScheduling taskScheduling;

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            SlashCommandConstructors x = new SlashCommandConstructors();
            await instantiateConfigHandler();
            taskScheduling = new TaskScheduling();
            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, fileio.getBotToken());
            await client.StartAsync();
            
            client.SlashCommandExecuted += discordHandler.SlashCommandHandler;
            //client.Ready += x.RegisterGuildCommand;
           

            //Block task until closed
            await Task.Delay(-1);
        }

        //public async Task StupidFuckingLoop()
        //{
        //    discordHandler.GetLastMessageAsync();
        //}

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        } //Log messages from gateway

        private bool SetupTS3ConnectionUsingStoredData()
        {   //Retrieve parameters from config stored in memory and store them somewhere we can handle them.
            string[] existingData = config.getSavedTeamspeakServerData();
            string existingHost = existingData[0];
            int existingServerID = int.Parse(existingData[1]);
            int existingChanID = int.Parse(existingData[2]);

            try
            {
                if (connectionData == null)
                { //If connectionData is not instantiated. Create instance of object using the retrieved parameters.
                    connectionData = new ts3Handler.serverConnectionData(existingHost, existingServerID, existingChanID);
                    return true;
                }
                else
                {
                    throw new Exception("connectionData already Setup."); //If instance exists, throw an error.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                return false;
            }
        }
        private async Task clearRegisteredSlashCommands()
        {
            var guild = client.GetGuild(175936015414984704);
            await guild.DeleteApplicationCommandsAsync();
        }
        private async Task instantiateConfigHandler() //Check for valid config. If no valid config, create one for user to populate.
        {

            if (fileio.checkIfConfigExist())
            {
               config = fileio.retrieveStoredConfig();
               isconfigured = true;


                //Console.WriteLine("Printing read-in config");
                //Console.WriteLine("\nStrSavedTeamspeakHostName: " + config.StrSavedTeamspeakHostName +
                //    "\nIntSavedTeamspeakVirtualServerID: " + config.IntSavedTeamspeakVirtualServerID +
                //    "\nStrWatchedDiscordUserID: " + config.StrWatchedDiscordUserID                   +
                //    "\nStrWatchedDiscordUserName: " + config.StrWatchedDiscordUserName               +
                //    "\nStrWatchedDiscordChannelID: " + config.StrWatchedDiscordChannelID             +
                //    "\nStrWatchedDiscordChannelName: " + config.StrWatchedDiscordChannelName         +
                //    "\nlastConfigUpdateFromDisk: "+ config.lastConfigUpdateFromDisk +"\n");                
            }
            else
            {
                await fileio.createConfig();
                isconfigured = false;

            }
        }
    }
}