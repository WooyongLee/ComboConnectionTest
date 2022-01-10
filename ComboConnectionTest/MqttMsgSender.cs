using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComboConnectionTest
{
    public class MqttMsgSender
    {
        // Send할 데이터를 쌓아놓는 큐
        Queue send5GmsgMqttQueue;

        public MqttMsgSender()
        {
            send5GmsgMqttQueue = new Queue();
        }

        /// <summary>
        /// 메시지 수신 요청
        /// </summary>
        /// <param name="hashtable">Send 대상 파라미터 테이블</param>
        public async Task SendMessage(Hashtable hashtable)
        {
            send5GmsgMqttQueue.Enqueue("0x" + ((byte)CMD_MODE.SA).ToString("X2")); // CMD
            send5GmsgMqttQueue.Enqueue("0x" + ((byte)MEASURE_TYPE.SWEPT_SA).ToString());

            // [SG] 설정값
            ParamSetJson freqSet = JsonManager.GetParamSet("Frequency", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(freqSet.Value) * 1000000);

            // [SG] 설정값
            ParamSetJson ampParamSet = JsonManager.GetParamSet("SGAmplitude", hashtable);
            // send5GmsgMqttQueue.Enqueue(int.Parse(ampParamSet.Value)); // 다른 큐에 넣는걸로 되어있음

            ParamSetJson spanSet = JsonManager.GetParamSet("PACTSpan", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(spanSet.Value) * 1000000); // SPAN

            ParamSetJson rbwSet = JsonManager.GetParamSet("PACTRBW", hashtable);
            RBW_MAP rbwValue = JsomManager
            send5GmsgMqttQueue.Enqueue(ulong.Parse(spanSet.Value) * 1000000); // RBW

            ParamSetJson vbwSet = JsonManager.GetParamSet("PACTVBW", hashtable);
            send5GmsgMqttQueue.Enqueue(ulong.Parse(spanSet.Value) * 1000000); // VBW



        }
    }
}
