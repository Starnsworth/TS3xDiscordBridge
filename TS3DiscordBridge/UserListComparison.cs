using Discord.Rest;
using Discord.WebSocket;

namespace TS3DiscordBridge
{
    /*
     * function: Compare users & sound off. Object should be able to be passed to the notifier subroutine with no further comparison.
     * 
     * TODO: Method to Build list of users who have reacted to the noti.
     * TODO: Method to build list of users currently in teamspeak - overall presence in the TS is good enough, we dont need a channel ID.
     *      TODO: Parse that huge string that gets returned on 'clientlist -uid' Need Nickname & uid.
     * TODO: Appropriate storage of information. Guess JSON would work with a properly implimented 'users' class.
     * but look to migrate to something else. TinyDB? Mongo? maria?
     */

    internal class UserListComparison
    {
        internal static async Task<List<string>> retrieveDiscordReactionsList()
        {
            var dHandler = Program.discordHandler;
            ulong messageUUID = await dHandler.GetLastMessageAsync();
            ulong channelID = Convert.ToUInt64(Program.config.StrWatchedDiscordChannelID);
            var ichannel = await Program.client.GetChannelAsync(channelID) as SocketTextChannel;
            var message = await ichannel.GetMessageAsync(messageUUID);
            var reactions = message.Reactions.ToArray();
            //var test = reactions[0].Value as SocketReaction;
            Thread.Sleep(100);


            throw new NotImplementedException();
            //Goto message
            //Get message & it's reactions
            //build list out of reactions




            return new List<string>();
        }

        internal List<string> retrieveTsCurrentUserList()
        {
            throw new NotImplementedException();
            //SSH into  serverQuery
            //get the string that has everything in it
            //parse the string to get the username, user uid, and channel id.

            return new List<string>();
        }

        internal void storeListsInDB()
        {
            throw new NotImplementedException();
        }

    }
}
