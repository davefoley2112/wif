1. Run Visual Studio in Administrative Mode
2. Check if a certificate with Subject "CN=SamlTokenSigningCertificate" is installed on your machine. Execute step 3 if the certificate is not installed.
3. Install the certificate by running Visual Studio Tools Command prompt in Administrative Mode and the following command:
makecert -r -pe -n "CN=SamlTokenSigningCertificate" -b 01/01/2011 -e 01/01/2013 -sky exchange -ss my
4. Compile and Run the application.