namespace TS3DiscordBridge
{
    /*
     * function: Sundays & Tuesdays, check in 3x to ensure valid data.
     * TODO: Timer Functionality to check in at the correct times.
     *      TODO: slash command so staff can create custom times to sound off.
     */


    public class TaskScheduling
    {

        internal DateTime requiredDateTime = new DateTime(); // this will get passed to some function inside of discordHandler
        string? TaskName;
        string shoutChannel;

        /// <summary>
        /// sets the requiredDateTime field of the TaskScheduling class to the default
        /// </summary>
        internal TaskScheduling() //Presets requiredDateTime to the next occurance of tuesday or sunday
        {
            //Set requiredDateTime to the closest tuesday or sunday.
            DateTime currentDateTime = DateTime.Now;
            shoutChannel = Program.config.StrDiscShoutChannel;
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
            TaskScheduler.Instance.ScheduleTask(18, 30, 24, checkDateTime);


            //Start Debug Section
            Console.WriteLine("New Set Date Time is: " + currentDateTime.ToString());
            //End Debug Section
            //manipulate the hour & minutes fields to 20 and 30
            //probably in another method like getModToRequiredTime(DateTime.Hour, DateTime.Minute)

        }

        void rebuildRequiredDateTime()
        {
            DateTime currentDateTime = DateTime.Now;
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
        }


        /// <summary>
        /// Function should be run once daily at 1830hrs. If requiredDateTime is set to 'today', we start two timers.
        /// One timer counts down until the actual operation, the other timer counts down to approx 10 minutes prior to do the setup.
        /// </summary>
        internal static void checkDateTime()
        {
            var timeToCheck = Program.taskScheduling.requiredDateTime;
            if (timeToCheck >= DateTime.Now)
            {
                if (timeToCheck.DayOfWeek == DateTime.Today.DayOfWeek)
                {
                    //if todays the day, call a function that checks the time and then builds the message to send.
                    //Get time interval until timetocheck
                    TimeSpan timeUntilOp = timeToCheck - DateTime.Now;
                    TimeSpan tenBeforeTimeUntilOp = timeUntilOp - TimeSpan.FromMinutes(10);

                    //var UntilOpTimer = new Timer(x =>
                    //{
                    //    UserListComparison.FinalComparison(); //Function that actually does the comparision and shouts it at discord.
                    //}, null, timeUntilOp, Timeout.InfiniteTimeSpan);

                    //var HalfUntilOpTimer = new Timer(x =>
                    //{
                    //    UserListComparison.setUpDataEarly(); //Function that starts the data collection process
                    //}, null, tenBeforeTimeUntilOp, Timeout.InfiniteTimeSpan);


                    //Program.discordHandler.buildMessasgeToSend();
                }
                else { Program.taskScheduling.rebuildRequiredDateTime(); }
            }
        }


        internal DateTime getRequiredDateTime()
        {
            return requiredDateTime;
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
        /// Gets the difference between current time and the required time.
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

        public class TaskScheduler
        {
            private static TaskScheduler _instance;
            private List<Timer> timers = new List<Timer>();

            private TaskScheduler() { }

            public static TaskScheduler Instance => _instance ?? (_instance = new TaskScheduler());

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
            //make discord send messages at the required time.
            //pass the class the scheduled time, do everything off the scheduled time
            //check for day on startup, use main timer to check every 12 hours what day it is.
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

            public static void testfunct()
            {
                return;
            }

        }


    }
}
