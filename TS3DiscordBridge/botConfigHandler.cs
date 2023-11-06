namespace TS3DiscordBridge
{
    public class botConfigHandler
    {
        //Just config variables. Could Reasonable be interacted with via 'Program.<instance>' contexts.

        public string StrSavedTeamspeakHostName;
        public int IntSavedTeamspeakVirtualServerID;
        public string StrWatchedDiscordUserID;
        public string StrWatchedDiscordUserName;
        public string StrWatchedDiscordChannelID;
        public string StrWatchedDiscordChannelName;
        public DateTime lastConfigUpdateFromDisk;
       


        public botConfigHandler()
        {
            StrSavedTeamspeakHostName ??= "DefaultHostName";
            StrWatchedDiscordUserID ??= "defaultDiscordUserID";
            StrWatchedDiscordChannelID ??= "defaultDiscordChannelID";
            StrWatchedDiscordChannelName ??= "defaultDiscordChannelName";
            StrWatchedDiscordUserName ??= "defaultDiscordUserName";
            if (IntSavedTeamspeakVirtualServerID <= 0) { IntSavedTeamspeakVirtualServerID = -1; }
        }



        public string[] getSavedTeamspeakServerData()
        {
            string[] savedData = new string[3];
            savedData[0] = StrSavedTeamspeakHostName;
            savedData[2] = StrWatchedDiscordUserID.ToString();
            return savedData;
        }
        public void setConfigValues(string newHostname, int virtualServerID, int tsChannelID, string discUserUUID, string discChannelUUID, string discChannelName, string discUserName)
        {
            StrSavedTeamspeakHostName = newHostname;
            IntSavedTeamspeakVirtualServerID = virtualServerID;
            StrWatchedDiscordUserID = discUserUUID;
            StrWatchedDiscordChannelID = discChannelUUID;
            StrWatchedDiscordChannelName = discChannelName;
            StrWatchedDiscordUserName = discUserName;
            
        }
    }


}

