
#define _CONNECT_SG_
#define _CONNECT_PM_
//#define _CONNECT_SA_
#define _CONNECT_PACT_

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;
using System.Threading;
using LoadingIndicator.WinForms;

namespace DabinPACT
{
    public delegate void HandleMessage(string error);
    public delegate void HandleMessageSCPI(byte[] response);

    public partial class frmMain : Form
    {
        // SA IP: 192.168.15.4 port 5025
        // SG IP: 192.168.0.51 port 5025
        // PW IP: DHCP port 1234

        public CommonFunctions CF;
        public FileUtil _FileUtil;

        public ConnectionDelegate connectHandler;

        private DataTable _table;
        private bool _clicked;

        private int _waitMessageTime = 50; // 50 ms PACT외 메시지 전송후, wait time
        private int _waitPACTTime = 50; // 50 ms PACT 메시지 전송 후, wait time

        public LNA_SET _LNA_Set;

        public string _SIGNAL_TITLE = "signal";

        CancellationTokenSource cts = null;

        private LongOperation _longOperation;

        protected async override void OnLoad(EventArgs e)
        {
            tbxResult.Clear();
            initServerInfo();
            initGridView();

#if (_CONNECT_PM_)
            _longOperation.Start();
            await initTcpScpiPM();
            _longOperation.Stop();
#endif

#if (_CONNECT_SG_)
            _longOperation.Start();
            await initTcpScpiSG();
            _longOperation.Stop();
#endif

#if (_CONNECT_SA_)
            _longOperation.Start();
            await initTcpScpiSA();
            _longOperation.Stop();
#endif
#if (_CONNECT_PACT_)
            _longOperation.Start();
            await initMqttClient();
            _longOperation.Stop();
#endif

            base.OnLoad(e);
        }

        private void initServerInfo()
        {
            ServerInfo mqttInfo = CF.GetServerInfo("nodConMQTT");
            _mqttServerIp = mqttInfo.Ip;
            _mqttPort = mqttInfo.Port;
            _mqttFtpIp = "ftp://" + mqttInfo.Ip;

            ServerInfo pmInfo = CF.GetServerInfo("nodConPM");
            _hostNamePM = pmInfo.Ip;
            _portPM = pmInfo.Port;
            tbxPMIP.Text = _hostNamePM;

            ServerInfo saInfo = CF.GetServerInfo("nodConSA");
            _hostNameSA = saInfo.Ip;
            _portSA = saInfo.Port;
            tbxSAIP.Text = _hostNameSA;

            ServerInfo sgInfo = CF.GetServerInfo("nodConSG");
            _hostNameSG = sgInfo.Ip;
            _portSG = sgInfo.Port;
            tbxSGIP.Text = _hostNameSG;
        }

        public frmMain()
        {
            InitializeComponent();
            _longOperation = new LongOperation(this, LongOperationSettings.Default);
            CF = new CommonFunctions(lblState, tbxResult, trvMenu);
            _FileUtil = new FileUtil();
            CF.LoadTree();
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private void OnConnectDevice(string hostName)
        {
            string message = "===== CONNECTTED " + hostName + "  =====\r\n";
            CallLogMessage(message);
        }

        private void OnDisConnectDevice(string hostName)
        {
            string message = "===== DISCONNECTTED " + hostName + "  =====\r\n";
            CallLogMessage(message);
        }

        private void CallLogMessage(string msg)
        {
            this.BeginInvoke((MethodInvoker)delegate { CF.Log(msg); });
        }

        private void CallLogMessageWithDt(string msg)
        {
            this.BeginInvoke((MethodInvoker)delegate { CF.LogWithDt(msg); });
        }

        private void CallLogMessageWithResultDt(string msg, int result, bool res)
        {
            this.BeginInvoke((MethodInvoker)delegate { CF.LogWithResultDt(msg, result, res); });
        }

        private void initGridView()
        {
            dataGridViewParams.ClearSelection();
            dataGridViewParams.Size = new System.Drawing.Size(640, 600);

            dataGridViewParams.DataSource = null;
            dataGridViewParams.Rows.Clear();
            dataGridViewParams.Refresh();

            _table = new DataTable();

            _table.Columns.Add("ParamName", typeof(string));
            _table.Columns.Add("ParamValue", typeof(string));

            //dataGridViewParams.EditMode = DataGridViewEditMode.EditOnEnter;

            dataGridViewParams.DataSource = _table;
            
            dataGridViewParams.Columns[0].Width = 200;
            dataGridViewParams.Columns[1].Width = 400;

            comboBox1.Visible = false;

            comboBox1.Items.Add("LNA Off Only");
            comboBox1.Items.Add("LNA On Only");
            comboBox1.Items.Add("LNA Off + On");
        }

        private void cbxExpandAll_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxExpandAll.Checked)
            {
                trvMenu.ExpandAll();
                cbxExpandAll.Text = "Collapse All";
            }
            else
            {
                trvMenu.CollapseAll();
                cbxExpandAll.Text = "Expand All";
            }
        }

        private async void OnClickItem(object sender, EventArgs e)
        {
            if (_clicked) return;
            _clicked = true;
            
            await Task.Delay(SystemInformation.DoubleClickTime);

            if (!_clicked) return;
            _clicked = false;

            tabMain.SelectedTab = this.tabResult;
        }

        private void OnDoubleClickItem(object sender, EventArgs e)
        {
            _clicked = false;
            comboBox1.Visible = false;

            tabMain.SelectedTab = tabConfigure;

            string node = trvMenu.SelectedNode.Name;

            CF.DataAddToGridView(node, _table);

            switch (node)
            {
                case "nodCalibrationPath":
                    comboBox1.Visible = true;
                    string pathValue = _table.Rows[0].Field<string>("ParamValue");
                    comboBox1.SelectedItem = pathValue;
                    break;
                default:
                    comboBox1.Visible = false;
                    break;
            }
            dataGridViewParams.Refresh();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (btnDo.BackColor == Color.LightGreen)
            {
                btnDo.BackColor = Color.LightGray;
            }
            else
            {
                btnDo.BackColor = Color.LightGreen;
            }
        }

        private void ChangeBtnState(bool isWorking)
        {
            if (isWorking)
            {
                btnDo.BackColor = Color.LightGreen;
                btnDo.Enabled = false;
                btnStop.Enabled = true;
            }
            else
            {
                btnDo.Enabled = true;
                btnStop.Enabled = false;
                btnDo.BackColor = Color.LightGray;
            }
        }

        private async void btnDo_Click(object sender, EventArgs e)
        {
            List<TreeNode> selectedNodes = CF.GetCheckedNode(trvMenu);
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += OnTimerTick;
            timer.Start();
            ChangeBtnState(true);
            try
            {
                if (selectedNodes.Count != 0)
                {
                    _LNA_Set = CF.GetLNAEnv();
                    if (_LNA_Set == LNA_SET.LNA_ON_OFF)
                    {
                        _LNA_Set = LNA_SET.LNA_OFF;
                        await DoSelectedScenario(selectedNodes);
                        _LNA_Set = LNA_SET.LNA_ON;
                        await DoSelectedScenario(selectedNodes);
                        _LNA_Set = LNA_SET.LNA_ON_OFF;
                    }
                    else
                    {
                        await DoSelectedScenario(selectedNodes);
                    }
                }
                else
                {
                    MessageBox.Show("수행할 항목을 선택해 주세요!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            timer.Stop();
            timer.Dispose();
            ChangeBtnState(true);
        }        

        private async Task DoSelectedScenario(List<TreeNode> selectedNodes)
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            foreach (TreeNode node in selectedNodes)
            {
                List<ParamSetJson> paramSets = CF.GetParams(node.Name);
                Hashtable hashtable = new Hashtable();

                if (paramSets != null)
                {
                    foreach (ParamSetJson paramSet in paramSets)
                    {
                        hashtable.Add(paramSet.Name, paramSet);
                    }
                    switch (node.Name)
                    {
                        case "nodCableLoss":
                            //await ExecCableLossScenario(hashtable, token);
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await ExecCableLossScenario(hashtable, token);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }, token);
                            break;
                        case "nodCalibrationPath":
                            // LNA On + Off 관련해서는 Do 선택시 무조건 읽어서 처리를 해서, Global 값으로 저장해서 사용해야 함.
                            break;
                        case "nodGlobalOffset":
                            await ExecGlobalOffsetScenario(hashtable);
                            break;
                        case "nodIQimbalance":
                            //await ExecIQimbalanceScenario(hashtable, token);
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await ExecIQimbalanceScenario(hashtable, token);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }, token);
                            break;
                        case "nodPowerAtten":
                            //await ExecPowerAttenScenario(hashtable);
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await ExecPowerAttenScenario(hashtable, token);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }, token);
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                cts.Cancel();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
            }
            finally
            {
                cts.Dispose();
            }
        }

        private void btnError_Click(object sender, EventArgs e)
        {
        }

        private void cbxWordwrap_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void OnSelectComboBox(object sender, EventArgs e)
        {            
            string node = "nodCalibrationPath";
            CF.setCalibrationValue(node, (String)comboBox1.SelectedItem);
            List<ParamSetJson> paramSetList = CF.GetParamDataSet(node);
            _table.Clear();
            _table.Rows.Add(paramSetList[0].Display, paramSetList[0].Value);
        }

        private void OnDrawSignal()
        {
            chtSignal.Titles.Clear();
            chtSignal.Series.Clear();
            chtSignal.Titles.Add("Spectrum Analyze");
            Series sin_series = chtSignal.Series.Add(_SIGNAL_TITLE);
            sin_series.ChartType = SeriesChartType.Spline;

            for (int i = 0; i < _spectrumData.Length; i++)
            {
                sin_series.Points.Add(_spectrumData[i] / 100);
            }
        }
    }
}
