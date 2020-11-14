using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace SAR
{
    public class SAR : ServiceBase
    {
        private const string LogName = "Service Application Runner";
        private const string LogSrc  = "Service Application Runner";

        private IntPtr currentProcHandle = IntPtr.Zero;
        private EventLog eventLog;
        public SAR()
        {
            if (!EventLog.SourceExists(LogSrc))
            {
                EventLog.CreateEventSource(LogSrc, LogName);
            }
            eventLog = new EventLog(LogName)
            {
                Source = LogSrc
            };
        }

        protected override void OnStart(string[] args)
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Length < 2)
            {
                var message = "SAR Can not start process: too few arguments specified";
                eventLog.WriteEntry(message, EventLogEntryType.Error);
                throw new Exception(message);
            }
            string[] processWithArgs = new string[cmdArgs.Length - 1];
            Array.Copy(cmdArgs, 1, processWithArgs, 0, processWithArgs.Length);
            string cmd = string.Join(" ", processWithArgs);
            var info = ProcessCreator.CreateProcess(0, cmd);
            if (info.ProcHandle == IntPtr.Zero)
            {
                string message = "SAR can not start process: " + cmd;
                eventLog.WriteEntry(message, EventLogEntryType.Error);
                throw new Exception(message);
            }
            else
            {
                currentProcHandle = info.ProcHandle;
                eventLog.WriteEntry(string.Format("SAR started process {0} with PID {1}", info.ProcName, info.PID), EventLogEntryType.Information);
            }
        }

        protected override void OnStop()
        {
            if (currentProcHandle != IntPtr.Zero) {
                ProcessCreator.StopProcess(currentProcHandle);
                currentProcHandle = IntPtr.Zero;
            }
        }
    }
}
