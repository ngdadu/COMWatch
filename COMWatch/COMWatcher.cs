using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;

namespace COMWatch
{
    public partial class COMWatcher : IDisposable
    {
        public string PortName { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public int BaudRate { get; protected set; } = 9600;
        public Parity Parity { get; protected set; } = Parity.None;
        public int DataBits { get; protected set; } = 8;
        public StopBits StopBits { get; protected set; } = StopBits.One;
        public int EventDelay { get; set; }

        [JsonIgnore]
        public SerialDataReceived OnDataReceived { get; set; } = async (c, s) => { await Task.Delay(1); };
        [JsonIgnore]
        public SerialDataReceived OnErrorReceived { get; set; } = async (c, s) => { await Task.Delay(1); };

        protected SerialPort? _serialPort;
        [JsonIgnore]
        public SerialPort? COMPort {
            get
            {
                if (_serialPort == null && Enabled && SerialPort.GetPortNames().Any(p => p.Equals(PortName, StringComparison.OrdinalIgnoreCase)))
                {
                    _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits)
                    {
                        Encoding = Encoding.ASCII
                    };
                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _serialPort.ErrorReceived += SerialPort_ErrorReceived;
                }
                return _serialPort;
            }
        }

        private async void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            await _serialPortLock.WaitAsync();
            try
            {
                recCount++;
                await OnErrorReceived.Invoke(this, e.EventType.ToString());
            }
            finally
            {
                _serialPortLock.Release();
            }
        }

        private SemaphoreSlim _serialPortLock = new SemaphoreSlim(1, 1);
        private long recCount = 0;
        protected string message = "";

        public long RecCount => recCount;
        public DateTime LastScanTime { get; private set; }

        private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            await _serialPortLock.WaitAsync();
            try
            {
                var port = (SerialPort)sender;
                var bcount = port.BytesToRead;
                if (bcount > 0)
                {
                    await Task.Delay(100); // to sync
                    message = port.ReadExisting();
                    while (!string.IsNullOrEmpty(message) && Char.IsControl(message[0])) message = message.Substring(1);
                    while (!string.IsNullOrEmpty(message) && Char.IsControl(message[message.Length-1])) message=message.Substring(0, message.Length-1);
                    recCount++;
                }
            }
            finally
            {
                _serialPortLock.Release();
            }
        }

        protected BackgroundWorker? worker;
        public bool IsRunning => worker != null && worker.IsBusy;
        public bool Start()
        {
            if (!Enabled || COMPort == null) return false;
            if (worker != null) return true;
            worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            worker.DoWork += SerialWorker_DoWork;
            worker.ProgressChanged += SerialWorker_ProgressChanged;
            worker.RunWorkerAsync(COMPort);
            return true;
        }
        public void Stop()
        {
            if (worker?.IsBusy ?? false) worker.CancelAsync();
            worker?.Dispose();
            worker = null;
        }

        public bool CheckData(ref long LastRec)
        {
            lock (_serialPortLock)
            {
                if (LastRec != recCount)
                {
                    LastRec = recCount;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private async void SerialWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 2 || e.ProgressPercentage == 3)
            {
                await OnErrorReceived.Invoke(this, ((Exception)e.UserState!).Message);
            }
            else
            {
                await OnDataReceived.Invoke(this, $"{e.UserState}");
            }
        }

        private void SerialWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var w = (BackgroundWorker)sender!;
            var port = e.Argument as SerialPort;
            if (w == null || port == null) return;
            long lastCount = 0;
            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                w.ReportProgress(2, ex);
                return;
            }
            DateTime eventTime = DateTime.Now.AddYears(1);
            while (!w.CancellationPending)
            {
                try
                {
                    if (CheckData(ref lastCount))
                    {
                        eventTime = DateTime.Now.AddMilliseconds(EventDelay);
                    }
                    if (eventTime <= DateTime.Now)
                    {
                        LastScanTime = eventTime;
                        w.ReportProgress(1, message);
                        eventTime = DateTime.Now.AddYears(1);
                    }
                }
                catch (Exception ex)
                {
                    w.ReportProgress(2, ex);
                    return;
                }
            }
            try
            {
                port.Close();
            }
            catch (Exception ex)
            {
                w.ReportProgress(3, ex);
                return;
            }
        }

        public void Dispose()
        {
            worker?.Dispose();
            worker = null;
            _serialPort?.Dispose();
            _serialPort = null;
        }
    }
    public delegate Task SerialDataReceived(COMWatcher sender, string message);
    public class COMWatchers
    {
        public List<COMWatcher> Watchers { get; set; } = new List<COMWatcher>();
        public string LogFileNamePattern { get; set; } = "%APP_PATH%\\Data\\%APP_NAME%_%PORT%_%DATE%.txt";

        public string GetLogFileName(COMWatcher watcher)
        {
            var appPath = Application.ExecutablePath;
            var now = DateTime.Now;
            return LogFileNamePattern
                .Replace("%APP_PATH%", Path.GetDirectoryName(appPath))
                .Replace("%APP_NAME%", Path.GetFileNameWithoutExtension(appPath))
                .Replace("%DATE%", now.ToString("yyyy-MM-dd"))
                .Replace("%YEAR%", now.ToString("yyyy"))
                .Replace("%MONTH%", now.ToString("MM"))
                .Replace("%DAY%", now.ToString("dd"))
                .Replace("%WEEKDAY%", (((int)now.DayOfWeek + 6) % 7).ToString())
                .Replace("%HOUR%", now.ToString("HH"))
                .Replace("%MINUTE%", now.ToString("mm"))
                .Replace("%PORT%", watcher.PortName)
                ;
        }
    }
    public class COMLogger
    {
        public string LogFileNamePattern { get; set; } = "";
        public string GetLogFileName(DateTime now)
        {
            return LogFileNamePattern
                .Replace("%DATE%", now.ToString("yyyy-MM-dd"))
                .Replace("%YEAR%", now.ToString("yyyy"))
                .Replace("%MONTH%", now.ToString("MM"))
                .Replace("%DAY%", now.ToString("dd"))
                .Replace("%WEEKDAY%", (((int)now.DayOfWeek + 6) % 7).ToString())
                .Replace("%HOUR%", now.ToString("HH"))
                .Replace("%MINUTE%", now.ToString("mm"))
                ;
        }
        public async Task WriteLogAsync(string message, bool isError = false)
        {
            var logTime = DateTime.Now;
            var logName = GetLogFileName(logTime);
            try
            {
                message = isError ? $"[{logTime:HH:mm:ss.fff}] *ERROR* {message}" : $"[{logTime:HH:mm:ss.fff}] {message}";
                if (!Directory.Exists(Path.GetDirectoryName(logName)))
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(logName!)!);
                if (File.Exists(logName))
                {
                    await File.AppendAllTextAsync(logName, Environment.NewLine + message);
                }
                else
                {
                    await File.WriteAllTextAsync(logName, message);
                }
            }
            catch { }
        }
    }
}
