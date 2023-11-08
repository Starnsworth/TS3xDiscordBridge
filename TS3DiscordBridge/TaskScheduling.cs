namespace TS3DiscordBridge
{
    /*
     * function: Sundays & Tuesdays, check in 3x to ensure valid data.
     * TODO: Timer Functionality to check in at the correct times.
     *      TODO: slash command so staff can create custom times to sound off.
     */


    internal class TaskScheduling
    {

        internal DateTime requiredDateTime = new DateTime(); // this will get passed to some function inside of discordHandler


        /// <summary>
        /// sets the requiredDateTime field of the TaskScheduling class.
        /// </summary>
        internal void SetRequiredDateTime() //This is purely for the regularly running ops.
        {
            //Set requiredDateTime to the closest tuesday or sunday.
            DateTime currentDateTime = DateTime.Now;
            if (currentDateTime.DayOfWeek.ToString() == "Monday" || currentDateTime.DayOfWeek.ToString() == "Tuesday") // if op is on tuesday
            {
                //set requiredDateTime to tuesday 1930
                requiredDateTime = SetNextWeekday(currentDateTime, DayOfWeek.Tuesday);
                requiredDateTime = SetToRequiredTime(currentDateTime, 19, 30);
            }
            else if (currentDateTime.DayOfWeek.ToString() == "Wednesday" || //If next op is on sunday
                       currentDateTime.DayOfWeek.ToString() == "Thursday" ||
                       currentDateTime.DayOfWeek.ToString() == "Friday" ||
                       currentDateTime.DayOfWeek.ToString() == "Saturday" ||
                       currentDateTime.DayOfWeek.ToString() == "Sunday")
            {
                //set requiredDateTime to Sunday 2030
                currentDateTime = SetNextWeekday(currentDateTime, DayOfWeek.Sunday);
                currentDateTime = SetToRequiredTime(currentDateTime, 20, 30);


                //Start Debug Section
                Console.WriteLine("New Set Date Time is: " + currentDateTime.ToString());
                //End Debug Section
                //manipulate the hour & minutes fields to 20 and 30
                //probably in another method like getModToRequiredTime(DateTime.Hour, DateTime.Minute)
            }


        }

        internal string getRequiredDateTime()
        {
            return requiredDateTime.ToString();
        }

        internal string SetCustomRequiredDateTime(DayOfWeek requestedSetDay, int requestedSetHour, int requestedSetMinute)
        {
            try
            {
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

        //need to know the time

        /// <summary>
        /// Gets the next specified weekday.
        /// </summary>
        /// <param name="startDay">DateTime object to start the search from. Eg. 'DateTime.Now'</param>
        /// <param name="dayToFind">DayOfWeek enum to find. Eg. 'DayOfWeek.Tuesday'</param>
        /// <returns>The amount of days to increment a DateTime object to return the requested day.</returns>
        internal static DateTime SetNextWeekday(DateTime startDay, DayOfWeek dayToFind)
        {
            int daysToAdd = ((int)dayToFind - (int)startDay.DayOfWeek + 7) % 7;
            //returns the DateTime obj for the next selected day.
            Console.WriteLine("Today is: " + startDay.ToString()
                            + "\nNext Weekday is: " + startDay.AddDays(daysToAdd).ToString());
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
            modHrs =  userHH - objToChange.Hour % 24;
            modMins = usermm - objToChange.Minute % 60;
            return objToChange.AddHours(modHrs).AddMinutes(modMins);

        }
    }
}
