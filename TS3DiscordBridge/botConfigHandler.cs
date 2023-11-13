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
        public string StrDiscShoutChannel;
       


        public botConfigHandler()
        {
            StrSavedTeamspeakHostName ??= "DefaultHostName";
            StrWatchedDiscordUserID ??= "defaultDiscordUserID";
            StrWatchedDiscordChannelID ??= "defaultDiscordChannelID";
            StrWatchedDiscordChannelName ??= "defaultDiscordChannelName";
            StrWatchedDiscordUserName ??= "defaultDiscordUserName";
            StrDiscShoutChannel = "defaultDiscordShoutChannel";
            if (IntSavedTeamspeakVirtualServerID <= 0) { IntSavedTeamspeakVirtualServerID = -1; }
        }



        public string[] getSavedTeamspeakServerData()
        {
            string[] savedData = new string[2];
            savedData[0] = StrSavedTeamspeakHostName;
            savedData[1] = StrWatchedDiscordUserID.ToString();
            return savedData;
        }
        public void setConfigValues(string newHostname, int virtualServerID, string discUserUUID, string discChannelUUID, string discChannelName, string discUserName, string discShoutChannel)
        {
            StrSavedTeamspeakHostName = newHostname;
            IntSavedTeamspeakVirtualServerID = virtualServerID;
            StrWatchedDiscordUserID = discUserUUID;
            StrWatchedDiscordChannelID = discChannelUUID;
            StrWatchedDiscordChannelName = discChannelName;
            StrWatchedDiscordUserName = discUserName;
            lastConfigUpdateFromDisk = DateTime.Now;
            StrDiscShoutChannel = discShoutChannel;

        }
    }


}

