using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace TS3DiscordBridge
{
    /*
     * function: Sundays & Tuesdays, check in 3x to ensure valid data.
     * DONE: Timer Functionality to check in at the correct times.
     *      DONE: slash command so staff can create custom times to sound off.
     */


    public class OperationTimer
    {
        //private readonly IServiceProvider _provider;
        private readonly botConfig _botConfig;
        private readonly OperationTimer _taskScheduling;


        internal DateTime RequiredDateTime = new DateTime();
        public DateTime requiredDateTime
        {
            get { return RequiredDateTime; }
            set { RequiredDateTime = value; }
        }

        string? TaskName;
        string shoutChannel;

        /// <summary>
        /// Presets requiredDateTime to the next occurance of tuesday or sunday
        /// </summary>
        public OperationTimer(botConfig botConfig) //Change this to something that can be set by a user.
        {

            _botConfig = botConfig;

            //Set requiredDateTime to the closest tuesday or sunday.
            DateTime currentDateTime = DateTime.Now;
            shoutChannel = _botConfig.StrDiscShoutChannel;
            if (currentDateTime.DayOfWeek.ToString() == "Monday" || currentDateTime.DayOfWeek.ToString() == "Tuesday") // if op is on tuesday
            {
                //set requiredDateTime to tuesday 1930
                var working = SetToRequiredTime(currentDateTime, 19, 30);
                requiredDateTime = SetNextWeekday(working, DayOfWeek.Tuesday);

            }
            else if (currentDateTime.DayOfWeek.ToString() == "Wednesday" || //If next op is on sunday
                       currentDateTime.DayOfWeek.ToString() == "Thursday" ||
                       currentDateTime.DayOfWeek.ToString() == "Friday" ||
                       currentDateTime.DayOfWeek.ToString() == "Saturday" ||
                       currentDateTime.DayOfWeek.ToString() == "Sunday")
            {
                //set requiredDateTime to Sunday 2030
                var working = SetNextWeekday(currentDateTime, DayOfWeek.Sunday);
                requiredDateTime = SetToRequiredTime(currentDateTime, 20, 30);





            }
            //after setting if the next one is tuesday or sunday.
            //we need to start the daily check for what day it is.
            //so start timer here. when the check is true, we do the needful
            TaskScheduler.Instance.ScheduleTask(18, 30, 24, CheckDateTime);


            //TODO: Do some actions shortly before the operation is supposed to start.
            //manipulate the hour & minutes fields to 20 and 30
            //probably in another method like getModToRequiredTime(DateTime.Hour, DateTime.Minute)

        }
        /// <summary>
        /// Resets RequiredDateTime to the next occurance of tuesday or sunday.
        /// </summary>
         void rebuildRequiredDateTime() 
        {
            DateTime currentDateTime = DateTime.Now;
            if (currentDateTime.DayOfWeek.ToString() == "Monday" || currentDateTime.DayOfWeek.ToString() == "Tuesday") // if op is on tuesday
            {
                //set requiredDateTime to tuesday 1930
                var working = SetToRequiredTime(currentDateTime, 19, 30);
                _taskScheduling.requiredDateTime = SetNextWeekday(working, DayOfWeek.Tuesday);

            }
            else if (currentDateTime.DayOfWeek.ToString() == "Wednesday" || //If next op is on sunday
                       currentDateTime.DayOfWeek.ToString() == "Thursday" ||
                       currentDateTime.DayOfWeek.ToString() == "Friday" ||
                       currentDateTime.DayOfWeek.ToString() == "Saturday" ||
                       currentDateTime.DayOfWeek.ToString() == "Sunday")
            {
                //set requiredDateTime to Sunday 2030
                var working = SetNextWeekday(currentDateTime, DayOfWeek.Sunday);
                _taskScheduling.requiredDateTime = SetToRequiredTime(currentDateTime, 20, 30);
            }
        }


        /// <summary>
        /// Function should be run once daily at 1830hrs. If requiredDateTime is set to 'today', we start two timers.
        /// One timer counts down until the actual operation, the other timer counts down to approx 10 minutes prior to do the setup.
        /// </summary>
        internal  void CheckDateTime()
        {
            var timeToCheck = _taskScheduling.requiredDateTime;
            if (timeToCheck >= DateTime.Now)
            {
                if (timeToCheck.DayOfWeek == DateTime.Today.DayOfWeek)
                {
                    //if todays the day, call a function that checks the time and then builds the message to send.
                    //Get time interval until timetocheck
                    TimeSpan timeUntilOp = timeToCheck - DateTime.Now;
                    TimeSpan tenBeforeTimeUntilOp = timeUntilOp - TimeSpan.FromMinutes(10);

                    var UntilOpTimer = new Timer(x =>
                    {
                        //UserListComparison.finalcomparison(); //function that actually does the comparision and shouts it at discord.
                    }, null, timeUntilOp, Timeout.InfiniteTimeSpan);

                    var halfuntiloptimer = new Timer(x =>
                    {
                        //UserListComparison.setupdataearly(); //function that starts the data collection process
                    }, null, tenBeforeTimeUntilOp, Timeout.InfiniteTimeSpan);


                    //Program.discordHandler.buildMessasgeToSend();
                }
                else { _taskScheduling.rebuildRequiredDateTime(); }
            }
        }



        /// <summary>
        /// Sets the the requiredDateTime field to the designated parameters.
        /// </summary>
        /// <param name="requestedSetDay"> DayOfWeek object of the day to set the task timeout to.</param>
        /// <param name="requestedSetHour">Hour in 24 hour format to set the task timeout to.</param>
        /// <param name="requestedSetMinute">Minute to set the task timeout to.</param>
        /// <returns></returns>
        internal string SetCustomRequiredDateTime(DayOfWeek requestedSetDay, int requestedSetHour, int requestedSetMinute)
        {
            try
            {
                TaskName = "Custom Operation";
                requiredDateTime = SetNextWeekday(DateTime.Now, requestedSetDay);
                requiredDateTime = SetToRequiredTime(requiredDateTime, requestedSetHour, requestedSetMinute);
                return "0";
            }
            catch
            {
                Console.WriteLine("Malformed data provided to 'SetToRequiredTime'");

                return "Malformed data provided as operation time.";
            }
        }

        /// <summary>
        /// Gets the next specified weekday.
        /// </summary>
        /// <param name="startDay">DateTime object to start the search from. Eg. 'DateTime.Now'</param>
        /// <param name="dayToFind">DayOfWeek enum to find. Eg. 'DayOfWeek.Tuesday'</param>
        /// <returns>The amount of days to increment a DateTime object to return the requested day.</returns>
        internal static DateTime SetNextWeekday(DateTime startDay, DayOfWeek dayToFind)
        {
            int daysToAdd = ((int)dayToFind - (int)startDay.DayOfWeek + 7) % 7;
            return startDay.AddDays(daysToAdd);
        }

        /// <summary>
        /// Modifies an existing DateTime object to the user specified time.
        /// </summary>
        /// <param name="objToChange">DateTime object of original time. Commonally a DateTime.Now object</param>
        /// <param name="userHH">User privded hour in 24hr time format.</param>
        /// <param name="usermm">User provided minutes.</param>
        /// <returns>Returns the method signiture to modifiy an existing DateTime object. Usually used in conjunction with 'objToChange'.</returns>
        internal static DateTime SetToRequiredTime(DateTime objToChange, int userHH, int usermm)
        {
            int modHrs, modMins;
            modHrs = userHH - objToChange.Hour % 24;
            modMins = usermm - objToChange.Minute % 60;
            return objToChange.AddHours(modHrs).AddMinutes(modMins);

        }


        /// <summary>
        /// Class to handle the scheduling of tasks.
        /// </summary>
        public class TaskScheduler
        {
            private static TaskScheduler _instance;
            private List<Timer> timers = new List<Timer>();

            private TaskScheduler() { }

            public static TaskScheduler Instance => _instance ?? (_instance = new TaskScheduler());


            /// <summary>
            /// Creates a timer that runs a task at the specified time.
            /// </summary>
            /// <param name="hour">Hour that you want the task to run in</param>
            /// <param name="min">Minute that you want the task to run at</param>
            /// <param name="intervalInHour">How often you want the task to run</param>
            /// <param name="task">Task to run. Commonly a lambda function</param>
            public void ScheduleTask(int hour, int min, double intervalInHour, Action task)
            {
                DateTime now = DateTime.Now;
                DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, hour, min, 0, 0);
                if (now > firstRun)
                {
                    firstRun = firstRun.AddDays(1);
                }

                TimeSpan timeToGo = firstRun - now;
                if (timeToGo <= TimeSpan.Zero)
                {
                    timeToGo = TimeSpan.Zero;
                }

                var timer = new Timer(x =>
                {
                    task.Invoke();
                }, null, timeToGo, TimeSpan.FromHours(intervalInHour));

                timers.Add(timer);
            }
        }

        class discDailyCheck
        {
            //Check if discord should send messages at the required time today.
            //set scheduledTime to requiredDateTime for the operation.
            //check what day it is on startup, use main timer to check every 12 hours what day it is.
            //if it is a scheduled day, run the function that sends messages.
            //This function only for working out if its the correct day.
            DateTime schedueledTime;

            internal discDailyCheck(DateTime timeToRun)
            {
                schedueledTime = timeToRun;
                //var day = timeToRun.DayOfWeek;
                //var hr = timeToRun.Hour;
                //var min = timeToRun.Minute;
                TaskScheduler.Instance.ScheduleTask(14, 49, 0.00139, testfunct/*() => { Console.WriteLine("Timer test"); }*/); //use this to check daily?
                //then use the function it calls to check the minutes when it matters.

            }
            //TODO: Ensure timer functionality works and isnt commented out.
            public static void testfunct()
            {
                return;
            }

        }


    }
}
