/*
 * Form에 있는 Component들을 쓰기 위해 파일만 분리
 */

#define _CONNECT_SG_

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using FluentFTP;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace DabinPACT {
    
    public delegate void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e);
    public delegate void HandleFtpMessage(string message);

    public partial class frmMain : Form {

        public MQTTClass _mqSubscriber;
        public MQTTClass _mqBinSubscriber;
        public MQTTClass _mqPublisher;

        public FtpClient _ftpClient;

        private string _mqttServerIp = "";
        private string _mqttPort = "";
        private string _mqttSendTopic = "pact/command";
        private string _mqttReceiveTopic = "pact/data1";
        private string _mqttReceiveTopicBin = "pact/data2";

        private Queue _sendMsgMqttQueue;
        private Queue _receiveMsgMqttQueue;

        private Queue _send5GMsgMqttQueue;

        private string _resultAck = "";

        private string _fixedValuesPadding = "0 0 0 1 0 2 1 0 2 1 0 2 1 1 5000 0 0 1000 0 800 1 1 100 0 0 0";
        private Int32[] _spectrumData = new Int32[1001];
        private bool _bReceivedBin = false;

        private string _BLANK = " ";
        private string _ZERO_PADDING = "0";

        private float _resTemperature = 0.0F;
        private string _resIQImb = "";

        //CancellationToken _cancellationToken;

        private async Task initMqttClient()
        {
            _sendMsgMqttQueue = new Queue();
            _receiveMsgMqttQueue = new Queue();
            _send5GMsgMqttQueue = new Queue();

            // Init MQPublisher (pact/command)
            void initPublisher()
            {
                _mqPublisher = new MQTTClass();
                _mqPublisher.messageHandler += OnPublisherMessage;
                _mqPublisher.receivedMessageHandler += OnPublisherReceivedMessage;
                _mqPublisher.BuildOptions(_mqttServerIp, int.Parse(_mqttPort));
                _mqPublisher.Connect(_mqttSendTopic);
            }

            // Init MQSubscriber (pact/data1)
            void initSubscriber()
            {
                _mqSubscriber = new MQTTClass();
                _mqSubscriber.messageHandler += OnSubscriberMessage;
                _mqSubscriber.receivedMessageHandler += OnSubscriberReceivedMessage;
                _mqSubscriber.BuildOptions(_mqttServerIp, int.Parse(_mqttPort));
                _mqSubscriber.Connect(_mqttReceiveTopic);
                _mqSubscriber.ReceiveMessage();

            }

            // Init MQBinSubscriber(pact/data2)
            void initSubscriberBin()
            {
                _mqBinSubscriber = new MQTTClass();
                _mqBinSubscriber.messageHandler += OnSubscriberMessage;
                _mqBinSubscriber.receivedMessageHandler += OnSubscriberReceivedBinMessage;
                _mqBinSubscriber.BuildOptions(_mqttServerIp, int.Parse(_mqttPort));
                _mqBinSubscriber.Connect(_mqttReceiveTopicBin);
                _mqBinSubscriber.ReceiveMessage();
            }

            await Task.Run(() =>
            {
                initSubscriber();
                initSubscriberBin();
                initPublisher();
                bool done = false;
                var watch = Stopwatch.StartNew();

                while (!done)
                {
                    done = _mqPublisher._mqttClient.IsConnected | _mqSubscriber._mqttClient.IsConnected | _mqBinSubscriber._mqttClient.IsConnected;
                    Task.Delay(_waitPACTTime);
                    if (watch.ElapsedMilliseconds > (long)CF._READ_TIMEOUT * 3)
                    {
                        watch.Stop();
                        done = true;
                    }
                }
            });
        }


        public async Task<bool> AsyncSendMessageMqtt()
        {
            await Task.Run(async () =>
            {
                while (!(_sendMsgMqttQueue.Count == 0) || !(_receiveMsgMqttQueue.Count == 0))
                {
                    if (_sendMsgMqttQueue.Count > 0)
                    {
                        _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(_sendMsgMqttQueue.Dequeue().ToString()));
                        _mqPublisher.SendMessage();
                    }
                    await Task.Delay(_waitPACTTime);
                }
            });
            if (_resultAck == "0x00")
            {
                return true;
            }
            return true;
        }

        // Connect/DisConnet/Error Message
        private void OnSubscriberMessage(string msg)
        {
            CallLogMessage(msg);
        }

        // Connect/DisConnet/Error Message
        private void OnPublisherMessage(string msg)
        {
            CallLogMessage(msg);
        }

        private void OnPublisherReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            CallLogMessage(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
        }

        private void OnSubscriberReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            //_receiveMsgMqttQueue
            if (_receiveMsgMqttQueue.Count == 1)
            {
                string sendMsg = _receiveMsgMqttQueue.Dequeue().ToString();
                string recvMsgPaload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if (recvMsgPaload.Contains(sendMsg)) {
                    // 마지막 1 byte는 보낸 메시지의 ACK 
                    string resultAck = recvMsgPaload.Substring(sendMsg.Length + 1);
                    _resultAck = String.Copy(resultAck);
                   
                    //CallLogMessage(resultAck);
                    Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    if (sendMsg == "0x22") _resTemperature = float.Parse(resultAck.Split(' ')[0]);
                    if (sendMsg == "0x43") _resIQImb = String.Copy(resultAck);
                } else
                {
                    CallLogMessage(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                }
            }
        }

        private void OnSubscriberReceivedBinMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            byte[] receviedSpectrum = e.ApplicationMessage.Payload;

            for (int i = 0; i < _spectrumData.Length; i++)
            {
                _spectrumData[i] = BitConverter.ToInt32(receviedSpectrum, (i + 2)  * 4);
            }
            _bReceivedBin = true;
        }
        
        private async Task ExecGlobalOffsetScenario(Hashtable a_hashtable)
        {
            // STEP 1. [SG] 와 PACT을 RF Cable로 연결
            DialogResult resultDlg = MessageBox.Show("[SG]와 PACT을 RF Cable로 연결해 주세요!",
                                    "Global Offset 1단계", MessageBoxButtons.OKCancel);
            if (resultDlg != DialogResult.OK) return;

            // STEP 2. 외부[SG]의 주파수를 GUI에서 설정한 ‘Frequency’ 으로 설정
            ParamSetJson freqSet = CF.GetParamSet("Frequency", a_hashtable);
#if (_CONNECT_SG_)
            try
            {
                await SendSGFrequencyMessage(float.Parse(freqSet.Value), freqSet.Unit);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
            // STEP 3. [SG] Amplitude 을 GUI에서 입력한 값으로 설정하고 RF ON, Mod OFF 상태로 설정
            ParamSetJson ampParamSet = CF.GetParamSet("SGAmplitude", a_hashtable);
#if (_CONNECT_SG_)
            try
            {
                await SendSGAmplitudeMessage(int.Parse(ampParamSet.Value), ampParamSet.Unit);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
            // STEP 4. PACT 장비를 SA Path 및 Sweep Mode로 동작하도록 API 설정 및 전달
            // TODO: yjpark 전달이라는 문구에 대해서 확인 필요

            _send5GMsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.SA).ToString("X2")); // CMD
            _send5GMsgMqttQueue.Enqueue("0x" + ((byte)MEASURE_TYPE.SWAPT_SA).ToString("X2"));   // MEASURE

            // STEP 5. PACT 장비의 LNA Path 및 GUI에서 설정된 frequency 정보를 설정 및 전달
            // FREQUENCY
            _send5GMsgMqttQueue.Enqueue(ulong.Parse(freqSet.Value) * 1000000);

            // STEP 6. PACT SA path의 Attenuator 을 GUI에서 입력한 값으로 설정
#if (_CONNECT_SG_)
            try
            {
                _sendMsgSGQueue.Enqueue(int.Parse(ampParamSet.Value));
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
            // STEP 7. PACT SA의 RBW, VBW, Span을 GUI에서 입력한 값으로 설정
            ParamSetJson spanSet = CF.GetParamSet("PACTSpan", a_hashtable);
            _send5GMsgMqttQueue.Enqueue(ulong.Parse(spanSet.Value) * 1000000); // SPAN


            ParamSetJson rbwSet = CF.GetParamSet("PACTRBW", a_hashtable);
            RBW_MAP rbwValue = CF.GetRBWValue(rbwSet);
            _send5GMsgMqttQueue.Enqueue(((byte)rbwValue).ToString("D")); // RBW

            ParamSetJson vbwSet = CF.GetParamSet("PACTVBW", a_hashtable);
            VBW_MAP vbwValue = CF.GetVBWValue(vbwSet);
            _send5GMsgMqttQueue.Enqueue(((byte)vbwValue).ToString("D")); // VBW

            _send5GMsgMqttQueue.Enqueue(_ZERO_PADDING); // Ampitude Mode

            ParamSetJson attenuatorSet = CF.GetParamSet("PACTAttenuator", a_hashtable);
            _send5GMsgMqttQueue.Enqueue(ulong.Parse(attenuatorSet.Value)); // Attenuator

            // LNA(0: off, 1: On)
            _send5GMsgMqttQueue.Enqueue(((byte)_LNA_Set).ToString("D")); // LNA

            _send5GMsgMqttQueue.Enqueue(_fixedValuesPadding); // Fixed Values(바뀌지 않는 값)

            // STEP 8. PACT 장비에서 스펙트럼 분석 기능 수행하고 측정 결과인 스펙트럼 파형을 받아옴
            await SendExecGlobalOffsetMessageToPACT();
            // 0x04 00 4Byte * 1001
            // Measurement Mode(4Byte) + Measurement Type(4Byte) + 4Byte x 1001 개의 스펙트럼 데이터가 DUT로부터 전달됨
            // 4 Byte의 1001개 스펙트럼 데이터는 4Byte value / 100 의 값을 cal SW의 Viewer에 표시

            // STEP 9. Viewer에 스펙트럼 파형 표시하고, 최대 값을 찾음.
            _SIGNAL_TITLE = "GlobalOffset";
            CallSignalDraw();

            float maxValue = _spectrumData.Max();

            // STEP 10. ‘Global Offset = [SG]의 Amplitude Level - 최대 값 + Cable offset of current frequency’으로 Global Offset 값을 계산
            // 파일에서 읽어서 처리를 한다.
            await _FileUtil.LoadCableOffset("Cable_OffSet");
            float cableOffset = _FileUtil.FindCurFreqCableOffset(float.Parse(freqSet.Value));
            float globalOffset = int.Parse(ampParamSet.Value) - (maxValue / 100) - cableOffset;

            string globalOffsetMessage = "Freq: " + float.Parse(freqSet.Value).ToString("F2") + "MHz | measured: " 
                + maxValue +  ", Global offset:" + globalOffset.ToString("F2") + ", Cable Offset:" + cableOffset;
            CF.LogWithDt(globalOffsetMessage);

            // 위 수식에서 Cable offset of current frequency의 값은 앞의 ‘cable offset’ 아이템 측정에서 나온 값
            if (_LNA_Set == LNA_SET.LNA_OFF)
            {
                _FileUtil._LNA_OFF_GOFFSET = globalOffset;
            } else if (_LNA_Set == LNA_SET.LNA_ON)
            {
                _FileUtil._LNA_ON_GOFFSET = globalOffset;
            }
            // Calibration File 생성
            // SA_Common.cal 파일을 생성하여 측정된 Global Offset을 binary 파일로 저장한다.
            await _FileUtil.WriteBinaryAsync(_FileUtil._GLOBAL_OFFSET_FNAME);

            EncodingFtpCmd(_FileUtil._GLOBAL_OFFSET_FNAME);
            // SA_Common.cal 파일 생성이 완료되면 자동으로 DUT에 해당 파일을 전송한다.
            await SendFtpScenario(_FileUtil._GLOBAL_OFFSET_FNAME);

            MessageBox.Show("Global OffSet 수행을 완료 했습니다!");
        }

        public async Task SendExecGlobalOffsetMessageToPACT()
        {
            string execStr = "";
            while(_send5GMsgMqttQueue.Count != 0)
            {
                execStr += _send5GMsgMqttQueue.Dequeue().ToString() + _BLANK;
            }
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(execStr));
            _mqPublisher.SendMessage();
            await Task.Delay(_waitPACTTime);
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes("0x11"));
            _mqPublisher.SendMessage();

            await Task.Run(async () =>
            {
                while (!_bReceivedBin)
                {
                    await Task.Delay(10);
                }
                _bReceivedBin = false;
            });
        }

        private async Task ExecPowerAttenScenario(Hashtable a_hashtable, CancellationToken ct)
        {
            // STEP 1. 외부[SG]와 PACT을 RF cable로 연결한다.
            float[] freqStart = new float[10];
            float[] freqEnd = new float[10];
            int attenStart = 0;
            int attenEnd = 0;
            string freqUnit = "";
            int ampStep = 0;
            List<AmplitudeSet> AmpDataList = new List<AmplitudeSet>();

            DialogResult resultDlg = MessageBox.Show("[SG]와 PACT을 RF Cable로 연결해 주세요!",
                                    "Power & Atten Accuracy 1단계", MessageBoxButtons.OKCancel);
            if (resultDlg != DialogResult.OK) return;

            // STEP 2. [SG] Amplitude 을 GUI에서 입력한 값으로 설정하고 RF ON, Mod OFF 상태로 설정한다.
            ParamSetJson ampStepSet = CF.GetParamSet("SGAmplitude", a_hashtable);
            string[] parseAmpValue = ampStepSet.Value.Split(',');
            for (int i = 0; i < parseAmpValue.Length; i++)
            {
                AmpDataList.Add(new AmplitudeSet(float.Parse(parseAmpValue[i].Split('/')[0]), 
                    float.Parse(parseAmpValue[i].Split('/')[1])));
            }
#if (_CONNECT_SG_)
            try
            {
                await SendSGAmplitudeMessage((int)AmpDataList[0].amplitude, ampStepSet.Unit);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
            // STEP 3. [SG]의 주파수를 Start Frequency로 설정한다.
            List<FreqRangeSet> freqDataList = new List<FreqRangeSet>();
            ParamSetJson freqRangeSet = CF.GetParamSet("FrequencyInterval", a_hashtable);
            freqUnit = freqRangeSet.Unit;
            string[] parseFreqValue = freqRangeSet.Value.Split(',');
            for (int i = 0; i < parseFreqValue.Length; i++)
            {
                freqDataList.Add(new FreqRangeSet(float.Parse(parseFreqValue[i].Split('~')[0]),
                    float.Parse(parseFreqValue[i].Split('~')[1])));
            }
            ParamSetJson freqStepSet = CF.GetParamSet("FrequencyStep", a_hashtable);
            
            // STEP 4. PACT의 설정 정보(LNA Path, Frequency, RBW, VBW, Span 등) 와 Attenuator 0 dB을 PACT에 전송한다.
            ParamSetJson spanSet = CF.GetParamSet("PACTSpan", a_hashtable);
            ParamSetJson rbwSet = CF.GetParamSet("PACTRBW", a_hashtable);
            RBW_MAP rbwValue = CF.GetRBWValue(rbwSet);
            ParamSetJson vbwSet = CF.GetParamSet("PACTVBW", a_hashtable);
            VBW_MAP vbwValue = CF.GetVBWValue(vbwSet);

            ParamSetJson attenuatorRangeSet = CF.GetParamSet("AttenuatorRange", a_hashtable);
            string[] attenuatorRange = attenuatorRangeSet.Value.Split('~');
            attenStart = int.Parse(attenuatorRange[0]); // Start
            attenEnd = int.Parse(attenuatorRange[1]); // End
            ParamSetJson attenuatorStepSet = CF.GetParamSet("AttenuatorStep", a_hashtable);
            await _FileUtil.LoadCableOffset("Cable_OffSet");

            List<PowerOffSet> powerOffsetList = new List<PowerOffSet>();
            List<AttenuatorOffSet> attenuatorOffList = new List<AttenuatorOffSet>();

            for (int i = 0; i < freqDataList.Count; i++)
            {
                //for (float j = freqDataList[i].startFreq; j <= freqDataList[i].endFreq; j = j + int.Parse(freqStepSet.Value))
                int freq_cnt = (int)Math.Ceiling((freqDataList[i].endFreq - freqDataList[i].startFreq) / int.Parse(freqStepSet.Value));
                for (int n = 0; n <= freq_cnt; n++)
                {
                    float j = freqDataList[i].startFreq + n * int.Parse(freqStepSet.Value);
                    if (j != freqDataList[i].startFreq)
                        j = (float)Math.Truncate(j);
                    if (j > freqDataList[i].endFreq)
                        j = freqDataList[i].endFreq;
#if (_CONNECT_SG_)
                    try
                    {
                        await SendSGFrequencyMessage(j, freqUnit);
                        if(j <= AmpDataList[0].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[0].amplitude, ampStepSet.Unit);
                        else if(j > AmpDataList[0].freq && j <= AmpDataList[1].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[1].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[1].freq && j <= AmpDataList[2].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[2].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[2].freq && j <= AmpDataList[3].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[3].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[3].freq && j <= AmpDataList[4].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[4].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[4].freq && j <= AmpDataList[5].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[5].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[5].freq && j <= AmpDataList[6].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[6].amplitude, ampStepSet.Unit);
                        else
                            await SendSGAmplitudeMessage((int)AmpDataList[7].amplitude, ampStepSet.Unit);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
                    Thread.Sleep(1000);
                    EncondingPowerAttenMessage(j, spanSet, rbwValue, vbwValue, 0);
                    await SendExecPowerAttenMessageToPACT();
                    // STEP 5. PACT로부터 현재의 온도 정보를 읽어와서 저장한다.
                    // 0x22 메시지를 보내면, 0x22 2개필드가 오는데, 앞에 하나만 사용한다.
                    float resTemperature = await SendReqTemperature();
                    // STEP 6.PACT로부터 스펙트럼 데이터를 수신한 후, 이를 Viewer에 표시하고 최대 값을 찾고 현재 주파수의 power offset을 계산한다.
                    //         Power offset = SG Amplitude level -max power + cable offset of current frequency
                    string powerMessage1 = "Freq: " + j + "MHz | Temperature: " + resTemperature.ToString("F2");
                    CallLogMessageWithDt(powerMessage1);
                    _SIGNAL_TITLE = "PowerOffset";
                    CallSignalDraw();

                    if (AmpDataList[ampStep].freq < j) ampStep++;

                    float maxPower = _spectrumData.Max();
                    float cableOffset = _FileUtil.FindCurFreqCableOffset(j);
                    float powerOffset = AmpDataList[ampStep].amplitude - (maxPower / 100) - cableOffset;

                    string powerMessage2 = "Freq: " + j + "MHz | SG Power: " + AmpDataList[ampStep].amplitude.ToString("F2") + ", MaxPower: " + (maxPower * 0.01) + " | Offset: " + powerOffset + "|Cable Offset: " + cableOffset;
                    CallLogMessageWithDt(powerMessage2);

                    using (StreamWriter outputFile = new StreamWriter(@"..\..\PowerOffset.txt", true))
                    {
                        outputFile.WriteLine("Freq : " + j + "MHz "+ powerOffset);
                    }

                    int powerAccuracy = (short)Math.Round(Math.Pow(10, powerOffset / 20) * 1024);
                    ulong freqValue = (ulong)(j * int.Parse(UNIT_HERZ.MHZ_UNIT.ToString("D")));
                    PowerOffSet poValue = new PowerOffSet(freqValue, resTemperature, powerAccuracy);
                    powerOffsetList.Add(poValue);

                    // att2 = 0
                    Hashtable att1HashTable = await PowerAttenMessage(attenStart, attenEnd, spanSet, rbwValue, vbwValue, attenuatorStepSet, j, maxPower, true, ct);
                    // att2 = 1
                    Hashtable att2HashTable = await PowerAttenMessage(attenStart, attenEnd, spanSet, rbwValue, vbwValue, attenuatorStepSet, j, maxPower, false, ct);

                    for (int k = 0; k < CS._ATTENUATOR.Count; k++)
                    //for (int k = 0; k < 1; k++)
                    {
                        float att1 = (float)att1HashTable[CS._ATTENUATOR[k * 2][0]];
                        float att2 = (float)att2HashTable[CS._ATTENUATOR[k * 2][1]];

                        int attenAccuracy = (short)Math.Round(Math.Pow(10, (att1 + att2) / 20) * 1024);
                        
                        AttenuatorOffSet attenValue = new AttenuatorOffSet(freqValue, (uint)k * 2 , attenAccuracy);                   
                        attenuatorOffList.Add(attenValue);
                    }
                    // STEP 9. 주파수를 Frequency Step만큼 증가시킨 후 3 ~8 의 과정을 반복한다.
                }
            }
            // STEP 10. 모든 주파수에 대해 Power offset 및 Attenuator offset 측정이 완료되면 Calibration Data 및 파일을 생성한 후, 
            // 해당 파일을 PACT 장비에 전송한다.

            // POWER OFFSET 파일 생성
            await _FileUtil.WriteBinPowerOffsetAsync(_LNA_Set == LNA_SET.LNA_OFF ? 
                _FileUtil._POWER_OFFSET_FNAME : _FileUtil._POWER_OFFSET_LNA_ON_FNAME, powerOffsetList);

            EncodingFtpCmd(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._POWER_OFFSET_FNAME : _FileUtil._POWER_OFFSET_LNA_ON_FNAME);
            // SA_PwrOffset.cal 파일 생성이 완료되면 자동으로 DUT에 해당 파일을 전송한다.
            await SendFtpScenario(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._POWER_OFFSET_FNAME : _FileUtil._POWER_OFFSET_LNA_ON_FNAME);


            // ATTENUATOR OFFSET 파일생성
            await _FileUtil.WriteBinAttenOffsetAsync(_LNA_Set == LNA_SET.LNA_OFF ? 
                _FileUtil._ATTEN_OFFSET_FNAME : _FileUtil._ATTEN_OFFSET_LNA_ON_FNAME, attenuatorOffList);
            //_FileUtil.ReadBinAttenOffsetAsync(_LNA_Set == LNA_SET.LNA_OFF ?
            //    _FileUtil._ATTEN_OFFSET_FNAME : _FileUtil._ATTEN_OFFSET_LNA_ON_FNAME);

            EncodingFtpCmd(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._ATTEN_OFFSET_FNAME : _FileUtil._ATTEN_OFFSET_LNA_ON_FNAME);
            // SA_PwrOffset.cal 파일 생성이 완료되면 자동으로 DUT에 해당 파일을 전송한다.
            await SendFtpScenario(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._ATTEN_OFFSET_FNAME : _FileUtil._ATTEN_OFFSET_LNA_ON_FNAME);

            MessageBox.Show("Power OffSet 수행을 완료 했습니다!");
        }

        private async Task <Hashtable> PowerAttenMessage(int attenStart, int attenEnd, ParamSetJson spanSet, RBW_MAP rbwValue, 
            VBW_MAP vbwValue, ParamSetJson attenuatorStepSet, float j, float powerOffset, bool att1, CancellationToken ct)
        {
            Hashtable attenHashTable = new Hashtable();
            for (int att = attenStart; att <= attenEnd; att = att + int.Parse(attenuatorStepSet.Value))
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

                // att1이 true 이면 CS._ATTENUATOR[att, 0]을 사용 att1이 false이면 CS._ATTENUATOR[0, att]를 사용한다.
                // STEP 7. ATT1을 Attenuator Range(0 ~30) 까지 Attenuator Step만큼 증가시키면서 다음과 같이  Attenuator 별 Attenuator Offset을 계산한다.
                // STEP 7-A. ATT2 = 0dB로 설정하고 ATT1을 Attenuator Step 만큼 증가시킨다.
                // STEP 8. ATT2을 Attenuator Range(0 ~30)까지 Attenuator Step만큼 증가시키면서 다음과 같이 Attenuator 별 Attenuator Offset을 계산한다.
                // STEP 8-A. ATT1 = 0dB로 설정하고 ATT2을 Attenuator Step만큼 증가시킨다.
                EncondingPowerAttenMessage(j, spanSet, rbwValue, vbwValue, att1 ? CS._ATTENUATOR_CODE[att, 0] : CS._ATTENUATOR_CODE[0, att]);
                await SendExecPowerAttenMessageToPACT();
                _SIGNAL_TITLE = "PowerOffset";
                CallSignalDraw();

                // STEP 7-B. PACT로부터 스펙트럼 데이터를 수신한 후, 이를 Viewer에 표시하고 최대 값을 이용하여 Attenuator offset을 계산한다.
                //          Attenuator offset = max power calculated by 6 - Target Attenuator - max power of current Attenuator 
                // STEP 8-B. PACT로부터 스펙트럼 데이터를 수신한 후 Viewer에 표시하고 최대 값을 이용하여 Attenuator offset을 계산한다.
                //          Attenuator offset = max power calculated by 6 - Target Attenuator - max power of current Attenuator
                int attValue = att1 ? CS._ATTENUATOR_CODE[att, 0] : CS._ATTENUATOR_CODE[0, att];
                //float maxPowerOffsetCurAtt = _spectrumData.Max() - attValue * 100;
                float maxPowerOffsetCurAtt = att == 0 ? powerOffset : _spectrumData.Max() - attValue * 100;

                // powerOffset 값이 아니라, max power calculated by 6는 maxPowerOffsetCurAtt 이다.
                //float attenuatorOffset = powerOffset - att - maxPowerOffsetCurAtt;
                //float attenuatorOffset = maxPowerOffsetCurAtt - att - maxPowerOffsetCurAtt;
                float attenuatorOffset = (powerOffset - maxPowerOffsetCurAtt) * 0.01f - att;
                float expected = (powerOffset * 0.01f) - att;
                string attenMessage = "Atten: " + att + " | Expected: " + expected.ToString("F2") + ", Measured: " 
                    + (maxPowerOffsetCurAtt*0.01).ToString("F2") + " | Offset: " + attenuatorOffset.ToString("F2");
                //CF.LogWithDt(attenMessage);
                CallLogMessageWithDt(attenMessage);
                attenHashTable.Add(att, attenuatorOffset);
            }
            return attenHashTable;
        }

        private void CallSignalDraw()
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                OnDrawSignal();
            });
        }

        public void EncondingPowerAttenMessage(float j, ParamSetJson a_spanSet, RBW_MAP a_rbwValue, VBW_MAP a_vbwValue, int attValue)
        {
            _send5GMsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.SA).ToString("X2")); // CMD
            _send5GMsgMqttQueue.Enqueue("0x" + ((byte)MEASURE_TYPE.SWAPT_SA).ToString("X2"));   // MEASURE

            // 변하는 값 FREQUENCY
            //_send5GMsgMqttQueue.Enqueue(j * 1000000);
            _send5GMsgMqttQueue.Enqueue((ulong)(j * 1000) * 1000);
            //_send5GMsgMqttQueue.Enqueue(float.Parse(a_spanSet.Value) * 1000000); // SPAN
            _send5GMsgMqttQueue.Enqueue(ulong.Parse(a_spanSet.Value) * 1000000); // SPAN
            _send5GMsgMqttQueue.Enqueue(((byte)a_rbwValue).ToString("D")); // RBW
            _send5GMsgMqttQueue.Enqueue(((byte)a_vbwValue).ToString("D")); // VBW
            _send5GMsgMqttQueue.Enqueue(_ZERO_PADDING); // Ampitude Mode
            _send5GMsgMqttQueue.Enqueue(attValue.ToString()); // Attenuator
            _send5GMsgMqttQueue.Enqueue(((byte)_LNA_Set).ToString("D")); // LNA(0: off, 1: On)
            _send5GMsgMqttQueue.Enqueue(_fixedValuesPadding); // Fixed Values(바뀌지 않는 값)
        }

        public async Task SendExecPowerAttenMessageToPACT()
        {
            string execStr = "";
            while (_send5GMsgMqttQueue.Count != 0)
            {
                execStr += _send5GMsgMqttQueue.Dequeue().ToString() + _BLANK;
            }
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(execStr));
            _mqPublisher.SendMessage();
            await Task.Delay(_waitPACTTime);
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes("0x11"));
            _mqPublisher.SendMessage();
            // wait Receive Binary data
            await Task.Run(async () =>
            {
                while (!_bReceivedBin)
                {
                    await Task.Delay(10);
                }
                _bReceivedBin = false;
            });
        }

        public async Task<float> SendReqTemperature()
        {
            _sendMsgMqttQueue.Enqueue("0x22");
            _receiveMsgMqttQueue.Enqueue("0x22");
            bool result = await AsyncSendMessageMqtt();
            await Task.Delay(_waitPACTTime);
            return _resTemperature;
        }

        private async Task ExecIQimbalanceScenario(Hashtable a_hashtable, CancellationToken ct)
        {
            List<AmplitudeSet> AmpDataList = new List<AmplitudeSet>();

            MessageBox.Show("IQ imbalance 수행을 시작합니다!");

            string freqUnit = "";
            // STEP 1. [SG] Amplitude 을 GUI에서 입력한 값으로 설정하고 RF ON, Mod OFF 상태로 설정
            //ParamSetJson ampParamSet = CF.GetParamSet("SGAmplitude", a_hashtable);
            ParamSetJson ampStepSet = CF.GetParamSet("SGAmplitude", a_hashtable);
            string[] parseAmpValue = ampStepSet.Value.Split(',');
            for (int i = 0; i < parseAmpValue.Length; i++)
            {
                AmpDataList.Add(new AmplitudeSet(float.Parse(parseAmpValue[i].Split('/')[0]),
                    float.Parse(parseAmpValue[i].Split('/')[1])));
            }
#if (_CONNECT_SG_)
            //try
            //{
            //    await SendSGAmplitudeMessage(int.Parse(ampParamSet.Value), ampParamSet.Unit);
            //}
            //catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            try
            {
                await SendSGAmplitudeMessage((int)AmpDataList[0].amplitude, ampStepSet.Unit);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
            ParamSetJson freqOffsetSet = CF.GetParamSet("SGFrequencyOffset", a_hashtable);

            ParamSetJson attenuatorSet = CF.GetParamSet("PACTAttenuator", a_hashtable);

            ParamSetJson carrierThresholdSet = CF.GetParamSet("CarrierThreshold", a_hashtable);

            ParamSetJson imagThresholdSet = CF.GetParamSet("ImagThreshold", a_hashtable);

            List<FreqRangeSet> freqDataList = new List<FreqRangeSet>();
            ParamSetJson freqRangeSet = CF.GetParamSet("Frequency", a_hashtable);
            freqUnit = freqRangeSet.Unit;
            string[] parseFreqValue = freqRangeSet.Value.Split(',');
            for (int i = 0; i < parseFreqValue.Length; i++)
            {
                freqDataList.Add(new FreqRangeSet(float.Parse(parseFreqValue[i].Split('~')[0]),
                    float.Parse(parseFreqValue[i].Split('~')[1])));
            }
            ParamSetJson freqStepSet = CF.GetParamSet("Step", a_hashtable);

            List<IQImbalance> iqImbalanceList = new List<IQImbalance>();

            for (int i = 0; i < freqDataList.Count; i++)
            {
                //for (float j = freqDataList[i].startFreq; j <= freqDataList[i].endFreq; j += int.Parse(freqStepSet.Value))
                int freq_cnt = (int)Math.Ceiling((freqDataList[i].endFreq - freqDataList[i].startFreq) / int.Parse(freqStepSet.Value));
                for (int n = 0; n <= freq_cnt; n++)
                {
                    float j = freqDataList[i].startFreq + n * int.Parse(freqStepSet.Value);
                    if (j != freqDataList[i].startFreq)
                        j = (float)Math.Truncate(j);
                    if (j > freqDataList[i].endFreq)
                        j = freqDataList[i].endFreq;
                    // STEP 2. [SG] 의 주파수를 Current Frequency + [SG] Frequency Offset 으로 설정
                    if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                    float sgFreqPlusValue = j + float.Parse(freqOffsetSet.Value);
#if (_CONNECT_SG_)
                    try
                    {
                        await SendSGFrequencyMessage(sgFreqPlusValue, freqUnit);
                        if (j <= AmpDataList[0].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[0].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[0].freq && j <= AmpDataList[1].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[1].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[1].freq && j <= AmpDataList[2].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[2].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[2].freq && j <= AmpDataList[3].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[3].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[3].freq && j <= AmpDataList[4].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[4].amplitude, ampStepSet.Unit);
                        else if (j > AmpDataList[4].freq && j <= AmpDataList[5].freq)
                            await SendSGAmplitudeMessage((int)AmpDataList[5].amplitude, ampStepSet.Unit);
                        else
                            await SendSGAmplitudeMessage((int)AmpDataList[6].amplitude, ampStepSet.Unit);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
                    Thread.Sleep(500);
                    // STEP 3. PACT 장비를 제어하기 위한 command을 전달
                    // STEP 3-1. PACT SA Path, LNA Path, Attenuator, Frequency 설정 정보
                    // STEP 3-2. CMD 모드를 ‘IQ Calibration’으로 설정
                    EncondingIQImbMessage(j, byte.Parse(attenuatorSet.Value), (byte)_LNA_Set, 
                        long.Parse(freqOffsetSet.Value), int.Parse(carrierThresholdSet.Value), int.Parse(imagThresholdSet.Value));

                    // STEP 4. PACT 장비로부터 IQ Imbalance Calibration 결과를 받아옴
                    string resultPlus = await SendExecIQimbalanceMessageToPACT();
                    IQImbMessage iQImbPlus = CF.GetIQImbMessageParse(resultPlus);

                    // STEP 5. 받아온 Calibration 결과를 ‘Process’ window에 표시
                    // STEP 5-1. 받아온 Image Suppression 결과가 GUI에서 설정한 Image Suppression보다 작으면 Pass, 크면 Fail로 표시
                    // STEP 5-2. 받아온 Carrier Suppression 결과가 GUI에서 설정한 Carrier Suppression보다 작으면 Pass, 크면 Fail로 표시
                    bool res = iQImbPlus.imageSuppresion < int.Parse(imagThresholdSet.Value) | iQImbPlus.carrierSuppression < int.Parse(carrierThresholdSet.Value);

                    string logMessagePlus = "Feq: " + j +"MHz, " + "Offset: +5 | " + "CW: " + (iQImbPlus.cwValue * 0.01) + 
                        ", Carrier: " + (iQImbPlus.carrierValue * 0.01) + ", Image Supp: " + (iQImbPlus.imageSuppresion * 0.01) + 
                        ", DC Supp: " + (iQImbPlus.carrierSuppression * 0.01) + ", I Gain: " + (iQImbPlus.iGain * 0.00001) + 
                        ", Q Gain: " + (iQImbPlus.qGain * 0.00001) + ", Phase Skew: " + (iQImbPlus.phaseSkew * 0.00001) + 
                        ", I Offset: " + iQImbPlus.iOffset + ", Q Offset: " + iQImbPlus.qOffset;
                    CallLogMessageWithResultDt(logMessagePlus, iQImbPlus.result, res);

                    // STEP 6. [SG]의 주파수를 Current Frequency – [SG] Frequency Offset으로 설정한 후, 3 ~ 5의 과정을 반복
                    float sgFreqMinusValue = j - float.Parse(freqOffsetSet.Value);
#if (_CONNECT_SG_)
                    try
                    {
                        await SendSGFrequencyMessage(sgFreqMinusValue, freqUnit);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
#endif
                    Thread.Sleep(500);
                    EncondingIQImbMessage(j, byte.Parse(attenuatorSet.Value), (byte)_LNA_Set, 
                        -long.Parse(freqOffsetSet.Value), float.Parse(carrierThresholdSet.Value), float.Parse(imagThresholdSet.Value));

                    string resultMinus = await SendExecIQimbalanceMessageToPACT();
                    IQImbMessage iQImbMinus = CF.GetIQImbMessageParse(resultMinus);

                    string logMessageMinus = "Feq: " + j + "MHz, " + "Offset: -5 | " + "CW: " + (iQImbMinus.cwValue * 0.01) +
                        ", Carrier: " + (iQImbMinus.carrierValue * 0.01) + ", Image Supp: " + (iQImbMinus.imageSuppresion * 0.01) +
                        ", DC Supp: " + (iQImbMinus.carrierSuppression * 0.01) + ", I Gain: " + (iQImbMinus.iGain * 0.00001) +
                        ", Q Gain: " + (iQImbMinus.qGain * 0.00001) + ", Phase Skew: " + (iQImbMinus.phaseSkew * 0.00001) +
                        ", I Offset: " + iQImbMinus.iOffset + ", Q Offset: " + iQImbMinus.qOffset;
                    CallLogMessageWithResultDt(logMessageMinus, iQImbMinus.result, res);

                    // STEP 7. PACT 장비로부터 전달받은 두 주파수에 대한 Calibration 값을 이용하여 Calibration Data을 생성
                    // STEP 7-1. Calibration Data 생성 방법은 다음 페이지 참조
                    IQImbalance iQImbCalData = CF.GetIQImbalanceData((ulong)(j * 1000000), iQImbPlus, iQImbMinus);
                    // STEP 8. 마지막 주파수가 될 때까지 주파수를 step만큼 변경하면서 2 ~ 7의 과정을 반복
                    iqImbalanceList.Add(iQImbCalData);
                }
            }
            // STEP 9. Fail없이 마지막 주파수까지 Calibration이 완료되면 자동으로 IQ_Imb.cal 파일을 생성하고 측정된 calibration 결과를 binary 파일로 생성하여 DUT에 전달
            // IQ_Imb 파일 생성
            await _FileUtil.WriteBinIQImbalanceAsync(_LNA_Set == LNA_SET.LNA_OFF ?
                _FileUtil._IQ_IMBALANCE_FNAME : _FileUtil._IQ_IMBALANCE_LNA_ON_FNAME, iqImbalanceList);

            _FileUtil.ReadBinIQImbalanceAsync(_LNA_Set == LNA_SET.LNA_OFF ?
                _FileUtil._IQ_IMBALANCE_FNAME : _FileUtil._IQ_IMBALANCE_LNA_ON_FNAME);

            EncodingFtpCmd(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._IQ_IMBALANCE_FNAME : _FileUtil._IQ_IMBALANCE_LNA_ON_FNAME);
            // SA_PwrOffset.cal 파일 생성이 완료되면 자동으로 DUT에 해당 파일을 전송한다.
            await SendFtpScenario(_LNA_Set == LNA_SET.LNA_OFF ? _FileUtil._IQ_IMBALANCE_FNAME : _FileUtil._IQ_IMBALANCE_LNA_ON_FNAME);

            MessageBox.Show("IQ Imbalance 수행을 완료 했습니다!");
        }

        public void EncondingIQImbMessage(float freq, byte AttenValue, byte PreampValue, long freqOffset, float carrierTh, float imageTh)
        {
            // 0x43 1000 0 100 0 10 100 100
            _send5GMsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.IQ_IMB_CAL).ToString("X2")); // CMD
                                                                                            // 변하는 값 FREQUENCY
            _send5GMsgMqttQueue.Enqueue(freq * 100); // KHz

            // Amplitude
            _send5GMsgMqttQueue.Enqueue(_ZERO_PADDING); // mode Fixed ‘0’
            _send5GMsgMqttQueue.Enqueue(AttenValue); // Attenuator
            _send5GMsgMqttQueue.Enqueue(PreampValue); // Preamp
                        
            _send5GMsgMqttQueue.Enqueue(freqOffset * 100); // SG Frequency Offset(KHz)
            _send5GMsgMqttQueue.Enqueue(carrierTh * 100); // Carrier Threshold: GUI 설정 값 x 100           
            _send5GMsgMqttQueue.Enqueue(imageTh * 100); // Image Threshold: GUI 설정 값 x 100
        }

        public async Task<string> SendExecIQimbalanceMessageToPACT()
        {
            string execStr = "";
            while (_send5GMsgMqttQueue.Count != 0)
            {
                execStr += _send5GMsgMqttQueue.Dequeue().ToString() + _BLANK;
            }

            _sendMsgMqttQueue.Enqueue(execStr);
            _receiveMsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.IQ_IMB_CAL).ToString("X2"));
            bool result = await AsyncSendMessageMqtt();
            await Task.Delay(_waitPACTTime);
            return _resIQImb;
        }
    }
}