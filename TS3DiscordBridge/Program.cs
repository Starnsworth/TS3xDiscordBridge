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
     * 
     * **CORE FUNCTIONALITY **
     * ==DONE: Read Settings from disk using botConfigHandler() and store somewhere==
     * ==DONE: Do discord logon stuff==
     * ==DONE: Check if a message from watched user has been sent in watched channel in last 6 hours ==
     * 
     * function: Sundays & Tuesdays, check in 3x to ensure valid data.
     *      TODO: Timer Functionality to check in at the correct times.
     *      TODO: slash command so staff can create custom times to sound off.
     *      
     * function: Compare users & sound off.      
     * TODO: Method to Build list of users who have reacted to the noti.
     * TODO: Method to build list of users currently in teamspeak - overall presence in the TS is good enough, we dont need a channel ID.
     *      TODO: Parse that huge string that gets returned on 'clientlist -uid' Need Nickname & uid.
     *      TODO: Appropriate storage of information. Guess JSON would work with a properly implimented 'users' class.
     *              but look to migrate to something else. TinyDB? Mongo? maria?
     *              
     * TODO: Compare User lists, Ping discord users where no match is found.
     * 
     * TODO: Cleanup debug behaviour.
     * 
     * Stretch Goals;
     * TODO: Properly Impliment logging, failure states, and exceptions.
     * TODO: Refactor the code so it's nicer... But fuck that.
     * 
     * Really Stretchy Goals (Like Probably not happening);
     * TODO: Impliment a webdashboard to easily relate discord users to teamspeak UUIDs
     * 
     * 
     */




    public class Program
    {
        //Setting Context for use when needed
        public static botConfigHandler? config;
        ts3Handler.serverConnectionData? connectionData;
        bool isconfigured;
        public static DiscordSocketClient client = new DiscordSocketClient();
        FileOperations fileio = new FileOperations();
        discordHandler discordHandler = new discordHandler();


        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            SlashCommandConstructors x = new SlashCommandConstructors();
            instantiateConfigHandler();
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
        private void instantiateConfigHandler() //Check for valid config. If no valid config, create one for user to populate.
        {

            if (fileio.checkIfConfigExist())
            {
               config = fileio.retrieveStoredConfig();
               isconfigured = true;


                Console.WriteLine("Printing read-in config");
                Console.WriteLine("\nStrSavedTeamspeakHostName: " + config.StrSavedTeamspeakHostName +
                    "\nIntSavedTeamspeakVirtualServerID: " + config.IntSavedTeamspeakVirtualServerID +
                    "\nStrWatchedDiscordUserID: " + config.StrWatchedDiscordUserID                   +
                    "\nStrWatchedDiscordUserName: " + config.StrWatchedDiscordUserName               +
                    "\nStrWatchedDiscordChannelID: " + config.StrWatchedDiscordChannelID             +
                    "\nStrWatchedDiscordChannelName: " + config.StrWatchedDiscordChannelName         +
                    "\nlastConfigUpdateFromDisk: "+ config.lastConfigUpdateFromDisk +"\n");                
            }
            else
            {
                fileio.createConfig();
                isconfigured = false;

            }
        }
    }
}