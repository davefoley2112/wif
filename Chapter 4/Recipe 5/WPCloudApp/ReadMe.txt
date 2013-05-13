1. In the WpCloudApp.Web proj web.config file specify the 

<serviceKeys>
          <add serviceName="uri:wpcloudapp" serviceKey="YOUR KEY HERE" />
        </serviceKeys>
and

<trustedIssuers>
          <add issuerIdentifier="_https://YOUR NAMESPACE HERE.accesscontrol.windows.net/" name="WPCloudApp" />
        </trustedIssuers>
2. Compile the solution and run the WPCloudApp.Phone project.