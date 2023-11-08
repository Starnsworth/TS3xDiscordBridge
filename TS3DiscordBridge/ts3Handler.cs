using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static TS3DiscordBridge.ts3Handler;

namespace TS3DiscordBridge
{
    public class ts3Handler
    {
        //Handle the pre-connection data and the post connection data here.
        static serverConnectionData? connectionData;
        
        internal class serverConnectionData
        {
            string serverHostAddress;
            int virtualServerID;

            internal string getServerHostAddress()
            {
                return serverHostAddress;
            }
            internal int getVirtualServerID()
            {
                return virtualServerID;
            }

            internal serverConnectionData(string serverHostAddress, int virtualServerID, int chanID)
            {
                this.serverHostAddress = serverHostAddress;
                this.virtualServerID = virtualServerID;
            }
         }
        
        
        internal static void serverQueryErrorHandler(string serverQueryError)
        {
         throw new Exception(serverQueryError);
         //call this method when the serverQuery session gives us an issue. If we need to do more than just throw ~~hands~~ error then it can be handled here as opposed to inline.
            
        }

        //------------------------------------------------------------------//
        //Handle connecting to ServerQuery and retrieving output from here.

        internal class serverQueryConnectionHandler 
        {
            internal class chanUserListHandler
            {
                string chanName;
                int chanUserCount;
                List<string> chanUserList;

                public chanUserListHandler()
                {
                    if (connectionData != null) { serverQueryConnector connector = new serverQueryConnector(connectionData.getServerHostAddress(), connectionData.getVirtualServerID()); }
                    throw new Exception("No Connection Data in object 'connectionData'. Has setupUsingExistingData method been run?");

                    //Telnet to the provided channel as per serverQueryConnector then parse data into above vars. Once parsed, these vars can be pushed to disk if need be.
                }

            }

            internal class serverQueryConnector //like actually connect to serverQuery and find that data you need.
            {

                string serverHostAddress;
                int virtualServerID;

                internal serverQueryConnector(string serverHostAddress, int virtualServerID)
                {
                    this.serverHostAddress = serverHostAddress;
                    this.virtualServerID = virtualServerID;

                    //save the vars to disk for use later.
                    //usage : serverQueryConnector(getServerHostAddress(), getVirtualServerID(),getChannelID());
                }

            }


        }
    }
    
}
