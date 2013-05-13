1. Make sure that the certificate used for signing and encryption is properly installed in the relevant store location.
2. Make sure that the process has sufficient privileges to read the certificate.
3. Update the web.config files in both the WebSTS and WebRP projects to use the appropriate certificates.
4. Make sure that the Claims 2 Windows Token Service is running.
5. Verify that the process account is part of the allowed callers list in the c2wtshost.exe.config file.
6. Make sure that you are running the Solution in Admin mode.
7. You should be connected to the Domain Controller.
8. Provide a valid UPN address in the ImpersonatingUPN app settings key.
9. Compile the solution. Run the WebSTS project (Make sure Default.aspx is set as the starting page).