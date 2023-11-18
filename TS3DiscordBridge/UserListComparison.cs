using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using TeamSpeak3QueryApi.Net.Specialized;

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
        private readonly FileOperations _fileio;


        static Dictionary<string, ulong> discUserList = new Dictionary<string, ulong>(); //Key is plaintext username, value is the UUID
        static Dictionary<string, string> tsUserList = new Dictionary<string, string>(); //Key is plaintext nickname, value is UID.
        Dictionary<string, string> discUserNotFound = new Dictionary<string, string>();

        public UserListComparison(discordHandler discordHandler, botConfig botConfig, DiscordSocketClient client, FileOperations fileio)
        {
            _botConfigHandler = botConfig;
            _discordHandler = discordHandler;
            _discordSocketClient = client;
            _fileio = fileio;
        }

        internal async Task retrieveDiscordReactionsList()
        {
            var thumbEmoji = new Emoji("👍");
            var regJ = new Emoji("🇯");
            ulong messageUUID = await _discordHandler.GetLastMessageAsync();
            ulong channelID = Convert.ToUInt64(_botConfigHandler.UlongWatchedDiscordChannelID);
            var ichannel = await _discordSocketClient.GetChannelAsync(channelID) as SocketTextChannel;
            var message = await ichannel.GetMessageAsync(messageUUID) as IMessage;
            var ThumbReac = await message.GetReactionUsersAsync(thumbEmoji, 40).FlattenAsync();
            var JReac = await message.GetReactionUsersAsync(regJ, 10).FlattenAsync();
            foreach (var item in ThumbReac)
            {
                if (!item.IsBot)
                {
                    discUserList.Add(item.Username.ToString(), item.Id);
                }

            }
            foreach (var item in JReac)
            {
                if (!item.IsBot)
                {
                    discUserList.Add(item.Username.ToString(), item.Id);
                }
            }

            return;
        }

        internal async Task retrieveTsCurrentUserList()
        {
            Dictionary<string, string> tsNickUID = new Dictionary<string, string>();
            var rc = new TeamSpeakClient(/*config.StrSavedTeamspeakHostName*/"localhost"); //TODO: Remove debug reference
            await rc.Connect();
            await rc.Login("test2", "fAosdwwI"); //TODO: Make this not hardcoded.
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
            tsUserList = tsNickUID;

            return;
        }

        /// <summary>
        /// Loops through discUserList and checks if any usernames show up in tsUserList. If found, adds both keys and values to the db. if not found, searches the database to see if the discord id exists.
        /// </summary>
        public void CompareAndRecordMatches() 
        {
            string connectionString = "Data Source=" + Path.Combine(_fileio.DirectoryPath, "shoutbot.db") + ";";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Create a table if it doesn't exist
                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS UserMatches (
                    DiscordUserId INTEGER PRIMARY KEY UNIQUE,
                    DiscordUsername TEXT,
                    TeamspeakUsername TEXT,
                    TeamspeakUserId TEXT UNIQUE
                )";
                command.ExecuteNonQuery();
                Thread.Sleep(100);
                // Iterate through Discord dictionary
                foreach (var discUser in discUserList)
                {
                    // Check if the Teamspeak dictionary has a matching key
                    if (tsUserList.TryGetValue(discUser.Key, out string tsUsername))
                    {
                        using (var cmd = new SqliteCommand(@"
            INSERT INTO UserMatches (DiscordUsername, DiscordUserId, TeamspeakUsername, TeamspeakUserId)
            VALUES (@DiscordUsername, @DiscordUserId, @TeamspeakUsername, @TeamspeakUserId)"))
                        {
                            cmd.Parameters.AddWithValue("@DiscordUsername", discUser.Key);
                            cmd.Parameters.AddWithValue("@DiscordUserId", discUser.Value.ToString());
                            cmd.Parameters.AddWithValue("@TeamspeakUsername", tsUsername);
                            cmd.Parameters.AddWithValue("@TeamspeakUserId", tsUserList[tsUsername]);

                            using (var transaction = connection.BeginTransaction())
                            {
                                cmd.Connection = connection;
                                cmd.Transaction = transaction;

                                // Execute the command
                                cmd.ExecuteNonQuery();

                                // Commit the transaction
                                transaction.Commit();
                            }
                        }
                    }
                    else
                    {
                        //TODO: Fall back to SearchDatabase method to see if we can find a match
                        //Search DB with discord username or ID, if no match, add to list of users not found.
                        //if there is a match, check if the TS3UID matches the TS3UID in the TS3 list. if it does, don't add to list of non-matches.
                        if (!SearchDatabase(discUser.Value)) //If we don't find a match, add to list of users not found.
                        {
                            discUserNotFound.Add(discUser.Key, discUser.Value.ToString());
                        }

                    }
                }

            }
            File.WriteAllText(Path.Combine(_fileio.DirectoryPath, "userNotFound.txt"), string.Join(Environment.NewLine, discUserNotFound));

        }

        /// <summary>
        /// searches the database for a match to the search term. If a match is found, return true. If no match is found, return false.
        /// </summary>
        /// <param name="searchTerm">ulong discord Unique User ID</param>
        /// <returns></returns>
        public bool SearchDatabase(ulong searchTerm)
        {
            string connectionString = "Data Source=" + Path.Combine(_fileio.DirectoryPath, "shoutbot.db") + ";";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SqliteCommand(@"
            SELECT * FROM UserMatches
            WHERE DiscordUsername = @SearchTerm OR TeamspeakUsername = @SearchTerm OR DiscordUserId = @SearchTerm OR TeamspeakUserId = @SearchTerm "))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {//If we have rows, we have a match.
                            while (reader.Read())
                            {
                                var userMatch = new UserMatch
                                {
                                    DiscordUserId = Convert.ToUInt64(reader.GetInt64(reader.GetOrdinal("DiscordUserId"))),
                                    DiscordUsername = reader.GetString(reader.GetOrdinal("DiscordUsername")),
                                    TeamspeakUsername = reader.GetString(reader.GetOrdinal("TeamspeakUsername")),
                                    TeamspeakUserId = reader.GetString(reader.GetOrdinal("TeamspeakUserId"))
                                };
                                //Check if the known teamspeakUserID exists in the list of users currently in TS.
                                if (!tsUserList.ContainsValue(userMatch.TeamspeakUserId))
                                {
                                    //if it doesn't, add the user to the list of users not found.
                                    discUserNotFound.Add(userMatch.DiscordUsername, userMatch.DiscordUserId.ToString());

                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
        }
    }

    // Model class for database results
    public class UserMatch
    {
        public string DiscordUsername { get; set; }
        public ulong DiscordUserId { get; set; }
        public string TeamspeakUsername { get; set; }
        public string TeamspeakUserId { get; set; }
    }

}


