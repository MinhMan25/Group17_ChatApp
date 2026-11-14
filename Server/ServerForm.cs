using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace Chat_room_test_TCP
{
    public partial class ServerForm : Form
    {
        private Process webServiceProcess;
        private bool isClosing;

        public ServerForm()
        {
            InitializeComponent();
            Text = "Zalu WebService Dashboard";
            Status_Label.Text = "Trạng thái: Đã tắt";
            ServiceUrl_Label.Text = "Địa chỉ: http://localhost:5000";
            FormClosing += ServerForm_FormClosing;
            UpdateControls(false);
            WriteLog("Dashboard sẵn sàng. Nhấn Bật WebService để khởi chạy API.");
        }

        private void Start_Server_Button_Click(object sender, EventArgs e)
        {
            if (webServiceProcess != null && !webServiceProcess.HasExited)
                return;

            string projectDirectory = FindWebServiceProjectDirectory();
            if (projectDirectory == null)
            {
                WriteLog("LỖI: Không tìm thấy WebService/WebService.csproj. Hãy chạy dashboard từ thư mục repo hoặc bin của project Server.");
                return;
            }

            string assemblyFile = FindWebServiceAssembly(projectDirectory);
            if (assemblyFile == null)
            {
                WriteLog("LỖI: Chưa tìm thấy WebService.dll trong WebService/bin. Hãy build WebService một lần trong Visual Studio hoặc chạy dotnet build WebService/WebService.csproj.");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "\"" + assemblyFile + "\"",
                WorkingDirectory = projectDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                 // Đọc log tiếng Việt bằng UTF-8
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                process.OutputDataReceived += (_, args) =>
                {
                    if (args.Data != null) WriteLog(args.Data);
                };
                process.ErrorDataReceived += (_, args) =>
                {
                    if (args.Data != null) WriteLog("[stderr] " + args.Data);
                };
                process.Exited += (processSender, eventArgs) => OnWebServiceExited(process);

                if (!process.Start())
                    throw new InvalidOperationException("Không khởi chạy được tiến trình dotnet.");

                webServiceProcess = process;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                UpdateControls(true);
                WriteLog("Đang khởi chạy " + assemblyFile);
            }
            catch (Exception ex)
            {
                WriteLog("LỖI khởi chạy WebService: " + ex.Message);
                UpdateControls(false);
            }
        }

        private static string FindWebServiceProjectDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "WebService", "WebService.csproj");
                if (File.Exists(candidate)) return Path.GetDirectoryName(candidate);
                directory = directory.Parent;
            }
            return null;
        }

        private static string FindWebServiceAssembly(string projectDirectory)
        {
            string binDirectory = Path.Combine(projectDirectory, "bin");
            if (!Directory.Exists(binDirectory)) return null;

            string newestAssembly = null;
            DateTime newestWrite = DateTime.MinValue;
            foreach (string candidate in Directory.GetFiles(binDirectory, "WebService.dll", SearchOption.AllDirectories))
            {
                DateTime lastWrite = File.GetLastWriteTimeUtc(candidate);
                if (lastWrite > newestWrite)
                {
                    newestWrite = lastWrite;
                    newestAssembly = candidate;
                }
            }
            return newestAssembly;
        }

        private void Close_Server_Button_Click(object sender, EventArgs e)
        {
            StopWebService();
        }

        private void StopWebService()
        {
            Process process = webServiceProcess;
            if (process == null || process.HasExited)
            {
                webServiceProcess = null;
                UpdateControls(false);
                return;
            }

            try
            {
                WriteLog("Đang tắt WebService...");
                process.Kill();
                process.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                WriteLog("LỖI khi tắt WebService: " + ex.Message);
            }
            finally
            {
                webServiceProcess = null;
                UpdateControls(false);
            }
        }

        private void OnWebServiceExited(Process process)
        {
            if (IsDisposed || Disposing) return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (ReferenceEquals(webServiceProcess, process))
                    {
                        webServiceProcess = null;
                        UpdateControls(false);
                        WriteLog("WebService đã dừng (exit code " + process.ExitCode + ").");
                    }
                }));
            }
            catch (InvalidOperationException) { }
        }

        private void UpdateControls(bool running)
        {
            Start_Server_Button.Enabled = !running;
            Close_Server_Button.Enabled = running;
            Status_Label.Text = running ? "Trạng thái: Đang chạy" : "Trạng thái: Đã tắt";
            Status_Label.ForeColor = running ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
        }

        private void WriteLog(string message)
        {
            if (IsDisposed || Disposing) return;
            if (Server_ChatView.InvokeRequired)
            {
                try { Server_ChatView.BeginInvoke(new Action<string>(WriteLog), message); }
                catch (InvalidOperationException) { }
                return;
            }

            Server_ChatView.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
            Server_ChatView.SelectionStart = Server_ChatView.TextLength;
            Server_ChatView.ScrollToCaret();
        }

        private void Exit_button_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isClosing) return;
            isClosing = true;
            StopWebService();
        }

        private void Server_ChatView_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
