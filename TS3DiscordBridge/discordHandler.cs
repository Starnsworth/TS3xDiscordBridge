using Discord;
using Discord.WebSocket;


namespace TS3DiscordBridge
{
    //TODO: Create method to call after the UserListComparison has finished its work.
    public class discordHandler
    {

        private readonly botConfig _botConfigHandler;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly TaskScheduling _taskScheduling;

        //fields to keep track of.

        public discordHandler(botConfig botConfigHandler)
        {
            _botConfigHandler = botConfigHandler;
        }

        internal class currentlyHandledMessage
        {
            ulong UUID;
            DateTimeOffset messageReceivedTime;


            internal currentlyHandledMessage()
            {
                UUID = 0;
                DateTimeOffset messageReceivedTime = DateTimeOffset.MinValue;
            }

            public void assignFields(ulong uuid, DateTimeOffset msgTime)
            {
                UUID = uuid;
                messageReceivedTime = msgTime;
            }

            public ulong getUUID()
            {
                return UUID;
            }

        }
        static currentlyHandledMessage currentMessage = new currentlyHandledMessage();

        //Everything related to actually doing things in discord servers go here.
        public async Task<ulong> GetLastMessageAsync()
        {
            //await command.RespondAsync("Got It", ephemeral: true);

            //Get the UUID for the channel we're polling & the UUID for our expected author.
            ulong watchedDiscordChannelID = _botConfigHandler.UlongWatchedDiscordChannelID;
            ulong watchedDiscordUserID = _botConfigHandler.UlongWatchedDiscordUserID;
            //Check the channel for the last message
            var IChannel = await _discordSocketClient.GetChannelAsync(watchedDiscordChannelID);
            ITextChannel? toTextChannel = IChannel as SocketTextChannel;
            IEnumerable<IMessage> messages = await toTextChannel.GetMessagesAsync(5).FlattenAsync();
            //compare the messages author uuid to our expected uuid
            currentMessage = parseMessageCollection(messages, Convert.ToUInt64(watchedDiscordUserID));
            //if message matches criteria, store details of message in a 'currentlyHandledMessage' object

            return currentMessage.getUUID();
        }
        internal currentlyHandledMessage parseMessageCollection(IEnumerable<IMessage> messages, ulong desiredID)
        {

            currentlyHandledMessage currentCandidate = new currentlyHandledMessage();
            foreach (var message in messages)
            {

                if (message.Author.Id == desiredID && (message.Timestamp >= DateTimeOffset.UtcNow.AddHours(-14)) && message.Reactions.Count >= 1) //Check if correct author and if message is recent enough
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
            //await UserListComparison.retrieveDiscordReactionsList();
            //await _userListComparision.retrieveTsCurrentUserList();

            var args = command.Data.Options.ToArray();
            //DATA ORDER
            // (int)DAY -> (int)HOUR ->(int)MINUTE
            var test = Enum.GetName(typeof(DayOfWeek), args[0].Value); //Make args[0] into something DayOfWeek can read.


            DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), Enum.GetName(typeof(DayOfWeek), args[0].Value), true);
            int hour = Convert.ToInt32(args[1].Value);
            int minute = Convert.ToInt32(args[2].Value);


            if ((hour <= 24 && minute <= 60 && hour >= 0 && minute >= 0) == false)
            {
                var setCustomDateTimeFail = SlashCommandModule.constructEmbedForResponse("Malformed data provided as custom operation time!", Color.Red);
                await command.ModifyOriginalResponseAsync(x => x.Embed = setCustomDateTimeFail.Build());
            }
            else
            {
                _taskScheduling.SetCustomRequiredDateTime(day, hour, minute); //userland command that can get called by the slash command.
                var setCustomDateTimeSuccess = SlashCommandModule.constructEmbedForResponse(_taskScheduling.getRequiredDateTime().ToString(), Color.Green, "Custom Time Set Successfully!");
                await command.ModifyOriginalResponseAsync(x => x.Embed = setCustomDateTimeSuccess.Build());
            }



        }

        public async Task buildMessasgeToSend()
        {
            Thread.Sleep(1);
            //check the time and then do the user list comparison.
            //Time should be 10 minutes before Program.taskScheduling.requiredDateTime
            var requiredTime = _taskScheduling.requiredDateTime;
            if (DateTime.Now >= requiredTime)
            {
                Console.WriteLine(DateTime.Now + " is Larger than " + requiredTime.ToString());
            }
            else
            {
                Console.WriteLine(DateTime.Now + " is smaller than " + requiredTime.ToString());
            }

            //System.Timers.Timer aTimer = new System.Timers.Timer(60000); //creates a timer aTimer that expires every minute



        }

    }


}