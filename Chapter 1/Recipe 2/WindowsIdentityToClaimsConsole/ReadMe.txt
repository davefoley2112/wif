1. Run Visual Studio in Administrative Mode
2. Make sure you are connected to the domain if your credentials are provided by Active Directory.
3. Make sure you have appropriate rights to create a file in C Drive. Else change the path in code to point to a location where you have the rights.
4. Install the certificate by running Visual Studio Tools Command prompt in Administrative Mode and using the following command:
makecert -r -pe -n "CN=SamlTokenSigningCertificate" -b 01/01/2011 -e 01/01/2013 -sky exchange -ss my
5. Compile and Run the application.
