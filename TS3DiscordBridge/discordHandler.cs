using Discord;
using Discord.WebSocket;

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
                case "test-scheduler":
                    await CreateCustomScheduledItem(command);
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
            string newUsernameHR = (string)InputArray[2];
            string newChanHR = (string)InputArray[3];
            var newChanType = InputArray[3].Value as ITextChannel;
            var newChannelID = newChanType.Id;
            var userTypeCast = InputArray[2].Value as IGuildUser;
            var newUserID = userTypeCast.Id;
            var strNewUserID = newUserID.ToString();
            var strNewChannelID = newChannelID.ToString();
            //-------------------------------------------------//

            //create instance of config handler
            botConfigHandler newConfig = new botConfigHandler();
            newConfig.setConfigValues(newHostname, Convert.ToInt32(newServerID), strNewUserID, strNewChannelID, newChanHR, newUsernameHR); //Push new configs to json
            FileOperations x = new(); await x.DumpConfigToJSON(newConfig);

            //Build the message that is shown to the user.
            var message = "Updated Settings:\nHostname: " + "`" + newConfig.StrSavedTeamspeakHostName + "` "
            + "\nTS3 Virtual Server ID: " + "`" + newConfig.IntSavedTeamspeakVirtualServerID + "`"
            + "\nWatched Discord User: " + "`" + newConfig.StrWatchedDiscordUserName + "`"
            + "\nWatched User UUID: " + "`" + newConfig.StrWatchedDiscordUserID + "`"
            + "\nWatched Channel: " + "`" + newConfig.StrWatchedDiscordChannelName + "`"
            + "\nWatched Channel UUID: `" + newConfig.StrWatchedDiscordChannelID + "`";

            //Build the embed thats shown to the user
            var embedBuild = SlashCommandConstructors.constructEmbedForResponse(message, Color.Green, "Config Updated Successfully!");
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
                    currentCandidate.assignFields(message.Id, message.Timestamp);
                    return currentCandidate;
                }
                return currentCandidate;
            }
            return currentCandidate;
        }

        public async Task CreateCustomScheduledItem(SocketSlashCommand command)
        {
            await command.DeferAsync(true); //Defer the responce because we need to know the outcome of the operation so we can respond appropriately.
            TaskScheduling taskScheduling = new TaskScheduling();


            var args = command.Data.Options.ToArray();
            //DATA ORDER
            // (int)DAY -> (int)HOUR ->(int)MINUTE
            var test = Enum.GetName(typeof(DayOfWeek), args[0].Value); //Make args[0] into something DayOfWeek can read.


            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), Enum.GetName(typeof(DayOfWeek), args[0].Value), true);
            int hour = Convert.ToInt32(args[1].Value);
            int minute = Convert.ToInt32(args[2].Value);


            if ((hour <= 24 && minute <= 60 && hour >= 0 && minute >= 0) == false)
            {
                var setCustomDateTimeFail = SlashCommandConstructors.constructEmbedForResponse("Malformed data provided as custom operation time!", Color.Red);
                await command.ModifyOriginalResponseAsync(x => x.Embed = setCustomDateTimeFail.Build());
            }
            else
            {
                taskScheduling.SetCustomRequiredDateTime(day, hour, minute); //userland command that can get called by the slash command.
                var setCustomDateTimeSuccess = SlashCommandConstructors.constructEmbedForResponse(taskScheduling.getRequiredDateTime(), Color.Green, "Custom Time Set Successfully!");
                await command.ModifyOriginalResponseAsync(x => x.Embed = setCustomDateTimeSuccess.Build());
            }



        }
    }
}