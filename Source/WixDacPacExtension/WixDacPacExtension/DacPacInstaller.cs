using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;

namespace WixDacPacExtension
{
    public class DacPacInstaller
    {
        /// <summary>
        /// this property will store the output text from SqlPackage.exe
        /// </summary>
        public string OutputText { get; set; }

        /// <summary>
        /// The collection of parameters passed to this extension
        /// </summary>
        private Parameters Parameters { get; set; }

        /// <summary>
        /// the session from the MSI
        /// </summary>
        private Session Session { get; set; }

        public DacPacInstaller(Session session)
        {
            // get the parameters from the session
            this.Session = session;
            Parameters = new Parameters(Session);

            // log the parameters to the MSI log
            Session.Log(Parameters.ToString());

            // initialize the output
            OutputText = string.Empty;
        }

        public bool RunDacPac()
        {
            // create the form that will do all the work
            using (var form = new WixDacPacUiForm(Parameters))
            {
                if (Parameters.ShowUI)
                    form.ShowDialog();
                else
                    form.Start(false);

                // return path to backup file
                return form.SqlPackageWasSuccessful;
            }
        }
    }

}