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


}

