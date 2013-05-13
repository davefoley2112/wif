1. Run Visual Studio 2010 in Admin Mode.
2. Make sure that the certificate used in the Federation Metadata Generator Tool is deployed correctly.
3. The MvcSTS uses forms authentication and the db file is attached to SQL Express in the config. Make sure to setup the DB correctly else you might get errors during login.
4. There is an occasional problem with the cookie that prevents redirection after login. Make sure to clear any cookies and do a clean rebuild in case the redirection to the MvcClient fails after login.
5. Compile and Run the MvcClient project.