/*
 * Form에 있는 Component들을 쓰기 위해 파일만 분리
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DabinPACT {
	public partial class frmMain : Form {

        public string _hostNamePM = ""; // 	Prologix-00-21-69-01-29	 192.168.0.3
        public string _portPM = "";

        public TelnetConnection _tcPM;

        private Queue _sendMsgPMQueue;
        private Queue _receiveMsgPMQueue;
        //private string _oneCycleMessagePM = "";

        private float _measureValueOfPM = 0;
        public HandleMessageSCPI errorHandler;

        private async Task initTcpScpiPM()
        {
            void initPMClient() 
            {
                _sendMsgPMQueue = new Queue();
                _receiveMsgPMQueue = new Queue();

                _tcPM = new TelnetConnection("Power Meter");
                _tcPM.Opened += OnConnectDevice;
                _tcPM.Closed += OnDisConnectDevice;

                _tcPM.ReadTimeout = CF._READ_TIMEOUT; //  sec

                _tcPM.Open(_hostNamePM, _portPM);
                _tcPM.messageHandler += OnSCPIMessagePM;

            }
            await Task.Run(() =>
            {
                try
                {
                    initPMClient();
                }
                catch (Exception ex)
                {
                    CallLogMessage("[PM] " + ex.ToString());
                }
            });
        }

        public void WritePM(string s)
        {
            _tcPM.WriteLine(s);
            if (s.IndexOf('?') >= 0)
            {
                try
                {
                    _tcPM.Read();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //_tcPM.Dispose();
                }
            }
        }

        public async Task<float> AsyncSendMessagePM()
        {
            await Task.Run(async () =>
            {
                while (!(_sendMsgPMQueue.Count == 0) || !(_receiveMsgPMQueue.Count == 0))
                {
                    if (_sendMsgPMQueue.Count > 0) WritePM((string)_sendMsgPMQueue.Dequeue());
                    //await Task.Delay(100);
                    await Task.Delay(_waitMessageTime);
                }
            });
            return _measureValueOfPM;
        }

        private void OnSCPIMessagePM(byte[] reponse)
        {
            string result = ASCIIEncoding.ASCII.GetString(reponse);
            _measureValueOfPM = float.Parse(result);
            if (_receiveMsgPMQueue.Count == 1)
            {
                //_oneCycleMessagePM += CF.CombineMessage((string)_receiveMsgPMQueue.Dequeue(), 
                //    ASCIIEncoding.ASCII.GetString(reponse));
                // 메시지를 화면에 보여주는 것이 비동기이기 때문에, 복사를 해야 한다.
                //String message = String.Copy(_oneCycleMessagePM);
                //this.BeginInvoke(
                //(MethodInvoker)delegate
                //{
                //    CF.LogWithDt(message);
                //});
                //_oneCycleMessagePM = "";
                _receiveMsgPMQueue.Dequeue();
            }
            else
            {
                //_oneCycleMessagePM += CF.CombineMessage((string)_receiveMsgPMQueue.Dequeue(), 
                //    ASCIIEncoding.ASCII.GetString(reponse)) + "|";
                _receiveMsgPMQueue.Dequeue();
                if (_sendMsgPMQueue.Count > 0) WritePM((string)_sendMsgPMQueue.Dequeue());
            }
        }

        private async Task ExecCableLossScenario(Hashtable a_hashtable, CancellationToken ct)
        {
            float step = 0;
            float[] start = new float[10];
            float[] end = new float[10];
            int freqCount = 0;
            int amplitude = 0;
            string freqUnit = "";
            string amplitudeUnit = "";

            // STEP 1. 계측기 연결 메시지 창
            // [Power Meter]의 Sensor와 [SG]의 RF Output port을 연결하라는 팝업 창 띄움. 팝업 창의 OK 버튼을 누르면 2번 실행
            DialogResult resultDlg1 = MessageBox.Show("Power Meter의 Sensor와 RF Output port를 연결해 주세요!", 
                "1단계", MessageBoxButtons.OKCancel);
            if (resultDlg1 != DialogResult.OK) return;

            Hashtable sgOffsetTable = new Hashtable();


            if (a_hashtable.ContainsKey("Step").Equals(true))
            {
                //ParamSet stepSet = a_hashtable["Step"] as ParamSet;
                ParamSetJson stepSet = CF.GetParamSet("Step", a_hashtable);
                step = float.Parse(stepSet.Value);
            }


            if (a_hashtable.ContainsKey("Amplitude").Equals(true))
            {
                //ParamSet ampSet = a_hashtable["Amplitude"] as ParamSet;
                ParamSetJson ampSet = CF.GetParamSet("Amplitude", a_hashtable);
                amplitudeUnit = ampSet.Unit;
                amplitude = int.Parse(ampSet.Value);
            }

            if (a_hashtable.ContainsKey("Frequency").Equals(true))
            {
                //ParamSet FreqSet = a_hashtable["Frequency"] as ParamSet;
                ParamSetJson FreqSet = CF.GetParamSet("Frequency", a_hashtable);
                freqUnit = FreqSet.Unit;
                string[] parseFreqValue = FreqSet.Value.Split(',');
                freqCount = parseFreqValue.Length;

                for (int i = 0; i < parseFreqValue.Length; i++)
                {
                    string[] rangeFreq = parseFreqValue[i].Split('~');
                    start[i] = float.Parse(rangeFreq[0]);
                    end[i] = float.Parse(rangeFreq[1]);
                }
            }

            // STEP 2. GUI에서 입력한 주파수 범위의 첫 번째 주파수로 [Power Meter] 및 [SG]의 주파수를 설정
            // STEP 3. [SG] Amplitude level을 GUI에서 입력한 값으로 설정하고 RF On, Mod Off 시킴

            await SendSGAmplitudeMessage(amplitude, amplitudeUnit);

            // STEP 4. [Power Meter] 에서 측정된 값을 읽은 후, 아래의 계산으로 해당 주파수에 대한[SG] Offset 측정
            // SG Offset = SG Level - MeasuredValue of Power Meter

            // STEP 5. [Power Meter] 및[SG] 주파수를 step만큼 변경하여 4 의 과정을 실행하면서 주파수 별[SG] offset 값을 PC에 저장

            // STEP 6.마지막 주파수까지 4 ~ 5의 과정을 반복
            sgOffsetTable = await AsyncOffSetRoutine(step, start, end, freqCount, amplitude, freqUnit, sgOffsetTable, false, ct);
            await _FileUtil.ProcessWriteAsync("SG_OffSet", sgOffsetTable);

            // STEP 7. 
            DialogResult resultDlg2 = MessageBox.Show("Calibration에 사용할 RF Cable과 [SG]을 연결 해 주세요!",
                                "7단계", MessageBoxButtons.OKCancel);

            // STEP 8. GUI에서 입력한 주파수 범위의 첫번째 주파수로 [Power Meter] 및 [SG] 주파수를 설정

            // STEP 9. [Power Meter]에서 측정된 값을 읽은 후, 아래의 계산으로 해당 주파수에 대한 Cable Offset 측정
            // Cable Offset = SG Level - MeasuredValue of Power Meter + SG Offset
            if (resultDlg2 == DialogResult.OK)
            {
                Hashtable cableOffsetTable = await AsyncOffSetRoutine(step, start, end, freqCount, amplitude, freqUnit, sgOffsetTable, true, ct);
                await _FileUtil.ProcessWriteAsync("Cable_OffSet", cableOffsetTable);
            }
            MessageBox.Show("Cable Loss 수행을 완료 했습니다!");
        }

        private async Task<Hashtable> AsyncOffSetRoutine(float step, float[] start, float[] end, int freqCount, int amplitude,
            string unit, Hashtable sgOffsetTable, bool isCableOffSet, CancellationToken ct)
        {
            Hashtable offsetTable = new Hashtable();
            for (int i = 0; i < freqCount; i++)
            {
                for (float j = start[i]; j <= end[i]; j = j + step)
                {
                    if (j != start[i])
                        j = (float)Math.Truncate(j);
                    if (j > end[i])
                        j = end[i];

                    // 주파수: j, SG Level: amplitude, Power Meter에서 측정된 값: measuredValue
                    if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                    await SendSGFrequencyMessage(j, unit);

                    await SendPMFrequencyMessage(j, unit);

                    _sendMsgPMQueue.Enqueue("MEASURE?");
                    _receiveMsgPMQueue.Enqueue("MEASURE?");
                    float measuredValue = await AsyncSendMessagePM();
                    string measuredMessage = "Freq: " + j.ToString("F2") + "MHz | Measured: " + measuredValue;
                    CallLogMessageWithDt(measuredMessage);

                    if (isCableOffSet)
                    {
                        // float cableOffSet = amplitude - measuredValue + (float)sgOffsetTable[j];
                        //float cableOffSet = amplitude - measuredValue - (float)sgOffsetTable[j] - (float)sgOffsetTable[j];
                        float cableOffSet = amplitude - measuredValue;
                        offsetTable.Add(j, cableOffSet);
                    }
                    else
                    {
                        // SG Offset = SG Level - Measured Value of Power Meter
                        float sgOffSet = amplitude - measuredValue;
                        offsetTable.Add(j, sgOffSet);
                    }
                }
            }
            return offsetTable;
        }

        private async Task SendPMFrequencyMessage(float frequency, string freqUnit)
        {
            string freqCmdPM = "SENS:FREQ " + frequency + " " + freqUnit;
            _sendMsgPMQueue.Enqueue(freqCmdPM);
            //_receiveMsgPMQueue.Enqueue(freqCmdPM);
            if (_sendMsgPMQueue.Count > 0) WriteSG((string)_sendMsgPMQueue.Dequeue());
            await Task.Delay(_waitMessageTime);
        }
    }
}