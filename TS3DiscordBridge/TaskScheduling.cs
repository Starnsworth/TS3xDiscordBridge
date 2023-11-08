using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3DiscordBridge
{
    /*
     * function: Sundays & Tuesdays, check in 3x to ensure valid data.
     * TODO: Timer Functionality to check in at the correct times.
     *      TODO: slash command so staff can create custom times to sound off.
     */


    internal class TaskScheduling
    {
        
        DateTime requiredDateTime = new DateTime();

        //Need to know the day
        public TaskScheduling()
        {
            //Set requiredDateTime to the closest tuesday or sunday.
            DateTime currentDateTime= DateTime.Now;

            var currentDay = currentDateTime.DayOfWeek;
            Console.WriteLine(currentDay.ToString());
            Thread.Sleep(100);

            if (currentDateTime.DayOfWeek.ToString() == "Monday" || currentDateTime.DayOfWeek.ToString() == "Tuesday")
            {
                //set required DateTime
            }


        }

        //need to know the time

    }
}
