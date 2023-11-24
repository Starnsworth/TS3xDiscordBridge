using Microsoft.Data.Sqlite;

namespace TS3DiscordBridge
{
    public interface IDatabase
    {
        string BuildCreateTableString(bool UseIdColumnAsPrimaryKey = false); //All DBs need to be able to create their table string
        string BuildInsertString(); //All DBs need to be able to create their insert string
        string DbConnectionString { get; } //All DBs need to be able to return their connection string
        string TableName { get; } //All DBs need to be able to return their table name
        Dictionary<string, string> FieldsAndTypes { get; } //All DBs need to be able to return their fields and types

    }
    public class DatabaseHandler //Actual DB actions belong in here
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
        public async Task SaveToDB(object ObjToSave, IDatabase dbToInteractWith)
        {
            //In Here; Check that the ObjToSave has the same amount of fields as keys in the dbToInteractWith.FieldsAndTypes
            //Then we can programatically loop through all the fields of the object and add them to the command.
            //This will allow us to use the same method to save to any table in the database.




            using (var connection = new SqliteConnection(dbToInteractWith.DbConnectionString))
            {
                connection.Open();

                // Create a table if it doesn't exist
                await DbTableCreate(connection, dbToInteractWith.BuildCreateTableString());

                // If it does, add both keys and values to the database
                var y = dbToInteractWith.BuildInsertString();
                using (var cmd = new SqliteCommand(@dbToInteractWith.BuildInsertString()))
                {
                    var ListOfFields = ObjToSave.GetType().GetProperties().ToList();
                    List<System.Reflection.PropertyInfo> ListOfNotNullFields = new List<System.Reflection.PropertyInfo>(); // = ListOfFields.Where(x => x.GetValue(ObjToSave) != null).ToList();
                    foreach(var field in ListOfFields)
                    {
                        if(field.GetValue(ObjToSave) != null && !field.GetValue(ObjToSave).Equals(0UL))
                        {
                            ListOfNotNullFields.Add(field);
                        }
                    }

                    foreach (var field in ListOfNotNullFields) //build the addwithvalue command
                    {
                        var index = ListOfNotNullFields.IndexOf(field); //second half of the command
                        var dbColumnName = dbToInteractWith.FieldsAndTypes.Keys.ElementAt(index);
                        string addWithValueString = "@" + dbColumnName;
                        var x = field.GetValue(ObjToSave);
                        cmd.Parameters.AddWithValue(addWithValueString, field.GetValue(ObjToSave));
                    }

                    Thread.Sleep(100);
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
        /// <param name="dbToInteractWith">DatabaseSchema object that defines the database we want to interact with.</param>
        /// <returns>returns object if found. returns null if otherwise.</returns>
        public UserMatch SearchDBForAlias(ulong searchTerm, IDatabase dbToInteractWith)
        {
            UserMatch userMatch = new UserMatch();
            using (var connection = new SqliteConnection(dbToInteractWith.DbConnectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM {dbToInteractWith.TableName} WHERE DiscordUserId = @SearchTerm";

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {//If we have rows, we have a match.
                            while (reader.Read())
                            {
                                userMatch.DiscordUserId = Convert.ToUInt64(reader.GetInt64(reader.GetOrdinal("DiscordUserId")));
                                userMatch.DiscordUsername = reader.GetString(reader.GetOrdinal("DiscordUsername"));
                                userMatch.TeamspeakUsername = reader.GetString(reader.GetOrdinal("TeamspeakUsername"));
                                userMatch.TeamspeakUserId = reader.GetString(reader.GetOrdinal("TeamspeakUserId"));


                            };
                            //Check if the known teamspeakUserID exists in the list of users currently in TS.
                        }
                        else
                        {
                            Console.WriteLine("No rows found.");
                            return null;
                        }
                    }
                }
                return userMatch;
            }
        }

        /// <summary>
        /// Takes a tablecreate string and executes it on the DB
        /// </summary>
        /// <param name="connection">A SQLite connection object</param>
        /// <param name="CreateTableString">The string to execute on the database.</param>
        /// <returns></returns>
        async Task DbTableCreate(SqliteConnection connection, string CreateTableString)
        {
            var command = connection.CreateCommand();
            command.CommandText = CreateTableString;
            await command.ExecuteNonQueryAsync();

        }


        // Model class for database results
        public class UserMatch
        { //Order matters because inserting into the db is done by index.
            public ulong DiscordUserId { get; set; }
            public string DiscordUsername { get; set; }
            public string TeamspeakUserId { get; set; }
            public string TeamspeakUsername { get; set; }
            
        }


    }
    /// <summary>
    /// Object that defines the schema of the database that we want to interact with.
    /// Not currently implemented, but will be used to help create the database and allow uniform access.
    /// </summary>
    public class DatabaseSchema : IDatabase //Object that defines the structure of the database that we want to interact with.
    {
        public string DbConnectionString { get; } //Will ultimately be a path to the database file where the filename is user selectable.
        public string TableName { get; } // The name for the table that w want.
        public Dictionary<string, string> FieldsAndTypes { get; } //Dictionary of fields and their types. first entry will be the primary key.

        public DatabaseSchema(string dbName, string tableName, Dictionary<string, string> dbFields)
        {
            DbConnectionString = "Data Source=" + Path.Combine(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TS3 x Discord Bridge"), dbName + ".db") + ";"; //look into making this nicer
            TableName = tableName;
            FieldsAndTypes = dbFields;
        }


        /// <summary>
        /// Builds a string that can be used to create a table in the database.
        /// </summary>
        /// <param name="UseIdColumnAsPrimaryKey">Whether we use an autoincrementing id as the primary key or not.</param>
        /// <returns></returns>
        public string BuildCreateTableString(bool UseIdColumnAsPrimaryKey = false)
        {
            string createTableString = "CREATE TABLE IF NOT EXISTS " + TableName + " (";
            if (UseIdColumnAsPrimaryKey)
            {
                createTableString += "id INTEGER PRIMARY KEY AUTOINCREMENT, ";
                foreach (KeyValuePair<string, string> field in FieldsAndTypes)
                {
                    createTableString += field.Key + " " + field.Value + ", ";
                }
            }
            else
            {
                foreach (KeyValuePair<string, string> field in FieldsAndTypes.Take(1))
                {
                    createTableString += field.Key + " " + field.Value + " PRIMARY KEY UNIQUE, ";
                }
                foreach (KeyValuePair<string, string> field in FieldsAndTypes.Skip(1))
                {
                    createTableString += field.Key + " " + field.Value + ", ";
                }
            }
            createTableString = createTableString.Remove(createTableString.Length - 2); //Remove the last comma and space.
            createTableString += ")";
            return createTableString;
        }
        /// <summary>
        /// Builds a string that can be used to insert a row into the database.
        /// used with AddWithValue to add the values to the command.
        /// </summary>
        /// <returns>A string to pass to the db engine.</returns>
        public string BuildInsertString()
        {
            string insertString = "INSERT INTO " + TableName + " (";
            foreach (KeyValuePair<string, string> field in FieldsAndTypes)
            {
                insertString += field.Key + ", ";
            }
            insertString = insertString.Remove(insertString.Length - 2); //Remove the last comma and space.
            insertString += ") VALUES (";
            foreach (KeyValuePair<string, string> field in FieldsAndTypes)
            {
                insertString += "@" + field.Key + ", ";
            }
            insertString = insertString.Remove(insertString.Length - 2); //Remove the last comma and space.
            insertString += ")";
            return insertString;
        }
    }

}
