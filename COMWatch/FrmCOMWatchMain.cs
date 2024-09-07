using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Windows.Forms;

namespace COMWatch
{
    public partial class FrmCOMWatchMain : Form
    {
        private COMWatchers watchers = new COMWatchers();

        public FrmCOMWatchMain()
        {
            InitializeComponent();
            ntfMain.Icon = this.Icon;
        }

        private void FrmCOMWatchMain_Load(object sender, EventArgs e)
        {
            try
            {
                var watchersFile = Path.ChangeExtension(Application.ExecutablePath, "configs.json");
                var watchersContent = File.Exists(watchersFile) ? File.ReadAllText(watchersFile) : "";
                if (!string.IsNullOrEmpty(watchersContent)) watchers = JsonSerializer.Deserialize<COMWatchers>(watchersContent) ?? new COMWatchers();
                Text = $"COMWatch ({watchers.Watchers.Count}): {watchersFile}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JsonConfigs-Error: {ex.Message}");
                Application.Exit();
            }
            foreach (var watcher in watchers.Watchers)
            {
                watcher.OnDataReceived = COMDataReceived;
                watcher.OnErrorReceived = COMErrorReceived;
                var item = lvPorts.Items.Add(watcher.PortName);
                item.Tag = watcher;
                item.ImageIndex = !watcher.Enabled ? 0 : watcher.COMPort is null ? 2 : 1;
                item.SubItems.AddRange(new[] { "", "", "" });
                logFiles[watcher.PortName] = new COMLogger { LogFileNamePattern = watchers.GetLogFileName(watcher) };
                if (watcher.Enabled) watcher.Start();
            }
            cbView.SelectedIndex = 0;
        }

        private readonly Dictionary<string, COMLogger> logFiles = new Dictionary<string, COMLogger>();

        private async Task COMDataReceived(COMWatcher sender, string message)
        {
            if (string.IsNullOrEmpty(message) || !logFiles.TryGetValue(sender.PortName, out var logger)) return;
            await logger.WriteLogAsync(message);
            var item = lvPorts.Items.OfType<ListViewItem>().FirstOrDefault(v => v.Text == sender.PortName);
            if (item == null) return;
            item.SubItems[1].Text = sender.LastScanTime.ToString("dd.MM.yyyy HH:mm:ss");
            item.SubItems[2].Text = sender.RecCount.ToString("#,##0");
            item.SubItems[3].Text = message;
            item.ForeColor = SystemColors.ControlText;
        }

        private async Task COMErrorReceived(COMWatcher sender, string message)
        {
            if (!logFiles.TryGetValue(sender.PortName, out var logger)) return;
            await logger.WriteLogAsync(message, true);
            var item = lvPorts.Items.OfType<ListViewItem>().FirstOrDefault(v => v.Text == sender.PortName);
            if (item == null) return;
            item.SubItems[1].Text = sender.LastScanTime.ToString("dd.MM.yyyy HH:mm:ss");
            item.SubItems[2].Text = sender.RecCount.ToString("#,##0");
            item.SubItems[3].Text = message;
            item.ForeColor = Color.Red;
        }

        private void FrmCOMWatchMain_Shown(object sender, EventArgs e)
        {
            lvPorts_SelectedIndexChanged(sender, e);
            BeginInvoke(new Action(() => WindowState = FormWindowState.Minimized));
        }

        private void FrmCOMWatchMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ntfMain.Visible = true;
                Hide();
            }
            else if (!Visible)
            {
                ntfMain.Visible = false;
                lvPorts_SelectedIndexChanged(sender, e);
                Show();
            }
        }

        private void NtfMain_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            FrmCOMWatchMain_Resize(this, EventArgs.Empty);
        }

        bool isRunning;
        public bool Running
        {
            get
            {
                return lvPorts.SelectedItems.OfType<ListViewItem>().Any(v => ((v.Tag as COMWatcher)?.IsRunning ?? false));
            }
            set
            {
                isRunning = value;
                //foreach (var watch in watchers.Watchers) if (isRunning && watch.IsActive) watch.Start(); else watch.Stop();
                foreach (ListViewItem item in lvPorts.SelectedItems)
                {
                    var watch = (COMWatcher)item.Tag!;
                    if (watch.Enabled && value) watch.Start(); else watch.Stop();
                    item.ImageIndex = !watch.Enabled ? 0 : watch.IsRunning ? 1 : 2;
                }
                ChangeBtnRunText(value);
            }
        }

        private void ChangeBtnRunText(bool value)
        {
            btnRun.Text = $"{lvPorts.SelectedItems.Count}x {(value ? "Stop" : "Start")} [F8]";
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            Running = !Running;
        }

        private void cbView_SelectedIndexChanged(object sender, EventArgs e)
        {
            lvPorts.View = cbView.SelectedIndex == 1 ? View.Details : View.Tile;
            if (lvPorts.View == View.Details) lvPorts.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void cbView_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            // a dropdownlist may initially have no item selected, so skip the highlighting:
            if (e.Index >= 0)
            {
                Graphics g = e.Graphics;
                Brush brush = new SolidBrush((e.State & DrawItemState.Selected) == DrawItemState.Selected ? SystemColors.MenuHighlight : e.BackColor);
                Brush tBrush = new SolidBrush(e.ForeColor);

                g.FillRectangle(brush, e.Bounds);
                var txt = $"{cbView.Items[e.Index]}";
                var tsize = e.Graphics.MeasureString(txt, e.Font!);
                e.Graphics.DrawString(txt, e.Font!, tBrush, 8, e.Bounds.Top + ((float)e.Bounds.Height - tsize.Height) / 2, StringFormat.GenericDefault);
                brush.Dispose();
                tBrush.Dispose();
            }
            e.DrawFocusRectangle();
        }

        private void lvPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPorts.SelectedItems.Count == 0)
            {
                btnRun.Enabled = false;
                btnRun.Text = "No selection";
            }
            else
            {
                btnRun.Enabled = true;
                ChangeBtnRunText(Running);
            }
        }

        private void FrmCOMWatchMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && MessageBox.Show("Exit application?", "COMWatch", MessageBoxButtons.OKCancel) != DialogResult.OK) e.Cancel = true;
        }

        private void FrmCOMWatchMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in lvPorts.Items) item.Selected = true;
                ChangeBtnRunText(Running);
                ActiveControl = lvPorts;
            }
            if (e.KeyCode == Keys.F8 && btnRun.Enabled) btnRun.PerformClick();
        }
    }
}
