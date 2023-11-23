using Microsoft.Data.Sqlite;
using static TS3DiscordBridge.DatabaseHandler;

namespace TS3DiscordBridge
{
    public interface IDatabase
    {
        Task SaveToDB(UserMatch ObjToSave); //Allow a schema to be passed to this method to create the table if it doesn't exist.
        bool SearchDatabase(ulong searchTerm);
        Task DbTableCreate(SqliteConnection connection);
    }
    public class DatabaseHandler : IDatabase //Actual DB actions belong in here
    {
        public readonly string connectionString;
        public readonly FileOperations _fileio;
        public DatabaseHandler(FileOperations fileio)
        {
            _fileio = fileio;
            connectionString = "Data Source=" + Path.Combine(_fileio.DirectoryPath, "shoutbot.db") + ";"; //Maybe make DBname configurable?
        }


        /// <summary>
        /// Just adds the user to the database. No comparison is done.
        /// </summary>
        /// <returns>
        /// Returns the users who weren't found in the teamspeak.
        /// </returns>

        // TODO: Send a DM to an admin with the list of users not found + ts users not matched with discord users for manual alias updates.
        public async Task SaveToDB(UserMatch ObjToSave)
        {

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Create a table if it doesn't exist
                await DbTableCreate(connection);

                // If it does, add both keys and values to the database
                using (var cmd = new SqliteCommand(@"
                        INSERT INTO UserMatches (DiscordUsername, DiscordUserId, TeamspeakUsername, TeamspeakUserId)
                        VALUES (@DiscordUsername, @DiscordUserId, @TeamspeakUsername, @TeamspeakUserId)"))
                {
                    cmd.Parameters.AddWithValue("@DiscordUsername", ObjToSave.DiscordUsername);
                    cmd.Parameters.AddWithValue("@DiscordUserId", ObjToSave.DiscordUserId);
                    cmd.Parameters.AddWithValue("@TeamspeakUsername", ObjToSave.DiscordUsername);
                    cmd.Parameters.AddWithValue("@TeamspeakUserId", ObjToSave.TeamspeakUserId);

                    using (var transaction = connection.BeginTransaction())
                    {
                        cmd.Connection = connection;
                        cmd.Transaction = transaction;

                        // Execute the command
                        await cmd.ExecuteNonQueryAsync();

                        // Commit the transaction
                        await transaction.CommitAsync();
                    }
                }
            }
        }

        /// <summary>
        /// searches the database for a match to the search term. If a match is found, return true. If no match is found, return false.
        /// </summary>
        /// <param name="searchTerm">ulong discord Unique User ID</param>
        /// <returns></returns>
        public bool SearchDatabase(ulong searchTerm)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SqliteCommand(@"
            SELECT * FROM UserMatches
            WHERE DiscordUsername = @SearchTerm OR TeamspeakUsername = @SearchTerm OR DiscordUserId = @SearchTerm OR TeamspeakUserId = @SearchTerm "))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {//If we have rows, we have a match.
                            while (reader.Read())
                            {
                                var userMatch = new UserMatch
                                {
                                    DiscordUserId = Convert.ToUInt64(reader.GetInt64(reader.GetOrdinal("DiscordUserId"))),
                                    DiscordUsername = reader.GetString(reader.GetOrdinal("DiscordUsername")),
                                    TeamspeakUsername = reader.GetString(reader.GetOrdinal("TeamspeakUsername")),
                                    TeamspeakUserId = reader.GetString(reader.GetOrdinal("TeamspeakUserId"))
                                };
                                //Check if the known teamspeakUserID exists in the list of users currently in TS.
                            }
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
        }


        public async Task DbTableCreate(SqliteConnection connection)
        //Allow this to take a schema object so we can build the string dynamically and make it more modular.
        {
            var command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS UserMatches (
                    DiscordUserId INTEGER PRIMARY KEY UNIQUE,
                    DiscordUsername TEXT,
                    TeamspeakUsername TEXT,
                    TeamspeakUserId TEXT UNIQUE
                )";
            await command.ExecuteNonQueryAsync();

        }
        // Model class for database results
        public class UserMatch
        {
            public string DiscordUsername { get; set; }
            public ulong DiscordUserId { get; set; }
            public string TeamspeakUsername { get; set; }
            public string TeamspeakUserId { get; set; }
        }

        public class DbSchema //A schema object that we'd pass to the databasecreate method to create the table.
        {
            
        }

        

    }
    /// <summary>
    /// Object that defines the schema of the database that we want to interact with.
    /// Not currently implemented, but will be used to help create the database and allow uniform access.
    /// </summary>
    public class DatabaseSchema 
        //Object that defines the schema of the database that we want to interact with.
    {
        string? ConnectionString { get; set; } //Will ultimately be a path to the database file where the filename is user selectable.
        string? TableName { get; set; } // The name for the table that we'll be creating.
        Dictionary<string, string>? Fields { get; set; } //Dictionary of fields and their types. first entry will be the primary key.

        public DatabaseSchema()
        {
            //Need to work out a way to define the connection string using the field directoryPath in FileOperations.
            //using _fileio requires an object reference and I'm not sure how to get that here.
            //Orignally was going to just use the databaseschema as part of the DI but I dont think that's necessary.

            
        }
    }
    
}
