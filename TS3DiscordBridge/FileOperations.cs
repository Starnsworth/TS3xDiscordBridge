using System.Text.Json;

namespace TS3DiscordBridge
{
    internal class FileOperations

    //Any method or function that actually touches files belongs in here.
    {
        public readonly string directoryPath;
        public readonly string configFile;
        public FileOperations()
        {
            directoryPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TS3 x Discord Bridge");
            configFile = Path.Combine(directoryPath, "config.json");
        }

        public bool checkIfConfigExist()
        {
            if (File.Exists(configFile))
            {
                Console.WriteLine("Found config file " + configFile);
                return true;
            }
            return false;
        }

        public botConfigHandler retrieveStoredConfig()
        {
            Console.WriteLine("Using Config " + configFile);
            if (File.Exists(configFile))
            {
                string configString = File.ReadAllText(configFile);
                var options = new JsonSerializerOptions { IncludeFields = true, };
                var deserializeddata = System.Text.Json.JsonSerializer.Deserialize<botConfigHandler>(configString, options);
                    if (deserializeddata != null)
                    {
                        return deserializeddata;
                    }
                    else
                    {
                        Console.WriteLine("CONFIG READ FAILED!");
                    throw new Exception("Config Deserialise Failed.");
                }
            }
            else
            {
                throw new Exception("Config.json does not exist at path: " + configFile);
            }
        }

        public void createConfig()
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Config Directory does not exist! '\n' Creating " + directoryPath);
                Directory.CreateDirectory(directoryPath);
            }
            else if (!File.Exists(configFile))
            {
                Console.WriteLine("No Config Found at :" + configFile);
                Console.WriteLine("Creating Boilerplate config file.");
                botConfigHandler config = new botConfigHandler();
                using (File.Create(configFile))
                    DumpConfigToJSON(config);

            }
        }

        public bool DumpConfigToJSON(botConfigHandler instanceToDumpToJSON)
        {
            var options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
            string jsondump = JsonSerializer.Serialize(instanceToDumpToJSON, options);
            File.WriteAllText(configFile, jsondump);
            if (File.Exists(configFile) && File.ReadAllText(configFile) == jsondump)
            { Console.WriteLine("Config Succussfully written to disk"); return true; }
            else { Console.WriteLine("Config failed to write to disk."); return false; }
        }

        public string getBotToken()
        {
            string tokenPath = Path.Combine(directoryPath, "token.txt");
            if (File.Exists(tokenPath) == false)
            {
                Console.WriteLine("No Bot Token Found! Please place token at: " + tokenPath);
                Thread.Sleep(-1);
                throw new Exception("No Bot Token Found! Please place token at: " + tokenPath);
            }
            else
            {
                string token = File.ReadAllText(tokenPath);
                return token;
            }
        }
    }
}
