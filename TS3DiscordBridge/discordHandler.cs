using Discord;
using Discord.WebSocket;
using System.Net.NetworkInformation;

namespace TS3DiscordBridge
{
     //TODO: After Compare User lists, Ping discord users where no match is found.
    internal class discordHandler
    {
        //fields to keep track of.
        internal class currentlyHandledMessage
        {
            ulong UUID;
            DateTimeOffset messageRecievedTime;


            internal currentlyHandledMessage()
            {
                UUID = 0;
                DateTimeOffset messageRecievedTime = DateTimeOffset.MinValue;
            }

            public void assignFields(ulong uuid, DateTimeOffset msgTime)
            {
                UUID = uuid;
                messageRecievedTime = msgTime;
            }
        }
        static currentlyHandledMessage currentMessage = new currentlyHandledMessage();

        //Everything related to actually doing things in discord servers go here.

        public async Task SlashCommandHandler(SocketSlashCommand command)
        { //Switch-Case to handle all slash commands given to the bot. Each case needs to await a function.
            switch (command.Data.Name)
            {
                case "setup-bot-config":
                    await setupBotConfig(command);
                    break;
                case "trigger-get-last-messages":
                    await GetLastMessageAsync(command);
                    break;
            }
        }

        public async Task setupBotConfig(SocketSlashCommand command)
        {
            await command.DeferAsync(true); //ack the command but Defer response until actually handled.

            //pass the variables setup by the 'setup' command to the botConfigHandler and push the botConfigHandler class to disk.
            var TSorDiscord = command.Data.Options;
            var InputArray = TSorDiscord.ToArray();
            //order = HOSTNAME -> SERVER_ID -> TS_CHAN_ID -> DS_USER -> DS_CHAN
            //---------------------------------------------------//
            //Data Transformations so we can do what we need to.
            string newHostname = (string)InputArray[0].Value;
            var newServerID = (long)InputArray[1].Value;
            var newChanID = (long)InputArray[2].Value;
            string newUsernameHR = (string)InputArray[3];
            string newChanHR = (string)InputArray[4];
            var newChanType = InputArray[4].Value as ITextChannel;
            var newChannelID = newChanType.Id;
            var userTypeCast = InputArray[3].Value as IGuildUser;
            var newUserID = userTypeCast.Id;
            var strNewUserID = newUserID.ToString();
            var strNewChannelID = newChannelID.ToString();
            //-------------------------------------------------//

            //create instance of config handler
            botConfigHandler newConfig = new botConfigHandler();
            newConfig.setConfigValues(newHostname, Convert.ToInt32(newServerID), Convert.ToInt32(newChanID), strNewUserID, strNewChannelID, newChanHR, newUsernameHR); //Push new configs to json
            FileOperations x = new(); _ = x.DumpConfigToJSON(newConfig);

            //Build the message that is shown to the user.
            var message = "Updated Settings:\nHostname: `" + newHostname
            + "`\nTS3 Virtual Server ID: `" + newServerID + "`\nTS3 Channel ID: `" + newChanID
            + "`\nWatched Discord User: `" + newUsernameHR + "`\nWatched User UUID: `" + newUserID + "`\nWatched Channel: `" + newChanHR
            + "`\nWatched Channel UUID: `" + newChannelID + "`";

            //Build the embed thats shown to the user
            var embedBuild = new EmbedBuilder().WithAuthor("TS3xDiscord Bridge").WithTitle("Config Updated Successfully").WithDescription(message).WithCurrentTimestamp().WithColor(Color.Green);

            await command.ModifyOriginalResponseAsync(x => x.Embed = embedBuild.Build()); //Use original responce position to reply to user.

            //force an update of the current config context so that the values are available in memory and we're not waisting time doing disk things.
        }


        public async Task GetLastMessageAsync(SocketSlashCommand command)
        {
            await command.RespondAsync("Got It", ephemeral: true);


            //Get the UUID for the channel we're polling & the UUID for our expected author.
            string watchedDiscordChannelID = Program.config.StrWatchedDiscordChannelID;
            string watchedDiscordUserID = Program.config.StrWatchedDiscordUserID;
            //Check the channel for the last message
            var IChannel = await Program.client.GetChannelAsync(Convert.ToUInt64(watchedDiscordChannelID));
            var toTextChannel = IChannel as SocketTextChannel;
            var messages = await toTextChannel.GetMessagesAsync(5).FlattenAsync();
            //compare the messages author uuid to our expected uuid
            currentMessage = parseMessageCollection(messages, Convert.ToUInt64(watchedDiscordUserID));
            //if message matches criteria, store details of message in a 'currentlyHandledMessage' object

        }


        public currentlyHandledMessage parseMessageCollection(IEnumerable<IMessage> messages, ulong desiredID)
        {
            currentlyHandledMessage currentCandidate = new currentlyHandledMessage();
            foreach (var message in messages)
            {

                if (message.Author.Id == desiredID && (message.Timestamp >= DateTimeOffset.UtcNow.AddHours(-1))) //Check if correct author and if message is recent enough
                {
                    currentCandidate.assignFields(message.Id,message.Timestamp);
                    return currentCandidate;
                }
                return currentCandidate;
            }
            return currentCandidate;
        }
    }
}