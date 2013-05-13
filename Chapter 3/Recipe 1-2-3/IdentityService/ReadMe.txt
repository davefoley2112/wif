1. Make sure that the certificate used for signing and encryption is properly installed in the relevant store location.
2. Make sure that the process has sufficient privileges to read the certificate.
3. Update the web.config files in both the WebSTS and WebRP projects to use the appropriate certificates.
4. Compile the solution. Run the WebSTS project (Make sure Default.aspx is set as the starting page).