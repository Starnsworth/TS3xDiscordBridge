using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Net.Security;
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
            ulong channelID = Convert.ToUInt64(_botConfigHandler.UlongWatchedDiscordChannelID);
            var ichannel = await _discordSocketClient.GetChannelAsync(channelID) as SocketTextChannel;
            var message = await ichannel.GetMessageAsync(messageUUID) as IMessage;
            var ThumbReac = await message.GetReactionUsersAsync(thumbEmoji, 40).FlattenAsync();
            var JReac = await message.GetReactionUsersAsync(regJEmoji, 10).FlattenAsync();
            foreach (var item in ThumbReac)
            {
                if (!item.IsBot)
                {
                    discUserList.Add(item.Username.ToString(), item.Id);
                }

            }
            foreach (var item in JReac)
            {
                if (!item.IsBot && !discUserList.ContainsValue(item.Id))
                {
                    discUserList.Add(item.Username.ToString(), item.Id);
                }
            }

            return discUserList;
        }

        /// <summary>
        /// Builds list of users currently on the teamspeak server. we ignore exactly what channel theyre in because the TS has a single purpose.
        /// If we look to extend functionality for use on in other communities, we can add a channelID check.
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
                    tsNickUID.Add(i.NickName, uid.ClientUid);

                }
            }
            return tsNickUID;
        }

        public void AdminAddAliasToDB()
        {
            throw new NotImplementedException();
        }

        public async Task runUserComparison()
        {
#if DEBUG
            var discUserList = await retrieveDiscordReactionsList();
#else
            var discUserList = await retrieveDiscordReactionsList();
            var tsUserList = await retrieveTsCurrentUserList();
#endif

            //Do the main comparision here.
            foreach (var discUser in discUserList)
            {
#if DEBUG //This block is the internal of the if statement below.

                    //retrieve all keys/values and send to database.
                    UserMatch FoundUserInfo = new UserMatch();
                    FoundUserInfo.DiscordUsername = discUser.Key;
                    FoundUserInfo.DiscordUserId = discUser.Value;
                    FoundUserInfo.TeamspeakUsername = "Test";
                    FoundUserInfo.TeamspeakUserId = "testID";
                    await _db.SaveToDB(FoundUserInfo);
                    //Then remove the found ts user from the list.
                    tsUserList.Remove(discUser.Key);

#else
                if (
                    tsUserList.TryGetValue(discUser.Key, out string tsUsername)
                   )
                {
                    //retrieve all keys/values and send to database.
                    UserMatch FoundUserInfo = new UserMatch();
                    FoundUserInfo.DiscordUsername = discUser.Key;
                    FoundUserInfo.DiscordUserId = discUser.Value;
                    FoundUserInfo.TeamspeakUsername = tsUsername;
                    FoundUserInfo.TeamspeakUserId = tsUserList[tsUsername];
                    await _db.SaveToDB(FoundUserInfo);
                    //Then remove the found ts user from the list.
                    tsUserList.Remove(discUser.Key);
                }
                else
                {
                    //search the db for an alias, if no alias, add to list of users not found.
                    discUserNotFound.Add(discUser.Key, discUser.Value);
                }
#endif

            }

        }

        






        

        

        
    }
}


