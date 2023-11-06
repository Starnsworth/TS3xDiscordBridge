using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

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
    }
}
