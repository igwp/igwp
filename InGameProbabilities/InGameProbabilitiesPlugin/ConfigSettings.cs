
namespace InGameProbabilitiesPlugin
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;

    internal class ConfigSettings
    {
        private const string PredictionServiceHostKey = "PredictionServiceHost";

        private const string PredictionServicePortKey = "PredictionServicePort";

        private const string ApiKeyKey = "ApiKey";

        private const string GameHookPortKey = "GameHookPort";

        public string PredictionServiceHost { get; private set; }

        public ushort PredictionServicePort { get; private set; }

        public string ApiKey { get; private set; }

        public ushort GameHookPort { get; private set; }

        public ConfigSettings()
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            try
            {
                var settings = config.AppSettings;
                this.PredictionServiceHost = settings.Settings[PredictionServiceHostKey].Value;
                this.PredictionServicePort = ushort.Parse(settings.Settings[PredictionServicePortKey].Value);
                this.ApiKey = settings.Settings[ApiKeyKey].Value;
                this.GameHookPort = ushort.Parse(settings.Settings[GameHookPortKey].Value);
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
        }
    }
}
