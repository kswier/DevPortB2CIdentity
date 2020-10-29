using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Newtonsoft.Json;
using System.IO;

namespace APIMUserNormalization.Models
{
    public class AppSettingsFile
    {
        public AppSettings AppSettings { get; set; }
        private static IConfiguration Configuration { set; get; }

        public static AppSettings ReadFromJsonFile()
        {
            var builder = new ConfigurationBuilder();
            //default from project, has so working examples
            //builder.AddAzureAppConfiguration("Endpoint=https://usernormalizationconfiguration.azconfig.io;Id=Do9P-l6-s0:FCswtGBYgfMD7TNh+fMi;Secret=Veq5LbBxfMatEwsrpdAmvQScN6Rp0YTKMe3MbqXQhXY=");

            //2020-10-08 config from Sebastian.Adan
            builder.AddAzureAppConfiguration("Endpoint=https://apimb2cconf.azconfig.io;Id=yk8O-l6-s0:2fwdKxcKM8kBaR0Wh85v;Secret=ELiblmcJWMpyPCSb52AWeJC5veAePsBKKrHerNRKQO0=");

            //2020-10-09 config from August.Banks
            //builder.AddAzureAppConfiguration("Endpoint = https://augustapimconfig.azconfig.io;Id=bCPZ-l2-s0:mg4Rkg3xqWcaJt9eky42;Secret=i/fWaETsk4BE2Kn1SipvdcpO3kgy7CepUPSBZ7eHDXk=");
            
            Configuration = builder.Build();
            return Configuration.Get<AppSettings>();
        }
    }

    public class AppSettings
    {



        [JsonProperty(PropertyName = "TableConnection")]
        public string TableConnection { get; set; }

        [JsonProperty(PropertyName = "MigrationEnabled")]
        public bool MigrationEnabled { get; set; }

        [JsonProperty(PropertyName = "AADB2CTenantId")]
        public string AADB2CTenantId { get; set; }

        [JsonProperty(PropertyName = "AADB2CAppId")]
        public string AADB2CAppId { get; set; }

        [JsonProperty(PropertyName = "AADB2CClientSecret")]
        public string AADB2CClientSecret { get; set; }

        [JsonProperty(PropertyName = "AADB2CB2cExtensionAppClientId")]
        public string AADB2CB2cExtensionAppClientId { get; set; }

        [JsonProperty(PropertyName = "APIMSubscriptionIds")]
        public string APIMSubscriptionIds { get; set; }

        [JsonProperty(PropertyName = "APIMResourceGroups")]
        public string APIMResourceGroups { get; set; }

        [JsonProperty(PropertyName = "APIMApiManagementNames")]
        public string APIMApiManagementNames { get; set; }

        [JsonProperty(PropertyName = "APIMPostURL")]
        public string APIMPostURL { get; set; }

        [JsonProperty(PropertyName = "APIMClientID")]
        public string APIMClientID { get; set; }

        [JsonProperty(PropertyName = "APIMClientSecret")]
        public string APIMClientSecret { get; set; }

        [JsonProperty(PropertyName = "APIMResource")]
        public string APIMResource { get; set; }

        [JsonProperty(PropertyName = "APIMTenantId")]
        public string APIMTenantId { get; set; }

        [JsonProperty(PropertyName = "BackupStorageAccount")]
        public string BackupStorageAccount { get; set; }

        [JsonProperty(PropertyName = "BackupAccessKey")]
        public string BackupAccessKey { get; set; }

        [JsonProperty(PropertyName = "BackupContainer")]
        public string BackupContainer { get; set; }

        [JsonProperty(PropertyName = "RestoreSubscriptionId")]
        public string RestoreSubscriptionId { get; set; }

        [JsonProperty(PropertyName = "RestoreResourceGroup")]
        public string RestoreResourceGroup { get; set; }

        [JsonProperty(PropertyName = "RestoreServiceName")]
        public string RestoreServiceName { get; set; }

    }
}
