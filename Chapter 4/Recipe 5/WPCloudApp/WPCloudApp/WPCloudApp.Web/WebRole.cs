namespace WPCloudApp.Web
{
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using WPCloudApp.Web.Infrastructure;

    public class WebRole : RoleEntryPoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method initializes the Web role.")]
        public override bool OnStart()
        {
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) =>
                {
                    if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        // The corresponding configuration setting has changed, propagate the value
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            // In this case, the change to the storage account credentials in the
                            // service configuration is significant enough that the role needs to be
                            // recycled in order to use the latest settings. (for example, the 
                            // endpoint has changed)
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });

            // If no valid WIF settings are found in the Web Role configuration, then the Web Role shouldn't start
            if (!UpdateWifSettings())
            {
                return false;
            }

            return base.OnStart();
        }

        [System.Security.Permissions.EnvironmentPermission(System.Security.Permissions.SecurityAction.LinkDemand)]
        private static bool UpdateWifSettings()
        {
            using (var server = new Microsoft.Web.Administration.ServerManager())
            {
                var siteNameFromServiceModel = "Web";
                var siteName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", RoleEnvironment.CurrentRoleInstance.Id, siteNameFromServiceModel);

                var configFilePath = string.Format(System.Globalization.CultureInfo.InvariantCulture, @"{0}\Web.config", server.Sites[siteName].Applications[0].VirtualDirectories[0].PhysicalPath);
                var xml = System.Xml.Linq.XElement.Load(configFilePath);
                var identityModelService = xml.Element("microsoft.identityModel").Element("service");

                if (UpdateAttributeWithRoleSetting(identityModelService.Element("audienceUris").Element("add").Attribute("value"), "realm") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerTokenResolver").Element("serviceKeys").Element("add").Attribute("serviceName"), "realm") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerTokenResolver").Element("serviceKeys").Element("add").Attribute("serviceKey"), "serviceKey") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerNameRegistry").Element("trustedIssuers").Element("add").Attribute("issuerIdentifier"), "trustedIssuersIdentifier") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerNameRegistry").Element("trustedIssuers").Element("add").Attribute("name"), "trustedIssuerName"))
                {
                    xml.Save(configFilePath);
                    return true;
                }

                return false;
            }
        }

        private static bool UpdateAttributeWithRoleSetting(System.Xml.Linq.XAttribute attribute, string settingName)
        {
            var settingValue = ConfigReader.GetConfigValue(settingName, false);
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                attribute.Value = settingValue;
            }
            else if (string.IsNullOrWhiteSpace(attribute.Value))
            {
                return false;
            }

            return true;
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
