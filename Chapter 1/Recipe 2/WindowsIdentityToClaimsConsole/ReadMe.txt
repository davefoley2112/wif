1. Run Visual Studio in Administrative Mode

2. Make sure you are connected to the domain if your credentials are provided by Active Directory.

3. Make sure you have appropriate rights to create a file in C Drive. Else change the path in code to point to a location where you have the rights.

4. Install the certificate by running Visual Studio Tools Command prompt in Administrative Mode and using the following command:
makecert -r -pe -n "CN=SamlTokenSigningCertificate" -b 01/01/2011 -e 01/01/2013 -sky exchange -ss my

	-r		Creates a self-signed certificate.

	-pe		Marks the generated private key as exportable. This allows the private key to be included in the certificate.

	-n		Specifies the subject's certificate name. This name must conform to the X.500 standard. The simplest method is to specify the name in double quotes, preceded by CN=; for example, -n "CN=myName".

	-b		Specifies the start of the validity period. Defaults to the current date.

	-e		Specifies the end of the validity period. Defaults to 12/31/2039 11:59:59 GMT.

	-sky	Specifies the subject's key type, which must be one of the following: 
				* signature (which indicates that the key is used for a digital signature)
				* exchange (which indicates that the key is used for key encryption and key exchange)
				* an integer that represents a provider type. By default, you can pass 1 for an exchange key or 2 for a signature key.	

	-ss		Specifies the subject's certificate store name that stores the output certificate.
			For an example that displays the names of all standard certificate stores found on a local system, see the X509Store.Name property.

5. Compile and Run the application.