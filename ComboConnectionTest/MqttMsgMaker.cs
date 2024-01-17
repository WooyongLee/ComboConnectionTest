using System.Collections;

namespace ComboConnectionTest
{
    public static class MqttMsgMaker
    {
        /// <summary>
        /// 메시지 수신 요청
        /// </summary>
        /// <param name="hashtable">Send 대상 파라미터 테이블</param>
        public static Queue SendMessage(Hashtable hashtable)
        {
            Queue send5GmsgMqttQueue = new Queue();

            // (1), (2)
            send5GmsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.SA).ToString("X2")); // CMD
            send5GmsgMqttQueue.Enqueue("0x" + ((byte)MEASURE_TYPE.SWEPT_SA).ToString());

            // [SG] 설정값 (3)
            ParamSetJson freqSet = JsonManager.GetParamSet("Frequency", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(freqSet.Value) * 1000000);

            // [SG] 설정값
            ParamSetJson ampParamSet = JsonManager.GetParamSet("SGAmplitude", hashtable);
            // send5GmsgMqttQueue.Enqueue(int.Parse(ampParamSet.Value)); // 다른 큐에 넣는걸로 되어있음

            // (4)
            ParamSetJson spanSet = JsonManager.GetParamSet("PACTSpan", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(spanSet.Value) * 1000000); // SPAN

            // (5)
            ParamSetJson rbwSet = JsonManager.GetParamSet("PACTRBW", hashtable);
            RBW_MAP rbwValue = JsonManager.GetRBWValue(rbwSet);
            send5GmsgMqttQueue.Enqueue(((byte)rbwValue).ToString("D")); // RBW

            // (6)
            ParamSetJson vbwSet = JsonManager.GetParamSet("PACTVBW", hashtable);
            VBW_MAP vbwValue = JsonManager.GetVBWValue(vbwSet);
            send5GmsgMqttQueue.Enqueue(((byte)vbwValue).ToString("D")); // VBW

            // (7)
            string ampitudeMode = "0"; // ZERO PADDING 이라고 하여 0을 부여함
            send5GmsgMqttQueue.Enqueue(ampitudeMode);

            // (8)
            ParamSetJson attenuatorSet = JsonManager.GetParamSet("PACTAttenuator", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(attenuatorSet.Value));

            // (9)
            LNA_SET _LNA_Set = LNA_SET.LNA_OFF; // LNA 임시로 OFF모드 설정
            send5GmsgMqttQueue.Enqueue(((byte)_LNA_Set).ToString("D"));

            // (10)
            string _fixedValuesPadding = "0 0 0 1 0 2 1 0 2 1 0 2 1 1 5000 0 0 1000 0 800 1 1 100 0 0 0"; // ???, 해당 프로토콜 뒤쪽 필드 채우는 부분인지
            send5GmsgMqttQueue.Enqueue(_fixedValuesPadding);

            return send5GmsgMqttQueue;
        }

        public static Queue SendMessage(string strText)
        {
            Queue mqttQueue = new Queue();

            string[] strSplited = strText.Split(' ');
            foreach (var str in strSplited)
            {
                mqttQueue.Enqueue(str);
            }

            return mqttQueue;
        }
    }
}
