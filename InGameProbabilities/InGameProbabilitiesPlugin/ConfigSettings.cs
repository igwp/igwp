
namespace InGameProbabilitiesPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;

    internal class ConfigSettings
    {
        private const string PredictionServiceHostKey = "PredictionServiceHost";

        private const string PredictionServicePortKey = "PredictionServicePort";

        private const string GameHookPortKey = "GameHookPort";

        public string PredictionServiceHost { get; private set; }

        public ushort PredictionServicePort { get; private set; }

        public ushort GameHookPort { get; private set; }

        public IDictionary<string, int> ChampIds { get; private set; }

        public ConfigSettings()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var config = ConfigurationManager.OpenExeConfiguration(location);

            try
            {
                var settings = config.AppSettings;

                this.PredictionServiceHost = settings.Settings[ConfigSettings.PredictionServiceHostKey].Value;
                this.PredictionServicePort = ushort.Parse(settings.Settings[ConfigSettings.PredictionServicePortKey].Value);
                this.GameHookPort = ushort.Parse(settings.Settings[ConfigSettings.GameHookPortKey].Value);
            }
            catch (InvalidCastException e)
            {
                throw new ConfigurationErrorsException("config file does not have an appSettings section of the correct type", e);
            }
            catch (NullReferenceException e)
            {
                throw new ConfigurationErrorsException("configuration file does not have all expected values!", e);
            }
            catch (FormatException e)
            {
                throw new ConfigurationErrorsException("some properties are not correctly ushort values!", e);
            }

            using (var aFile = File.OpenText($"{Path.GetDirectoryName(location)}/constants.json"))
            {
                var jsonData = aFile.ReadToEnd();

                this.ChampIds = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonData);
            }
        }
    }
}
