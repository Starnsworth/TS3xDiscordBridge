# TS3DiscordBridge
Discord x TS3 Bridge - For the jips late to ARMA
Author: Starns - 2023

Intent: Provides a connection between an enrolled discord server and an appropriately setup TS3 server.
         The connection to TS3 is done via SSH on port 10022 and makes use of the below listed TS3 permissions & their powers.
         
Teamspeak Permissions: b_virtualserver_select,  b_virtualserver_client_list,
                       i_channel_subscribe_power =10 , i_channel_needed_subscribe_power = 10


## CORE FUNCTIONALITY
==DONE: Read Settings from disk using botConfigHandler() and store somewhere

==DONE: Do discord logon stuff

==DONE: Check if a message from watched user has been sent in watched channel in last 6 hours 

function: Sundays & Tuesdays, check in 3x to ensure valid data.

TODO: Timer Functionality to check in at the correct times.

TODO: slash command so staff can create custom times to sound off.
     
function: Compare users & sound off.      

TODO: Method to Build list of users who have reacted to the noti.

TODO: Method to build list of users currently in teamspeak - overall presence in the TS is good enough, we dont need a channel ID.

TODO: Parse that huge string that gets returned on 'clientlist -uid' Need Nickname & uid.

TODO: Appropriate storage of information. Guess JSON would work with a properly implimented 'users' class. Look to migrate to something else. TinyDB? Mongo? maria?
             
TODO: Compare User lists, Ping discord users where no match is found.

TODO: Cleanup debug behaviour.

### Stretch Goals;
TODO: Properly Impliment logging, failure states, and exceptions.

TODO: Refactor the code so it's nicer... But fuck that.

### Really Stretchy Goals (Like Probably not happening);
TODO: Impliment a webdashboard to easily relate discord users to teamspeak UUIDs

TODO: Jury rig into the training notifications and maybe ping users for that. Also allows trainers to easily get a list of attendies and then possibly loop into the armory rewards system.



# Dependencies
.Net 6
