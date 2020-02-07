using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;

namespace RDerP.ViewModels
{
    public class RdpTreeViewItem : RderpTreeViewItem
    {
        private bool IsActive { get; set; }
        private int Pid { get; set; }
        public RdpTreeViewItem(string path) : base(path)
        {
            
            var name = Path.GetFileName(path)?.Replace(".rdp", "");
            Name = name;
            Header = CreateHeader(name, Properties.Resources.TerminalIcon);
            MouseDoubleClick += OnRdpDoubleClick;
            Dispatcher?.Invoke(() => CheckForRunningSession(null, path, this));
        }

        private static void OnRdpDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var rdpItem = sender as RdpTreeViewItem;
            if (rdpItem == null) return;

            if (rdpItem.IsActive)
            {
                //switch to existing session
                var process = Process.GetProcessById(rdpItem.Pid);
                SetForegroundWindow(process.MainWindowHandle);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = Constants.Mstsc,
                Arguments = $"\"{rdpItem.FullPath}\""
            };
            var p = Process.Start(startInfo);

            rdpItem.Dispatcher?.Invoke(() => CheckForRunningSession(p, rdpItem.FullPath, rdpItem));
        }

        private static void CheckForRunningSession(Process process, string path, RdpTreeViewItem item)
        {

            //if we're passed a process, wait to make sure it's still running
            if (process != null && !process.HasExited)
            {
                Thread.Sleep(100);
            }

            if (process != null && !process.HasExited)
            {
                ToggleIsActive(item, true,process.Id);
                SetExitListener(process, item);
                return;
            }

            if (process == null || process.HasExited)
            {
                var elapsed = 0;
                var timeout = 500;
                var interval = 50;
                while (elapsed < timeout)
                {
                    process = Process.GetProcessesByName("mstsc")
                        .FirstOrDefault(p => p.StartInfo.Arguments.Contains(path));
                    if (process != null)
                    {
                        ToggleIsActive(item, true, process.Id);
                        SetExitListener(process, item);
                        return;
                    }

                    elapsed += interval;
                    Thread.Sleep(elapsed);
                }
            }
        }

        private static void ToggleIsActive(RdpTreeViewItem item, bool isActive, int pid)
        {
            item.Pid = pid;
            item.IsActive = isActive;
            item.Header = CreateHeader(item.Name,
                isActive ? Properties.Resources.TerminalConnectedIcon: Properties.Resources.TerminalIcon);
        }

        private static void SetExitListener(Process p, RdpTreeViewItem item)
        {
            p.EnableRaisingEvents = true;
            p.Exited += (sender, args) => { item.Dispatcher?.Invoke(() => ToggleIsActive(item, false, p.Id)); };
        }

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
