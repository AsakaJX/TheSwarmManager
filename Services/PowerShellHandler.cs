using System.Management.Automation;
using System.Text;

namespace TheSwarmManager.Services {
    public class PowerShellHandler {
        private PowerShell ps = PowerShell.Create();

        public string Command(string command) {
            string errorMessage = string.Empty;

            ps.AddScript(command);
            ps.AddCommand("Out-String");

            PSDataCollection<PSObject> outputCollection = new();
            ps.Streams.Error.DataAdded += (object? sender, DataAddedEventArgs e) => {
                // ! Could or couldn't work -> ?? "nothing"
                errorMessage = (sender as PSDataCollection<ErrorRecord> ?? "nothing")[e.Index].ToString();
            };
            IAsyncResult result = ps.BeginInvoke<PSObject, PSObject>(null, outputCollection);
            ps.EndInvoke(result);
            StringBuilder sb = new();
            foreach (var outputItem in outputCollection) {
                sb.AppendLine(outputItem.BaseObject.ToString());
            }
            ps.Commands.Clear();

            if (!string.IsNullOrEmpty(errorMessage)) {
                return errorMessage;
            }

            return sb.ToString().Trim();
        }
    }
}