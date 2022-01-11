
namespace ComboConnectionTest
{
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
		SWEPT_SA = 0x00, // Cal Mode
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
		LNA_ON_OFF = 2
	}
}
