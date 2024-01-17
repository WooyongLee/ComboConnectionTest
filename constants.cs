using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text.Json;
using Newtonsoft.Json;

namespace DabinPACT {
	//public enum enum_Thread_Switch {
	//	ets_DoNOT = -1, ets_TerminateNow,
	//	ets_Registered, ets_Attached, ets_Detached,
	//	ets_ViewMode, ets_Rotate, ets_LoadFromFile,
	//	ets_CommandSynch, ets_TaskPicture, ets_DeleteAll, ets_MoveFile
	//}

	public class MenuNode {
		public string NodeName { get; set; }
		public string DisplayName { get; set; }
		public string FunctionName { get; set; }
		public MenuNode SubNode { get; set; }
	}

	public enum UNIT_HERZ
	{
		KHZ_UNIT = 1000,
		MHZ_UNIT = 1000000
	}

	public enum FTP_CODE
	{
		MODE = 0x04,
		TYPE = 0x07,
		CAL_READY = 0x10,
		FILE_TRANS_START = 0x11,
		FILE_TRANS_DONE = 0x12,
		CAL_COMPLETE = 0x15
	}

	public enum CMD_MODE
	{
		VSWR = 0x01,
		DTF = 0x02,
		CABLE_LOSS = 0x03,
		SA = 0x04,
		_5G_NR = 0x41,
		DL_DEMODULATION = 0x42,
		IQ_IMB_CAL = 0x43,
		SG = 0x05
	}

	public enum MEASURE_TYPE
	{
		SWAPT_SA = 0x00, // Cal Mode
		CHANNEL_POWER = 0x01,
		OCCUPIED_BW = 0x02,
		ACLR = 0x03,
		SEM = 0x04,
		TRANSMIT_ON_OFF = 0x05
	}

	public enum RBW_MAP
	{
		KHz_1 = 6,
		KHz_3 = 7,
		KHz_10,
		KHz_30,
		KHz_100,
		KHz_300,
		KHz_1000,
		KHz_3000
	}

	public enum VBW_MAP
	{
		KHz_1 = 6,
		KHz_3 = 7,
		KHz_10,
		KHz_30,
		KHz_100,
		KHz_300,
		KHz_1000,
		KHz_3000
	}

	public enum LNA_SET
	{
		LNA_OFF = 0,
		LNA_ON = 1,
		LNA_ON_OFF =2
	}

	//Frequency(Unsigned 64b) | Temperature(Float 32b)| Power Accuracy(Signed 32b)
	public class PowerOffSet
	{
		public PowerOffSet(ulong a_freqValue, float a_tempValue, int a_powerAccuracy)
		{
			this.freqValue = a_freqValue;
			this.tempValue = a_tempValue;
			this.powerAccuracy = a_powerAccuracy;
		}
		public ulong freqValue { get; set; }
		public float tempValue { get; set; }
		public int powerAccuracy { get; set; }
	}

	//Frequency(Unsigned 64b) | Attenuator(Unsigned 32b) | Atten Accuracy(Unsigned 32b)
	public class AttenuatorOffSet
	{
		public AttenuatorOffSet(ulong a_freqValue, uint a_attenValue, int a_attenAccuracy)
		{
			this.freqValue = a_freqValue;
			this.attenValue = a_attenValue;
			this.attenAccuracy = a_attenAccuracy;
		}
		public ulong freqValue { get; set; }
		public uint attenValue { get; set; }
		public int attenAccuracy { get; set; }
	}

	//Frequency(Unsigned 64b) | A(Signed 32b) int | B(Signed 32b) | C(Signed 32b) | D(Signed 32b) | E(Signed 32b) | F(Signed 32b)
	public class IQImbalance
	{
		public IQImbalance() { }

		public IQImbalance(ulong a_freqValue, int a_A, int a_B, int a_C, int a_D, int a_E, int a_F)
		{
			this.freqValue = a_freqValue;
			this.A = a_A;
			this.B = a_B;
			this.C = a_C;
			this.D = a_D;
			this.E = a_E;
			this.F = a_F;

		}
		public ulong freqValue { get; set; }
		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }
		public int D { get; set; }
		public int E { get; set; }
		public int F { get; set; }
	}

	public class IQImbMessage
	{
		public IQImbMessage(string a_cwValue, string a_imageValue, string a_carrierValue, string a_imageSuppresion, string a_carrierSuppression,
			string a_iGain, string a_qGain, string a_phaseSkew, string a_iOffset, string a_qOffset, string a_result)
		{
			this.cwValue = short.Parse(a_cwValue);
			this.imageValue = short.Parse(a_imageValue);
			this.carrierValue = short.Parse(a_carrierValue);
			this.imageSuppresion = short.Parse(a_imageSuppresion);
			this.carrierSuppression = short.Parse(a_carrierSuppression);
			this.iGain = int.Parse(a_iGain);
			this.qGain = int.Parse(a_qGain);
			this.phaseSkew = int.Parse(a_phaseSkew);
			this.iOffset = int.Parse(a_iOffset);
			this.qOffset = int.Parse(a_qOffset);
			this.result = int.Parse(a_result);
		}

		public short cwValue { get; set; }
		public short imageValue { get; set; }
		public short carrierValue { get; set; }
		public short imageSuppresion { get; set; }
		public short carrierSuppression { get; set; }
		public int iGain { get; set; }
		public int qGain { get; set; }
		public int phaseSkew { get; set; }
		public int iOffset { get; set; }
		public int qOffset { get; set; }
		public int result { get; set; }
	}

	public class ResultSet
	{
		public ResultSet(string a_Name, int a_Unit, string a_StringUnit, string a_Digit)
		{
			this.Name = a_Name;
			this.Unit = a_Unit;
			this.StringUnit = a_StringUnit;
			this.Digit = a_Digit;
		}
		public string Name { get; set; }
		public int Unit { get; set; }
		public string StringUnit { get; set; }
		public string Digit { get; set; }
	}

	public class AmplitudeSet
	{
		public AmplitudeSet (float a_freq, float a_amplitude)
		{
			this.freq = a_freq;
			this.amplitude = a_amplitude;
		}
		public float freq { get; set; }
		public float amplitude { get; set; }
	}

	public class FreqRangeSet
	{
		public FreqRangeSet(float a_start, float a_end)
		{
			this.startFreq = a_start;
			this.endFreq = a_end;
		}
		public float startFreq { get; set; }
		public float endFreq { get; set; }
	}

	public class ParamSetJson
	{
		[JsonProperty("Name")]
		public string Name { get; set; }
		[JsonProperty("Display")]
		public string Display { get; set; }
		[JsonProperty("Value")]
		public string Value { get; set; }
		[JsonProperty("Unit")]
		public string Unit { get; set; }
	}

	public class ServerInfo
	{
		[JsonProperty("Name")]
		public string Name { get; set; }
		[JsonProperty("Display")]
		public string Display { get; set; }
		[JsonProperty("Ip")]
		public string Ip { get; set; }
		[JsonProperty("Port")]
		public string Port { get; set; }
	}

	public class ParamSet
	{
		public string Value { get; set; }
		public string Unit { get; set; }
	}

	public static class CS {
		public const string ProgramName = "Dabin PACT";

		public static readonly Color cError = Color.Red;
		public static readonly Color cNormal = Color.White;

		public const string sRetry = "RETRY";

		public static MultiKeyDictionary<int, int, int> _ATTENUATOR_CODE = new MultiKeyDictionary<int, int, int>()
		{
			{ 0, 0, 61 }, { 2,0,62 }, { 4,0,63 }, { 6,0,64 }, { 8,0,65 },
			{ 10,0,66 }, { 12,0,67 }, { 14,0,68 }, { 16,0,69 }, { 18,0,70 },
			{ 20,0,71 }, { 22,0,72 }, { 24,0,73 }, { 26,0,74 }, { 28,0,75 },
			{ 30,0,76 }, { 0,2,77 }, { 0,4,78 }, { 0,6,79 }, { 0,8,80 },
			{ 0,10,81 }, { 0,12,82 }, { 0,14,83 }, { 0,16,84 }, { 0,18,85 },
			{ 0,20,86 }, { 0,22,87 }, { 0,24,88 },  { 0,26,89 }, { 0,28,90 }, { 0,30,91 }
		};

		public static Dictionary<int, int[]> _ATTENUATOR = new Dictionary<int, int[]>()
		{
			{ 0, new [] {0 , 0} }, { 2, new [] {0 , 2} }, { 4, new [] {0 , 4} }, { 6, new [] {0 , 6} }, { 8, new [] {0 , 8} }, { 10, new [] {0 , 10 } }, 
			{ 12, new [] {0 , 12 } }, { 14, new [] {0 , 14 } }, { 16, new [] {0 , 16} }, { 18, new [] {0 , 18} }, { 20, new [] {0 , 20} },
			{ 22, new [] {0 , 22 } }, { 24, new [] {0 , 24 } }, { 26, new [] {0 , 26} }, { 28, new [] {0 , 28} }, { 30, new [] {0 , 30} },
			{ 32, new [] {2 , 30 } }, { 34, new [] {4 , 30 } }, { 36, new [] {6 , 30 } }, { 38, new [] {8 , 30 } }, { 40, new [] {10 , 30 } },
			{ 42, new [] {12 , 30 } }, { 44, new [] {14 , 30 } }, { 46, new [] {16 , 30 } }, { 48, new [] {18 , 30 } }, { 50, new [] {20 , 30 } },
			{ 52, new [] {22 , 30 } }, { 54, new [] {24 , 30 } }, { 56, new [] {26 , 30 } }, { 58, new [] {28 , 30 } }, { 60, new [] {30 , 30 } },
		};
	}

	public class MultiKeyDictionary<K1, K2, V> : Dictionary<K1, Dictionary<K2, V>>
	{
		public V this[K1 key1, K2 key2]
		{
			get
			{
				if (!ContainsKey(key1) || !this[key1].ContainsKey(key2))
					throw new ArgumentOutOfRangeException();
				return base[key1][key2];
			}
			set
			{
				if (!ContainsKey(key1))
					this[key1] = new Dictionary<K2, V>();
				this[key1][key2] = value;
			}
		}

		public void Add(K1 key1, K2 key2, V value)
		{
			if (!ContainsKey(key1))
				this[key1] = new Dictionary<K2, V>();
			this[key1][key2] = value;
		}

		public bool ContainsKey(K1 key1, K2 key2)
		{
			return base.ContainsKey(key1) && this[key1].ContainsKey(key2);
		}

		public new IEnumerable<V> Values
		{
			get
			{
				return from baseDict in base.Values
					   from baseKey in baseDict.Keys
					   select baseDict[baseKey];
			}
		}
	}
}

