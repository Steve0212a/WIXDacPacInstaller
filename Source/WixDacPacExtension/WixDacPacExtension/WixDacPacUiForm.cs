using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WixDacPacExtension
{
    public partial class WixDacPacUiForm : Form
    {
        private readonly Parameters parameters;
        public bool SqlPackageWasSuccessful { get; private set; }

        public WixDacPacUiForm(Parameters parameters)
        {
            this.parameters = parameters;
            InitializeComponent();

            // set up the window
            this.Text = parameters.UiTitle;
            this.Location = new Point(10,10);
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width/3, (int)(Screen.PrimaryScreen.Bounds.Height * 0.67));
        }

        private void WixDacPacUiForm_Load(object sender, EventArgs e)
        {
            // start it
            Start(true);
        }

        public void Start(bool runAsync)
        {
            // must be set in load whether it is visible or not
            this.Visible = parameters.ShowUI;

            // run the process on its own thread
            var action = new Action(
                () =>
                {
                    // build the command line parameters to sql package and create the process object to launch it
                    var commandLineArguments =
                        string.Format(
                            "/Action:{0} \"/SourceFile:{1}\" \"/TargetServerName:{2}\" \"/TargetDatabaseName:{3}\" {4}",
                            parameters.Action,
                            parameters.DacPacPath,
                            parameters.TargetServerName,
                            parameters.TargetDatabaseName,
                            parameters.OtherParameters
                        );
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = parameters.SqlPackagePath,
                            Arguments = commandLineArguments,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };

                    // log the arguments into the log file
                    ProcessOnOutputDataReceived(false, "Parameter to Extension=" + parameters);
                    ProcessOnOutputDataReceived(false, "arguments=" + process.StartInfo.Arguments);

                    // hook up to events to read output
                    process.OutputDataReceived += (s, args) => ProcessOnOutputDataReceived(false, args.Data);
                    process.ErrorDataReceived += (s, args) => ProcessOnOutputDataReceived(true, args.Data);

                    // Asynchronously read the standard output of the spawned process. This raises OutputDataReceived events for each line of output.
                    process.Start();

                    // read the output
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // wait for SqlPackage.exe to exit
                    // can't call process.WaitForExit() as it will lock up the thread and the UI will lock up
                    while (!process.HasExited)
                    {
                        Thread.Sleep(250);
                    }

                    // log the exit code
                    ProcessOnOutputDataReceived(false, "process.ExitCode=" + process.ExitCode);
                    SqlPackageWasSuccessful = process.ExitCode == 0;


                    // close the form if we should
                    if (runAsync)
                        if (!(!SqlPackageWasSuccessful && parameters.KeepOpenOnError))
                        {
                            // sleep for 3 seconds for the user to see the output
                            Thread.Sleep(3000);

                            // close the window on the correct thread
                            this.Invoke(new Action(this.Close));
                        }
                        else
                        {
                            // close the window on the correct thread
                            this.Invoke(new Action(
                                () =>
                                {
                                    panelCloseButton.Visible = true;
                                }));
                        }
                });

            if (runAsync)
                Task.Run(action);
            else
                action();
        }

        private readonly object dataLogLock = new object();
        private void ProcessOnOutputDataReceived(bool isError, string data)
        {
            // see if a blank line was written to error and ignore it
            if (isError && string.IsNullOrWhiteSpace(data))
                return;

            // it is possible to get an error and a success simultaneously which would cause
            // the StreamWrite to fail.
            lock (dataLogLock)
            {
                // get the new line and add it to the variable
                var newLine = string.Format("{0}\t{1}\t{2}\r\n", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff"), isError ? "ERROR" : "     ", data);

                if (parameters.ShowUI)
                    this.Invoke(
                        new Action(
                            () =>
                            {
                                txtOutput.SelectionStart = txtOutput.TextLength;
                                txtOutput.SelectionLength = 0;

                                txtOutput.SelectionColor = isError ? Color.Red : txtOutput.ForeColor;
                                txtOutput.AppendText(newLine);
                                txtOutput.SelectionColor = txtOutput.ForeColor;
                            }));

                // write the text to the given log file path
                using (var file = new System.IO.StreamWriter(parameters.LogFilePath, true))
                {
                    file.Write(newLine);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // close the window on the correct thread
            this.Invoke(new Action(this.Close));
        }
    }
}
