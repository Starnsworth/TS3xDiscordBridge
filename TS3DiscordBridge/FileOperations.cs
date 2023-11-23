using System.Text.Json;

namespace TS3DiscordBridge
{
    public class FileOperations

    //Any method or function that actually touches files belongs in here.
    {
        private readonly botConfig _botConfig;
         readonly string directoryPath;
         readonly string configFile;
        readonly string adminConfigFile;

        public string DirectoryPath { get => directoryPath; }
        public string AdminConfigFile { get => adminConfigFile; }
        public string ConfigFile { get => configFile; }

        public FileOperations(botConfig config)
        {
            _botConfig = config;
            directoryPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TS3 x Discord Bridge");
            configFile = Path.Combine(directoryPath, "config.json");
            adminConfigFile = Path.Combine(DirectoryPath, "adminconfig.json");
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

        public void retrieveStoredConfig()
        {
            Console.WriteLine("Using Config " + configFile);
            if (File.Exists(configFile))
            {
                string configString = File.ReadAllText(configFile);
                var options = new JsonSerializerOptions { IncludeFields = true, };
                var deserializeddata = JsonSerializer.Deserialize<botConfig>(configString, options);
                if (deserializeddata != null)
                {
                   _botConfig.setConfigValues(deserializeddata);
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

        public async Task createConfig()
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
                var objToDump = _botConfig;
                using (File.Create(configFile))
                await DumpConfigToJSON(objToDump);

            }
        }

        public async Task DumpConfigToJSON(botConfig instanceToDumpToJSON)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, };
            string jsondump = JsonSerializer.Serialize(instanceToDumpToJSON, options);
            await Task.Run(() => File.WriteAllText(configFile, jsondump));
            if (File.Exists(configFile) && File.ReadAllText(configFile) == jsondump)
            { Console.WriteLine("Config Succussfully written to disk"); }
            else { Console.WriteLine("Config failed to write to disk."); }
        }

        public async Task DumpConfigToJSON(adminInternalConfig instanceToDumpToJSON)
        {

            var options = new JsonSerializerOptions { WriteIndented = true, };
            string jsondump = JsonSerializer.Serialize(instanceToDumpToJSON, options);
            await Task.Run(() => File.WriteAllText(adminConfigFile, jsondump));
            if (File.Exists(adminConfigFile) && File.ReadAllText(adminConfigFile) == jsondump)
            { Console.WriteLine("Admin Config Succussfully written to disk"); }
            else { Console.WriteLine("Admin Config failed to write to disk."); }
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

        public void writeToFile(string destinationFileName, string dataToWrite, string filetype = ".txt")
        {
            var writeDestination = Path.Combine(directoryPath, destinationFileName + filetype);
            File.WriteAllText(writeDestination, dataToWrite);
        }



    }
}
