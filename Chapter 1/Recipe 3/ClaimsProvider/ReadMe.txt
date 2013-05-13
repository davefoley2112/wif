1. Run Visual Studio in Administrative Mode
2. Make sure you are connected to the domain if your credentials are provided by Active Directory.
3. Check if a certificate with Subject "CN=SamlTokenSigningCertificate" is installed on your machine. Execute step 4 if the certificate is not installed.
4. Install the certificate by running Visual Studio Tools Command prompt in Administrative Mode and the following command:
makecert -r -pe -n "CN=SamlTokenSigningCertificate" -b 01/01/2011 -e 01/01/2013 -sky exchange -ss my
5. Check if the certificate with subject "CN=localhost" is installed in "my" store. Execute step 4 replacing the subject with "CN=localhost" if the certificate is missing from the store.
6. Run the ClaimsProvider project.
7. Run the client.