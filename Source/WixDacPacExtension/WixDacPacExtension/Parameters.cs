using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;

namespace WixDacPacExtension
{
    public class Parameters
    {
        /// <summary>
        /// The action that is passed to SqlPackage.exe via /ACTION:
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// the path to SqlPackage.exe.  In the future, may give the option to search for it
        /// </summary>
        public string SqlPackagePath { get; set; }

        /// <summary>
        /// the path to the DACPAC to install
        /// </summary>
        public string DacPacPath { get; set; }

        /// <summary>
        /// the path to the log file that is written with all the output from SqlPackage.exe.
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// The target database server name where the dacpac should be processed.  Passed to SqlPackage.exe via /TargetServerName:
        /// </summary>
        public string TargetServerName { get; set; }

        /// <summary>
        /// The target database name where the dacpac should install.  Passed to SqlPackage.exe via /TargetDatabaseName:
        /// </summary>
        public string TargetDatabaseName { get; set; }

        /// <summary>
        /// Any additional parameters that should be passed to SqlPackage.exe such as "/p:RegisterDataTierApplication=True /p:BlockWhenDriftDetected=False"
        /// </summary>
        public string OtherParameters { get; set; }

        /// <summary>
        /// set to true to show the output of the SqlPackage in real time
        /// </summary>
        public bool ShowUI { get; set; }

        /// <summary>
        /// has no effect if ShowUI = false.  set to true to not automatically close the form if there was an error.
        /// </summary>
        public bool KeepOpenOnError { get; set; }

        /// <summary>
        /// Set this string to the desired title of the UI window
        /// </summary>
        public string UiTitle { get; set; }

        public Parameters(Session session)
        {
            // read the parameters from the session
            Action = GetParameter<string>(session, "Action", false, "Publish");
            SqlPackagePath = GetParameter<string>(session, "SqlPackagePath", true, null);
            DacPacPath = GetParameter<string>(session, "DacPacPath", true, null);
            LogFilePath = GetParameter<string>(session, "LogFilePath", true, null);
            TargetServerName = GetParameter<string>(session, "TargetServerName", true, null);
            TargetDatabaseName = GetParameter<string>(session, "TargetDatabaseName", true, null);
            UiTitle = GetParameter<string>(session, "UiTitle", false, "Installation of " + TargetDatabaseName + " DB");
            OtherParameters = GetParameter<string>(session, "OtherParameters", true, string.Empty);
            ShowUI = GetParameter<bool>(session, "ShowUI", false, false);
            KeepOpenOnError = GetParameter<bool>(session, "KeepOpenOnError", false, false);

            // validate what can be validated
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            //validate paths exist
            if (!File.Exists(SqlPackagePath))
                throw new ArgumentException("SqlPackagePath does not exist: " + SqlPackagePath);
            if (!File.Exists(DacPacPath))
                throw new ArgumentException("DacPacPath does not exist: " + DacPacPath);

            // validate log file folder exists (the file itself does not have to exist)
            var logFile = new FileInfo(LogFilePath);
            if (!logFile.Directory.Exists)
                throw new ArgumentException("LogFilePath is invalid: " + LogFilePath);
        }

        private static T GetParameter<T>(Session session, string parameterName, bool isRequired, T defaultValue)
        {
            // verify the session is not null - should never be null
            if (session== null)
                throw new ArgumentException("Session is null");

            try
            {
                // try to get the data
                var data = (T)Convert.ChangeType(session.CustomActionData[parameterName], typeof(T));

                // if we got here, we have the data so return it
                return data;
            }
            catch (KeyNotFoundException)
            {
                // if we get here, the data was not provided.  Check to see if it is required.
                if (isRequired)
                    throw new ArgumentException(string.Format("Required parameter \"{0}\" is missing, aborting.", parameterName));

                // not required, return the default
                return defaultValue;
            }
        }

        public override string ToString()
        {
            return string.Format("Action={0}\nSqlPackage={0}\nDacPacPath={1}\nTargetServerName={2}\nTargetServerName={3}\nTargetDatabaseName={4}\nOtherParameters={5}",
                Action,
                SqlPackagePath,
                DacPacPath,
                TargetServerName,
                TargetDatabaseName,
                OtherParameters
                );
        }
    }
}
