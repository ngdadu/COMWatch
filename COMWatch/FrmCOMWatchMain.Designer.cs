namespace COMWatch
{
    partial class FrmCOMWatchMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCOMWatchMain));
            ntfMain = new NotifyIcon(components);
            imageListSmall = new ImageList(components);
            flowLayoutPanel1 = new FlowLayoutPanel();
            cbView = new ComboBox();
            btnRun = new Button();
            lvPorts = new ListView();
            colPortName = new ColumnHeader();
            colScanTime = new ColumnHeader();
            colScanCount = new ColumnHeader();
            colMessage = new ColumnHeader();
            imageListLarge = new ImageList(components);
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ntfMain
            // 
            ntfMain.Text = "COMWatch";
            ntfMain.Visible = true;
            ntfMain.Click += NtfMain_Click;
            // 
            // imageListSmall
            // 
            imageListSmall.ColorDepth = ColorDepth.Depth32Bit;
            imageListSmall.ImageStream = (ImageListStreamer)resources.GetObject("imageListSmall.ImageStream");
            imageListSmall.TransparentColor = Color.Transparent;
            imageListSmall.Images.SetKeyName(0, "bullet_ball_grey.png");
            imageListSmall.Images.SetKeyName(1, "media_play.png");
            imageListSmall.Images.SetKeyName(2, "media_stop_red.png");
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(cbView);
            flowLayoutPanel1.Controls.Add(btnRun);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.Location = new Point(0, 358);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(800, 92);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // cbView
            // 
            cbView.DrawMode = DrawMode.OwnerDrawFixed;
            cbView.DropDownStyle = ComboBoxStyle.DropDownList;
            cbView.FormattingEnabled = true;
            cbView.ItemHeight = 80;
            cbView.Items.AddRange(new object[] { "Tile", "Details" });
            cbView.Location = new Point(3, 3);
            cbView.Margin = new Padding(3, 3, 60, 3);
            cbView.Name = "cbView";
            cbView.Size = new Size(182, 86);
            cbView.TabIndex = 1;
            cbView.DrawItem += cbView_DrawItem;
            cbView.SelectedIndexChanged += cbView_SelectedIndexChanged;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(248, 3);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(214, 85);
            btnRun.TabIndex = 0;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += BtnRun_Click;
            // 
            // lvPorts
            // 
            lvPorts.AllowColumnReorder = true;
            lvPorts.Columns.AddRange(new ColumnHeader[] { colPortName, colScanTime, colScanCount, colMessage });
            lvPorts.Dock = DockStyle.Fill;
            lvPorts.FullRowSelect = true;
            lvPorts.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvPorts.LargeImageList = imageListLarge;
            lvPorts.Location = new Point(0, 0);
            lvPorts.Name = "lvPorts";
            lvPorts.Size = new Size(800, 358);
            lvPorts.SmallImageList = imageListSmall;
            lvPorts.TabIndex = 2;
            lvPorts.UseCompatibleStateImageBehavior = false;
            lvPorts.View = View.Tile;
            lvPorts.SelectedIndexChanged += lvPorts_SelectedIndexChanged;
            // 
            // colPortName
            // 
            colPortName.Text = "PortName";
            // 
            // colScanTime
            // 
            colScanTime.Text = "Last Scan";
            colScanTime.Width = 120;
            // 
            // colScanCount
            // 
            colScanCount.Text = "Scan Count";
            colScanCount.TextAlign = HorizontalAlignment.Right;
            colScanCount.Width = 80;
            // 
            // colMessage
            // 
            colMessage.Text = "Message";
            colMessage.Width = 260;
            // 
            // imageListLarge
            // 
            imageListLarge.ColorDepth = ColorDepth.Depth32Bit;
            imageListLarge.ImageStream = (ImageListStreamer)resources.GetObject("imageListLarge.ImageStream");
            imageListLarge.TransparentColor = Color.Transparent;
            imageListLarge.Images.SetKeyName(0, "bullet_ball_grey.png");
            imageListLarge.Images.SetKeyName(1, "media_play.png");
            imageListLarge.Images.SetKeyName(2, "media_stop_red.png");
            // 
            // FrmCOMWatchMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lvPorts);
            Controls.Add(flowLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            Name = "FrmCOMWatchMain";
            Text = "COMWatch";
            FormClosing += FrmCOMWatchMain_FormClosing;
            Load += FrmCOMWatchMain_Load;
            Shown += FrmCOMWatchMain_Shown;
            KeyUp += FrmCOMWatchMain_KeyUp;
            Resize += FrmCOMWatchMain_Resize;
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon ntfMain;
        private ImageList imageListSmall;
        private FlowLayoutPanel flowLayoutPanel1;
        private ListView lvPorts;
        private ColumnHeader colPortName;
        private ImageList imageListLarge;
        private Button btnRun;
        private ColumnHeader colScanCount;
        private ColumnHeader colScanTime;
        private ColumnHeader colMessage;
        private ComboBox cbView;
    }
}
