namespace DabinPACT
{
    partial class frmMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend7 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnDo = new System.Windows.Forms.Button();
            this.btnError = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tbxSAIP = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbxSGIP = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbxPMIP = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbxSN = new System.Windows.Forms.TextBox();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblState = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.spcBody = new System.Windows.Forms.SplitContainer();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.trvMenu = new System.Windows.Forms.TreeView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.cbxExpandAll = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.spcRight = new System.Windows.Forms.SplitContainer();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabResult = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tbxResult = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbxWordwrap = new System.Windows.Forms.CheckBox();
            this.tabConfigure = new System.Windows.Forms.TabPage();
            this.panel6 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.dataGridViewParams = new System.Windows.Forms.DataGridView();
            this.pnlRBottom = new System.Windows.Forms.Panel();
            this.chtSignal = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pnlTop.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcBody)).BeginInit();
            this.spcBody.Panel1.SuspendLayout();
            this.spcBody.Panel2.SuspendLayout();
            this.spcBody.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcRight)).BeginInit();
            this.spcRight.Panel1.SuspendLayout();
            this.spcRight.Panel2.SuspendLayout();
            this.spcRight.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabResult.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabConfigure.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParams)).BeginInit();
            this.pnlRBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chtSignal)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.SystemColors.HotTrack;
            this.pnlTop.Controls.Add(this.panel3);
            this.pnlTop.Controls.Add(this.btnError);
            this.pnlTop.Controls.Add(this.label7);
            this.pnlTop.Controls.Add(this.tbxSAIP);
            this.pnlTop.Controls.Add(this.label6);
            this.pnlTop.Controls.Add(this.tbxSGIP);
            this.pnlTop.Controls.Add(this.label5);
            this.pnlTop.Controls.Add(this.tbxPMIP);
            this.pnlTop.Controls.Add(this.label4);
            this.pnlTop.Controls.Add(this.tbxSN);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1587, 51);
            this.pnlTop.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel3.Controls.Add(this.btnStop);
            this.panel3.Controls.Add(this.btnDo);
            this.panel3.Location = new System.Drawing.Point(1018, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(239, 48);
            this.panel3.TabIndex = 10;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(131, 0);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(89, 48);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnDo
            // 
            this.btnDo.Location = new System.Drawing.Point(25, 0);
            this.btnDo.Name = "btnDo";
            this.btnDo.Size = new System.Drawing.Size(86, 48);
            this.btnDo.TabIndex = 10;
            this.btnDo.Text = "Do";
            this.btnDo.UseVisualStyleBackColor = true;
            this.btnDo.Click += new System.EventHandler(this.btnDo_Click);
            // 
            // btnError
            // 
            this.btnError.BackColor = System.Drawing.Color.White;
            this.btnError.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnError.Location = new System.Drawing.Point(1292, 5);
            this.btnError.Name = "btnError";
            this.btnError.Size = new System.Drawing.Size(75, 38);
            this.btnError.TabIndex = 9;
            this.btnError.Text = "Result";
            this.btnError.UseVisualStyleBackColor = false;
            this.btnError.Click += new System.EventHandler(this.btnError_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(252, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 17);
            this.label7.TabIndex = 7;
            this.label7.Text = "SA IP";
            // 
            // tbxSAIP
            // 
            this.tbxSAIP.Location = new System.Drawing.Point(300, 11);
            this.tbxSAIP.Name = "tbxSAIP";
            this.tbxSAIP.ReadOnly = true;
            this.tbxSAIP.Size = new System.Drawing.Size(100, 26);
            this.tbxSAIP.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(450, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 17);
            this.label6.TabIndex = 5;
            this.label6.Text = "SG IP";
            // 
            // tbxSGIP
            // 
            this.tbxSGIP.Location = new System.Drawing.Point(502, 12);
            this.tbxSGIP.Name = "tbxSGIP";
            this.tbxSGIP.ReadOnly = true;
            this.tbxSGIP.Size = new System.Drawing.Size(100, 26);
            this.tbxSGIP.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(675, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 17);
            this.label5.TabIndex = 3;
            this.label5.Text = "PM IP";
            // 
            // tbxPMIP
            // 
            this.tbxPMIP.Location = new System.Drawing.Point(725, 11);
            this.tbxPMIP.Name = "tbxPMIP";
            this.tbxPMIP.ReadOnly = true;
            this.tbxPMIP.Size = new System.Drawing.Size(100, 26);
            this.tbxPMIP.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(78, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 17);
            this.label4.TabIndex = 1;
            this.label4.Text = "S/N";
            // 
            // tbxSN
            // 
            this.tbxSN.Location = new System.Drawing.Point(115, 12);
            this.tbxSN.Name = "tbxSN";
            this.tbxSN.Size = new System.Drawing.Size(100, 26);
            this.tbxSN.TabIndex = 0;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pnlBottom.Controls.Add(this.lblState);
            this.pnlBottom.Controls.Add(this.label8);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 596);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1587, 49);
            this.pnlBottom.TabIndex = 1;
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(101, 15);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(171, 17);
            this.lblState.TabIndex = 1;
            this.lblState.Text = "순시적 상태를 나타냄";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(58, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = "상태:";
            // 
            // spcBody
            // 
            this.spcBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcBody.Location = new System.Drawing.Point(0, 51);
            this.spcBody.Name = "spcBody";
            // 
            // spcBody.Panel1
            // 
            this.spcBody.Panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.spcBody.Panel1.Controls.Add(this.pnlLeft);
            // 
            // spcBody.Panel2
            // 
            this.spcBody.Panel2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.spcBody.Panel2.Controls.Add(this.pnlRight);
            this.spcBody.Size = new System.Drawing.Size(1587, 545);
            this.spcBody.SplitterDistance = 345;
            this.spcBody.SplitterWidth = 8;
            this.spcBody.TabIndex = 2;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.panel5);
            this.pnlLeft.Controls.Add(this.panel4);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(345, 545);
            this.pnlLeft.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.trvMenu);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 29);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(345, 516);
            this.panel5.TabIndex = 2;
            // 
            // trvMenu
            // 
            this.trvMenu.CheckBoxes = true;
            this.trvMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvMenu.HideSelection = false;
            this.trvMenu.HotTracking = true;
            this.trvMenu.Location = new System.Drawing.Point(0, 0);
            this.trvMenu.Name = "trvMenu";
            this.trvMenu.Size = new System.Drawing.Size(345, 516);
            this.trvMenu.TabIndex = 1;
            this.trvMenu.Click += new System.EventHandler(this.OnClickItem);
            this.trvMenu.DoubleClick += new System.EventHandler(this.OnDoubleClickItem);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.cbxExpandAll);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(345, 29);
            this.panel4.TabIndex = 1;
            // 
            // cbxExpandAll
            // 
            this.cbxExpandAll.AutoSize = true;
            this.cbxExpandAll.Location = new System.Drawing.Point(221, 7);
            this.cbxExpandAll.Name = "cbxExpandAll";
            this.cbxExpandAll.Size = new System.Drawing.Size(106, 21);
            this.cbxExpandAll.TabIndex = 1;
            this.cbxExpandAll.Text = "Expand All";
            this.cbxExpandAll.UseVisualStyleBackColor = true;
            this.cbxExpandAll.CheckedChanged += new System.EventHandler(this.cbxExpandAll_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 17);
            this.label9.TabIndex = 0;
            this.label9.Text = "Test List";
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.SystemColors.Desktop;
            this.pnlRight.Controls.Add(this.spcRight);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(1234, 545);
            this.pnlRight.TabIndex = 0;
            // 
            // spcRight
            // 
            this.spcRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcRight.Location = new System.Drawing.Point(0, 0);
            this.spcRight.Name = "spcRight";
            this.spcRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcRight.Panel1
            // 
            this.spcRight.Panel1.Controls.Add(this.tabMain);
            // 
            // spcRight.Panel2
            // 
            this.spcRight.Panel2.Controls.Add(this.pnlRBottom);
            this.spcRight.Size = new System.Drawing.Size(1234, 545);
            this.spcRight.SplitterDistance = 324;
            this.spcRight.SplitterWidth = 10;
            this.spcRight.TabIndex = 0;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabResult);
            this.tabMain.Controls.Add(this.tabConfigure);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1234, 324);
            this.tabMain.TabIndex = 0;
            // 
            // tabResult
            // 
            this.tabResult.Controls.Add(this.panel2);
            this.tabResult.Controls.Add(this.panel1);
            this.tabResult.Location = new System.Drawing.Point(4, 26);
            this.tabResult.Name = "tabResult";
            this.tabResult.Padding = new System.Windows.Forms.Padding(3);
            this.tabResult.Size = new System.Drawing.Size(1226, 294);
            this.tabResult.TabIndex = 1;
            this.tabResult.Text = "Progress";
            this.tabResult.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tbxResult);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 59);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1220, 232);
            this.panel2.TabIndex = 3;
            // 
            // tbxResult
            // 
            this.tbxResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbxResult.Location = new System.Drawing.Point(0, 0);
            this.tbxResult.Name = "tbxResult";
            this.tbxResult.ReadOnly = true;
            this.tbxResult.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.tbxResult.Size = new System.Drawing.Size(1220, 232);
            this.tbxResult.TabIndex = 1;
            this.tbxResult.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbxWordwrap);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1220, 56);
            this.panel1.TabIndex = 2;
            // 
            // cbxWordwrap
            // 
            this.cbxWordwrap.AutoSize = true;
            this.cbxWordwrap.Location = new System.Drawing.Point(1089, 18);
            this.cbxWordwrap.Name = "cbxWordwrap";
            this.cbxWordwrap.Size = new System.Drawing.Size(101, 21);
            this.cbxWordwrap.TabIndex = 2;
            this.cbxWordwrap.Text = "Wordwarp";
            this.cbxWordwrap.UseVisualStyleBackColor = true;
            this.cbxWordwrap.CheckedChanged += new System.EventHandler(this.cbxWordwrap_CheckedChanged);
            // 
            // tabConfigure
            // 
            this.tabConfigure.Controls.Add(this.panel6);
            this.tabConfigure.Location = new System.Drawing.Point(4, 26);
            this.tabConfigure.Name = "tabConfigure";
            this.tabConfigure.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfigure.Size = new System.Drawing.Size(1226, 294);
            this.tabConfigure.TabIndex = 0;
            this.tabConfigure.Text = "Configure";
            this.tabConfigure.UseVisualStyleBackColor = true;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.comboBox1);
            this.panel6.Controls.Add(this.dataGridViewParams);
            this.panel6.Location = new System.Drawing.Point(33, 26);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(994, 275);
            this.panel6.TabIndex = 4;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(666, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.comboBox1.Size = new System.Drawing.Size(155, 24);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.OnSelectComboBox);
            // 
            // dataGridViewParams
            // 
            this.dataGridViewParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewParams.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewParams.Name = "dataGridViewParams";
            this.dataGridViewParams.RowHeadersWidth = 51;
            this.dataGridViewParams.RowTemplate.Height = 27;
            this.dataGridViewParams.Size = new System.Drawing.Size(640, 600);
            this.dataGridViewParams.TabIndex = 3;
            // 
            // pnlRBottom
            // 
            this.pnlRBottom.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlRBottom.Controls.Add(this.chtSignal);
            this.pnlRBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRBottom.Location = new System.Drawing.Point(0, 0);
            this.pnlRBottom.Name = "pnlRBottom";
            this.pnlRBottom.Size = new System.Drawing.Size(1234, 211);
            this.pnlRBottom.TabIndex = 0;
            // 
            // chtSignal
            // 
            this.chtSignal.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.LeftRight;
            this.chtSignal.BorderlineColor = System.Drawing.Color.Maroon;
            this.chtSignal.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea7.Name = "ChartArea1";
            this.chtSignal.ChartAreas.Add(chartArea7);
            this.chtSignal.Dock = System.Windows.Forms.DockStyle.Fill;
            legend7.Name = "Legend1";
            this.chtSignal.Legends.Add(legend7);
            this.chtSignal.Location = new System.Drawing.Point(0, 0);
            this.chtSignal.Name = "chtSignal";
            series7.ChartArea = "ChartArea1";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series7.Legend = "Legend1";
            series7.Name = "Series1";
            this.chtSignal.Series.Add(series7);
            this.chtSignal.Size = new System.Drawing.Size(1234, 211);
            this.chtSignal.TabIndex = 0;
            this.chtSignal.Text = "chart1";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.ClientSize = new System.Drawing.Size(1587, 645);
            this.Controls.Add(this.spcBody);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "frmMain";
            this.Text = "PACT ";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.spcBody.Panel1.ResumeLayout(false);
            this.spcBody.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBody)).EndInit();
            this.spcBody.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.spcRight.Panel1.ResumeLayout(false);
            this.spcRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcRight)).EndInit();
            this.spcRight.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabResult.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabConfigure.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParams)).EndInit();
            this.pnlRBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chtSignal)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.SplitContainer spcBody;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.SplitContainer spcRight;
        private System.Windows.Forms.Panel pnlRBottom;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtSignal;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabConfigure;
        private System.Windows.Forms.TabPage tabResult;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbxSN;
        private System.Windows.Forms.Button btnError;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbxSAIP;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbxSGIP;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbxPMIP;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbxWordwrap;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnDo;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.TreeView trvMenu;
        private System.Windows.Forms.CheckBox cbxExpandAll;
        private System.Windows.Forms.DataGridView dataGridViewParams;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.RichTextBox tbxResult;
    }
}

