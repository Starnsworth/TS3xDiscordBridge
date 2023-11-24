using Discord;
using Discord.WebSocket;
using TeamSpeak3QueryApi.Net.Specialized;
using static TS3DiscordBridge.DatabaseHandler;

namespace TS3DiscordBridge
{
    /*
     * function: Compare users & sound off. Object should be able to be passed to the notifier subroutine with no further comparison.
     * 
     * DONE: Method to Build list of users who have reacted to the noti.
     * DONE: Method to build list of users currently in teamspeak - overall presence in the TS is good enough, we dont need a channel ID.
     *      DONE: Parse that huge string that gets returned on 'clientlist -uid' Need Nickname & uid.
     * DONE: SQLite db implimented and values /should/ be able to be added.     
     *      
     *      
     * TODO: Compare the two lists, then check the db for aliases and return values that don't match to discUserNotFound. 
     *    Also send the list to admin to appropriately deal with aliases
     *      
     */

    public class UserListComparison
    {
        private readonly discordHandler _discordHandler;
        private readonly botConfig _botConfigHandler;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly DatabaseHandler _db;
        private readonly adminInternalConfig _aConfig;
        private readonly DatabaseSchema KnownUserAliasDb;

        Dictionary<string, ulong> discUserNotFound = new Dictionary<string, ulong>(); //Username, UID
        Dictionary<string, ulong> discUserList = new Dictionary<string, ulong>();
        Dictionary<string, string> tsUserList = new Dictionary<string, string>();

        public UserListComparison(discordHandler discordHandler, botConfig botConfig, DiscordSocketClient client, DatabaseHandler db, adminInternalConfig aConfig)
        {
            _botConfigHandler = botConfig;
            _discordHandler = discordHandler;
            _discordSocketClient = client;
            _db = db;
            _aConfig = aConfig;



            Dictionary<string, string> UserAliasFields = new Dictionary<string, string>();
            UserAliasFields.Add("DiscordUserId", "INTEGER");
            UserAliasFields.Add("DiscordUsername", "TEXT");
            UserAliasFields.Add("TeamspeakUsername", "TEXT");
            UserAliasFields.Add("TeamspeakUserId", "TEXT");
            KnownUserAliasDb = new DatabaseSchema("UserAliases", "KnownUserAliases", UserAliasFields);


        }

        //------------------------------------------//

        /// <summary>
        /// Builds a list of users who have reacted to the notification message in discord with the prescribed emojis.
        /// </summary>
        /// <returns></returns>
        internal async Task<Dictionary<string, ulong>> retrieveDiscordReactionsList()
        {
            Dictionary<string, ulong> discUserList = new Dictionary<string, ulong>(); //Username, UID
            //TODO: LowPri: Might be  worth making the emoji's configurable.
            var thumbEmoji = new Emoji("👍");
            var regJEmoji = new Emoji("🇯");
            ulong messageUUID = await _discordHandler.GetLastMessageAsync();
            if (messageUUID == 0)
            {
                return null;
            }
            ulong channelID = Convert.ToUInt64(_botConfigHandler.UlongWatchedDiscordChannelID);
            var ichannel = await _discordSocketClient.GetChannelAsync(channelID) as ITextChannel;
            var message = await ichannel.GetMessageAsync(messageUUID) as IMessage;
            var ThumbReac = await message.GetReactionUsersAsync(thumbEmoji, 50).FlattenAsync();
            // var JReac = await message.GetReactionUsersAsync(regJEmoji, 10).FlattenAsync();
            foreach (var item in ThumbReac)
            {
                if (!item.IsBot)
                {
                    discUserList.Add(item.Username.ToString(), item.Id);
                }

            }
            //foreach (var item in JReac)
            //{
            //    if (!item.IsBot && !discUserList.ContainsValue(item.Id))
            //    {
            //        discUserList.Add(item.Username.ToString(), item.Id);
            //    }
            //}

            return discUserList;
        }

        /// <summary>
        /// Builds list of users currently on the teamspeak server.
        /// If we need to extend functionality for use on in other communities, we can add a channelID check.
        /// </summary>
        /// <returns></returns>
        internal async Task<Dictionary<string, string>> retrieveTsCurrentUserList()
        {

            Dictionary<string, string> tsNickUID = new Dictionary<string, string>();
            var rc = new TeamSpeakClient(_botConfigHandler.StrSavedTeamspeakHostName);
            await rc.Connect();
            await rc.Login(_aConfig.tsSeverQueryUsername, _aConfig.tsServerQueryPassword);
            await rc.UseServer(_botConfigHandler.IntSavedTeamspeakVirtualServerID);
            var clientlist = await rc.GetClients();
            foreach (var i in clientlist)
            {
                if (i.Type == TeamSpeak3QueryApi.Net.Specialized.ClientType.FullClient)
                {
                    var uid = await rc.ClientUidFromClientId(i.Id);
                    tsNickUID.Add(i.NickName.ToLower(), uid.ClientUid);

                }
            }
            return tsNickUID;
        }

        public void AdminAddAliasToDB() //Gets called when an admin manually makes relations between users.
        {
            throw new NotImplementedException();
        }

        public async Task runUserComparison()
        {

            
            var discUserList = await retrieveDiscordReactionsList();
            if(discUserList == null)
            {
                return;
            }
            var tsUserList = await retrieveTsCurrentUserList();

            //Loop through reacted users. Check if they exist in teamspeak.
            foreach (var discUser in discUserList)
            {   //If User is in both discord and teamspeak.
                if (tsUserList.TryGetValue(discUser.Key, out string tsUsername)) //If User is in both discord and teamspeak.
                   
                { 
                    //check if the discordID already exists in the db, if it does, skip it.
                    if (_db.SearchDBForAlias(discUser.Value, KnownUserAliasDb) != null)
                    {
                        tsUserList.Remove(discUser.Key);
                        continue;
                    }
                    //If user does not exist in database, add them.
                    UserMatch FoundUserInfo = new UserMatch();
                    FoundUserInfo.DiscordUsername = discUser.Key;
                    FoundUserInfo.DiscordUserId = discUser.Value;
                    FoundUserInfo.TeamspeakUsername = discUser.Key;
                    FoundUserInfo.TeamspeakUserId = tsUsername;
                    await _db.SaveToDB(FoundUserInfo, KnownUserAliasDb);
                    //Then remove the found ts user from the list.
                    tsUserList.Remove(discUser.Key);
                }
                //If user is in discord but not in teamspeak.
                else
                { 
                    var DatabaseRecord = _db.SearchDBForAlias(discUser.Value, KnownUserAliasDb);
                    if (DatabaseRecord == null) //search the db for an alias, if no alias, add to list of users not found.
                    {
                        discUserNotFound.Add(discUser.Key, discUser.Value);
                        continue;
                    }
                    //If user is in the database, check if the teamspeakID exists in the TsUserList.
                    if (tsUserList.ContainsValue(DatabaseRecord.TeamspeakUserId))
                    {
                        //If the teamspeakID exists in the list, remove it from the list.
                        tsUserList.Remove(DatabaseRecord.TeamspeakUsername);
                    }
                    else
                    {
                        //If the teamspeakID does not exist in the list, add it to the list of users not found.
                        discUserNotFound.Add(discUser.Key, discUser.Value);
                    }
                }
            }

            //check the contents of the tsUserlist and the discUserNotFound list then add them all to a table or two for an admin to sort through.
            //Make Table of users not found in teamspeak.
            Dictionary <string,string> tsNoDiscTitles = new Dictionary<string, string>();
            tsNoDiscTitles.Add("TeamspeakUserId", "TEXT");
            tsNoDiscTitles.Add("TeamspeakUsername", "TEXT");
            
            var TeamspeakNoDiscord = new DatabaseSchema("UserAliases", "Teamspeak_No_Discord", tsNoDiscTitles);
            UserMatch TsNotDisc = new UserMatch();
            foreach (var item in tsUserList)
            {
                TsNotDisc.TeamspeakUsername = item.Key;
                TsNotDisc.TeamspeakUserId = item.Value;
                await _db.SaveToDB(TsNotDisc, TeamspeakNoDiscord);
            }
            //Make Table of discUserNotFound.
            Dictionary<string, string> discNotFoundTitles = new Dictionary<string, string>();
            discNotFoundTitles.Add("DiscordUserId", "INTEGER");
            discNotFoundTitles.Add("DiscordUsername", "TEXT");
            var DiscordNoTeamspeak = new DatabaseSchema("UserAliases", "Discord_No_Teamspeak", discNotFoundTitles);
            UserMatch DiscNotTs = new UserMatch();
            foreach (var item in discUserNotFound)
            {
                DiscNotTs.DiscordUsername = item.Key;
                DiscNotTs.DiscordUserId = item.Value;
                await _db.SaveToDB(DiscNotTs, DiscordNoTeamspeak);
            }
        }













    }
}


