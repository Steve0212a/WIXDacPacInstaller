using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace WixDacPacExtension
{
    /// <summary>
    /// This class is a wrapper to running SqlPackage.exe to utilize a DACPAC during MSI installation
    /// </summary>
    public class WixDacPac
    {
        /// <summary>
        /// Executes the installation via the given parameters
        /// </summary>
        /// <param name="session">the windows installer current session</param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult Execute(Session session)
        {
            // launch the debugger for troubleshooting
            // Debugger.Launch();

            session.Log("Begin Execute - WixDacPac");
            DacPacInstaller dacPacInstaller = null;
            try
            {
                // build the dac pac installer
                dacPacInstaller = new DacPacInstaller(session);

                // run SqlPackage.exe to install the DACPAC
                if (!dacPacInstaller.RunDacPac())
                {
                    throw new ApplicationException("SqlPackage.exe returned a non-zero exit code, aborting.");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Installation failed - the process will not roll back.\r\n\r\nException: " + exc.Message,
                    "DACPAC Installation Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                session.Log("End Execute - WixDacPac");
                return ActionResult.Failure;
            }

            // if we got here, all is good, return success
            session.Log("End Execute - WixDacPac");
            return ActionResult.Success;
        }
    }
}
