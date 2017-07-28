# WIXDacPacInstaller
Have you ever needed to install a SQL Server DACPAC as part of an MSI? The biggest problem is that if you manually run SqlPackage.exe, the output flies by on the screen and then the window closes without a chance to see any error messages. I googled for any way to log this output, but there was nothing. That led me to write this Wix Extension to solve the problem.

This code is a WIX Extension that helps to solve that problem. Using this extension, you can have the output of SqlPackage.exe written to a log file and optionally still shown to the screen if you desire.

Please check out the documentation for an example of how to work with this plug in.

Lastly, if there is something you would like to see added to this project, please let me know.

Thanks.
