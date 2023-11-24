using System.Net.Security;
using System.Text.Json;

namespace TS3DiscordBridge
{
    public class botConfig
    {
        //Just config variables.
        //private readonly botConfig _botConfig;

        private string strSavedTeamspeakHostName;
        private int intSavedTeamspeakVirtualServerID;
        private string strWatchedDiscordUserName;
        private ulong ulongWatchedDiscordUserID;
        private string strWatchedDiscordChannelName;
        private ulong ulongWatchedDiscordChannelID;
        private string strDiscShoutChannel;
        private ulong ulongDiscShoutChannelID;
        private DateTime lastConfigUpdateFromDisk;

        public string StrSavedTeamspeakHostName
        {
            get { return strSavedTeamspeakHostName; }
            set { strSavedTeamspeakHostName = value; }
        }
        public int IntSavedTeamspeakVirtualServerID
        {
            get { return intSavedTeamspeakVirtualServerID; }
            set { intSavedTeamspeakVirtualServerID = value; }

        }
        public string StrWatchedDiscordUserName
        {
            get { return strWatchedDiscordUserName; }
            set { strWatchedDiscordUserName = value; }
        }
        public ulong UlongWatchedDiscordUserID
        {
            get { return ulongWatchedDiscordUserID; }
            set { ulongWatchedDiscordUserID = value; }
        }
        public string StrWatchedDiscordChannelName
        {
            get { return strWatchedDiscordChannelName; }
            set { strWatchedDiscordChannelName = value; }
        }
        public ulong UlongWatchedDiscordChannelID
        {
            get { return ulongWatchedDiscordChannelID; }
            set { ulongWatchedDiscordChannelID = value; }
        }
        public string StrDiscShoutChannel
        {
            get { return strDiscShoutChannel; }
            set { strDiscShoutChannel = value; }
        }
        public ulong UlongDiscShoutChannelID
        {
            get { return ulongDiscShoutChannelID; }
            set { ulongDiscShoutChannelID = value; }
        }

        public DateTime LastConfigUpdateFromDisk
        {
            get { return lastConfigUpdateFromDisk; }
            set { lastConfigUpdateFromDisk = value; }
        }


        public botConfig()
        {
            strSavedTeamspeakHostName ??= "DefaultHostName";
            if (intSavedTeamspeakVirtualServerID <= 0) { intSavedTeamspeakVirtualServerID = -1;}
            strWatchedDiscordUserName ??= "defaultDiscordUserName";
            if (ulongWatchedDiscordUserID == 0){ulongWatchedDiscordUserID = ulong.MaxValue;}
            strWatchedDiscordChannelName ??= "defaultDiscordChannelName";
            if (ulongWatchedDiscordChannelID == 0) { ulongWatchedDiscordChannelID = ulong.MaxValue; }
            strDiscShoutChannel ??= "defaultDiscordShoutChannel";
            if (ulongDiscShoutChannelID == 0) { ulongDiscShoutChannelID = ulong.MaxValue; }
        }



        public string[] getSavedTeamspeakServerData()
        {
            string[] savedData = new string[2];
            savedData[0] = StrSavedTeamspeakHostName;
            savedData[1] = UlongWatchedDiscordUserID.ToString();
            return savedData;
        }

        /// <summary>
        /// Sets all fields of the botConfig class to the values entered in args.
        /// </summary>
        /// <param name="args">An array of values to set the botConfig fields to. Should have 8 entries.
        /// Order of params should be:
        /// 'TsHostname', 'TsVirtualServerID','DiscordUsername',
        /// 'DiscordUserUUID','DiscordWatchChannel','DiscordWatchChannelUUID',
        /// 'DiscordShoutChannel',DiscordShoutChannelUUID'</param>
        public void setConfigValues(string[] args)
        {
            StrSavedTeamspeakHostName = args[0];
            IntSavedTeamspeakVirtualServerID = Convert.ToInt32(args[1]);
            StrWatchedDiscordChannelName = args[2];
            UlongWatchedDiscordChannelID = Convert.ToUInt64(args[3]); 
            StrWatchedDiscordUserName = args[4];
            UlongWatchedDiscordUserID = Convert.ToUInt64(args[5]);
            StrDiscShoutChannel = args[6];
            UlongDiscShoutChannelID = Convert.ToUInt64(args[7]);
            lastConfigUpdateFromDisk = DateTime.Now;

        }
        public void setConfigValues(string newHostname, int newTsID, string newWatchedChan, ulong newWatchedChanID,string newWatchedUser, ulong newWatchedUserID, string newShoutChan, ulong newShoutChanID)
        {
            StrSavedTeamspeakHostName = newHostname;
            IntSavedTeamspeakVirtualServerID = newTsID;
            StrWatchedDiscordUserName = newWatchedChan;
            UlongWatchedDiscordUserID = newWatchedChanID;
            StrWatchedDiscordChannelName = newWatchedUser;
            UlongWatchedDiscordChannelID = newWatchedUserID;
            StrDiscShoutChannel = newShoutChan;
            UlongDiscShoutChannelID = newShoutChanID;
            lastConfigUpdateFromDisk = DateTime.Now;

        }

        public void setConfigValues(botConfig json)
        {
            strSavedTeamspeakHostName = json.StrSavedTeamspeakHostName;
            intSavedTeamspeakVirtualServerID = json.IntSavedTeamspeakVirtualServerID;
            strWatchedDiscordUserName = json.StrWatchedDiscordUserName;
            ulongWatchedDiscordUserID = json.UlongWatchedDiscordUserID;
            strWatchedDiscordChannelName = json.StrWatchedDiscordChannelName;
            ulongWatchedDiscordChannelID = json.UlongWatchedDiscordChannelID;
            strDiscShoutChannel = json.StrDiscShoutChannel;
            ulongDiscShoutChannelID = json.UlongDiscShoutChannelID;
            lastConfigUpdateFromDisk = DateTime.Now;
        }
    }

    public class adminInternalConfig //Things that should not be changeable while the bot is running. Can be get but not set
    {
        public string botToken { get; }
        public string tsSeverQueryUsername { get; }
        public string tsServerQueryPassword { get; }
        public ulong discordGuildID { get; }
        readonly FileOperations _fileio;

        internal class adminConfigModel
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public string MbotToken { get; set; }
            public string MtsSeverQueryUsername { get; set; }
            public string MtsServerQueryPassword { get; set; }
            public ulong  MdiscordGuildID { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        }

        public adminInternalConfig(FileOperations fileio)
        {
            _fileio = fileio;
            //get the token from the disk
            //get the ts3 server query username and password from the disk
            var adminConfigFile = Path.Combine(_fileio.DirectoryPath, "adminconfig.json");

            if (File.Exists(adminConfigFile))
            {
                string configString = File.ReadAllText(adminConfigFile);
                var options = new JsonSerializerOptions { IncludeFields = true, };

                var deserializeddata = JsonSerializer.Deserialize<adminConfigModel>(configString, options);

                if (deserializeddata.MbotToken != null && deserializeddata.MbotToken != "Default Token - Change This")
                {
                    botToken = deserializeddata.MbotToken;
                    tsSeverQueryUsername = deserializeddata.MtsSeverQueryUsername;
                    tsServerQueryPassword = deserializeddata.MtsServerQueryPassword;
                    discordGuildID = deserializeddata.MdiscordGuildID;
                }
                else
                {
                    Console.WriteLine("Invalid Config Detected!");
                    throw new Exception("Invalid Config Detected!");
                }
            }
            else
            {
                botToken ??= "Default Token - Change This";
                tsSeverQueryUsername ??= "Default ServerQuery Username - Change This";
                tsServerQueryPassword ??= "Default ServerQuery Password - Change This";
                if (discordGuildID != 0) { discordGuildID = 0; }

                _fileio.DumpConfigToJSON(this);
                Thread.Sleep(100);
                Console.WriteLine("No Config Found. Boilerplate has been created at: " + adminConfigFile + "\nPlease edit this file and restart the bot.");
                throw new Exception("No Config Found. Boilerplate has been created at: " + adminConfigFile + "\nPlease edit this file and restart the bot.");
                
            }

        }


    }

}

