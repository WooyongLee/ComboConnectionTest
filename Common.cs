using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DabinPACT
{
	public class CommonFunctions {
		// Class data
		private string _Dir;

		// For command
		private string _jsonString;
		private String _remoteFT = "Null";
		private String _parameters = "Null";

		public string RemoteFT { get { return _remoteFT; } }
		public string Parameters { get { return _parameters; } }

		private List<ResultSet> _commandName = new List<ResultSet>();

		public int _READ_TIMEOUT = 1000 * 15; //  15 sec

		int MHZ_UNIT = 1000000;
		//int KHZ_UNIT = 1000;
		int UNIT = 1;

		string F0 = "f0";
		string F2 = "f2";

		private RBW_MAP _default_PACTRBW = RBW_MAP.KHz_1000;
		private VBW_MAP _default_PACTVBW = VBW_MAP.KHz_1000;

		// 화면
		Label _lbl;
		RichTextBox _tbx;
		TreeView _trv;

		public CommonFunctions(Label lbl, RichTextBox tbx, TreeView trv) {
			_lbl = lbl;
			_tbx = tbx;
			_trv = trv;

			_Dir = Directory.GetCurrentDirectory();

			ResultSet set1 = new ResultSet("FREQ", MHZ_UNIT, "MHz", F0);
			ResultSet set2 = new ResultSet("MEASURE", UNIT, "dBm", F2);

			_commandName.Add(set1);
			_commandName.Add(set2);

		}

		bool readJsonItems(JsonElement fieldName, ref string nodename, ref string display, ref string remoteft, ref string parameters) {
			if (fieldName.TryGetProperty("NodeName", out JsonElement NodeName)) {
				nodename = NodeName.ToString();
				if (fieldName.TryGetProperty("Display", out JsonElement Display))
					display = Display.ToString();
				if (fieldName.TryGetProperty("RemoteFT", out JsonElement RemoteFT))
					remoteft = RemoteFT.ToString();
				if (fieldName.TryGetProperty("Params", out JsonElement Params))
					parameters = Params.ToString();
				return true;
			}

			return false;
		}

		bool matchJsonItems(JsonElement fieldName, string node) {
			bool find = false;

			if (fieldName.TryGetProperty("NodeName", out JsonElement NodeName)) {
				if (node == NodeName.ToString()) {
					find = true;

					if (fieldName.TryGetProperty("RemoteFT", out JsonElement RemoteFT))
						_remoteFT = RemoteFT.ToString();
					else
						_remoteFT = "null";

					if (fieldName.TryGetProperty("Params", out JsonElement Params))
						_parameters = Params.ToString();
					else
						_parameters = "null";
					}
			}

			return find;
		}

		public bool FindTree(string node) {
			using (JsonDocument document = JsonDocument.Parse(_jsonString)) {
				JsonElement root = document.RootElement;
				JsonElement nodeNameElement = root.GetProperty("1stLevelNodes");
				foreach (JsonElement fieldName in nodeNameElement.EnumerateArray()) {
					if (matchJsonItems(fieldName, node)) {
						return true;
					}

					if (fieldName.TryGetProperty("Sub", out JsonElement Sub)) {
						foreach (JsonElement subfieldName in Sub.EnumerateArray()) {
							if (matchJsonItems(subfieldName, node)) {
								return true;
							}
						}
					} 
				}
			}

			return false;
		}

		public void LoadTree()
		{
			string nodename = "Null";
			string display = "Null";
			string remoteFT = "Null";
			string parameters = "Null";

			_jsonString = File.ReadAllText(_Dir + "\\menunode.json");

			using (JsonDocument document = JsonDocument.Parse(_jsonString)) {
				JsonElement root = document.RootElement;
				JsonElement nodeNameElement = root.GetProperty("1stLevelNodes");
				foreach (JsonElement fieldName in nodeNameElement.EnumerateArray()) {
					if (readJsonItems(fieldName, ref nodename, ref display, ref remoteFT, ref parameters)) {
						//_trv.SelectedNode.Nodes.Add(nodename, display);
						if (!(nodename.Equals("nodConMQTT") || nodename.Equals("nodConPM") || nodename.Equals("nodConSA") || nodename.Equals("nodConSG")))
						{
							_trv.Nodes.Add(nodename, display);
							_trv.SelectedNode = _trv.Nodes[nodename];
						}
						//_trv.SelectedNode = _trv.Nodes[display];
					}

					if (fieldName.TryGetProperty("Sub", out JsonElement Sub)) {
						foreach (JsonElement subfieldName in Sub.EnumerateArray()) {
							if (readJsonItems(subfieldName, ref nodename, ref display, ref remoteFT, ref parameters)) {
								_trv.SelectedNode.Nodes.Add(nodename, display);
							}
						}
					} else {
					}
				}
			}
		}

		public void State(string info) {
			_lbl.Text = info;
		}

		public void Log(string info) {
			_tbx.SelectionColor = System.Drawing.Color.Black;
			_tbx.AppendText("\r\n" + info);
			_tbx.ScrollToCaret();
		}

		public void LogWithDt(string info)
		{
			_tbx.SelectionColor = System.Drawing.Color.Green;
			string currentDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			_tbx.AppendText("\r\n" + currentDt + "\t" + info);
			_tbx.ScrollToCaret();
		}

		public void LogWithResultDt(string info, int result, bool res)
		{
			_tbx.SelectionColor = System.Drawing.Color.Red;
			if (res) _tbx.SelectionColor = System.Drawing.Color.Green;

			string currentDt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			_tbx.AppendText("\r\n" + currentDt + " / " + (result == 0 ? "Fail": "Pass") + " / " + info);
			_tbx.ScrollToCaret();
		}

		public JsonElement GetTreeNodeParams(string node)
		{
			using (JsonDocument document = JsonDocument.Parse(_jsonString))
			{
				JsonElement root = document.RootElement;
				JsonElement nodeNameElement = root.GetProperty("1stLevelNodes");

				foreach (JsonElement fieldName in nodeNameElement.EnumerateArray())
				{
					if (matchJsonItems(fieldName, node))
					{
						return fieldName.Clone();
					}

					if (fieldName.TryGetProperty("Sub", out JsonElement Sub))
					{
						foreach (JsonElement subfieldName in Sub.EnumerateArray())
						{
							if (matchJsonItems(subfieldName, node))
							{
								return subfieldName.Clone();
							}
						}
					}
				}
			}

			return new JsonElement();
		}

		public void DataAddToGridView(string item, DataTable a_table)
		{
			JsonElement result = GetTreeNodeParams(item);
			string resultParams = result.GetProperty("Params").ToString();

			a_table.Clear();

			if (resultParams != "null")
			{
				List<ParamSetJson> valueSet = JsonConvert.DeserializeObject<List<ParamSetJson>>(resultParams);
				foreach (ParamSetJson parameter in valueSet)
				{
					a_table.Rows.Add(parameter.Display, parameter.Value);
				}
			}
		}

		public List<ParamSetJson> GetParamDataSet(string item)
		{
			JsonElement result = GetTreeNodeParams(item);
			string resultParams = result.GetProperty("Params").ToString();

			List<ParamSetJson> valueSet = null;

			if (resultParams != "null")
			{
				 valueSet = JsonConvert.DeserializeObject<List<ParamSetJson>>(resultParams);
			}
			return valueSet;
		}

		public void setCalibrationValue(string item, string value)
		{
			JObject jObject = JsonConvert.DeserializeObject(_jsonString) as JObject;
			// 위치 찾는것에 대해서는 재확인 필요
			jObject["1stLevelNodes"][5]["Sub"][0]["Params"][0]["Value"] = value;

			_jsonString = JsonConvert.SerializeObject(jObject, Formatting.Indented);			
		}

		public string CombineMessage(string a_cmd, string a_msg) {
			string result = "";
			foreach (ResultSet set in _commandName)
			{
				if (a_cmd.IndexOf(set.Name) >= 0)
				{
					result += set.Name + ":" +(string)(Convert.ToDouble(a_msg) / set.Unit).ToString(set.Digit) + " " + set.StringUnit;
					break;
				}
			}

			return result;
		}

		public ServerInfo GetServerInfo(string item)
		{
			JsonElement result = GetTreeNodeParams(item);
			ServerInfo deviceInfo = JsonConvert.DeserializeObject<ServerInfo>(result.ToString());
			return deviceInfo;
		}

		public List<ParamSetJson> GetParams(string item)
		{
			List<ParamSetJson> valueSet = null;
			JsonElement result = GetTreeNodeParams(item);
			string resultParams = result.GetProperty("Params").ToString();
			if (resultParams != "null")
			{
				valueSet = JsonConvert.DeserializeObject<List<ParamSetJson>>(resultParams);
			}
			return valueSet;
		}

		public List<TreeNode> GetCheckedNode(TreeView a_trvMenu)
		{
			List<TreeNode> selectedNodes = new List<TreeNode>();
			foreach (TreeNode node in a_trvMenu.Nodes)
			{
				if (node.Checked) selectedNodes.Add(node);

				if (node.Nodes != null)
				{
					foreach (TreeNode childNode in node.Nodes)
					{
						if (childNode.Checked)
						{
							selectedNodes.Add(childNode);
						}
					}
				}
			}
			return selectedNodes;
		}

		public ParamSetJson GetParamSet(string key, Hashtable a_hashtable)
		{
			if (a_hashtable.ContainsKey(key).Equals(true))
			{
				ParamSetJson set = a_hashtable[key] as ParamSetJson;
				return set;

			}
			return null;
		}

		public RBW_MAP GetRBWValue(ParamSetJson rbwSet)
		{
			RBW_MAP rbwValue;
			try
			{
				rbwValue = (RBW_MAP)Enum.Parse(typeof(RBW_MAP), rbwSet.Unit + "_" + rbwSet.Value);
			}
			catch
			{
				rbwValue = _default_PACTRBW;
			}

			return rbwValue;
		}

		public VBW_MAP GetVBWValue(ParamSetJson vbwSet)
		{
			VBW_MAP vbwValue;
			try
			{
				vbwValue = (VBW_MAP)Enum.Parse(typeof(VBW_MAP), vbwSet.Unit + "_" + vbwSet.Value);
			}
			catch
			{
				vbwValue = _default_PACTVBW;
			}

			return vbwValue;
		}

		public LNA_SET GetLNAEnv()
		{
			LNA_SET lna_Set = LNA_SET.LNA_OFF;
			List<ParamSetJson> lnaSet = GetParams("nodCalibrationPath");
			if (lnaSet != null)
			{
				switch (lnaSet[0].Value)
				{
					case "LNA Off Only":
						lna_Set = LNA_SET.LNA_OFF;
						break;
					case "LNA On Only":
						lna_Set = LNA_SET.LNA_ON;
						break;
					case "LNA Off + On":
						lna_Set = LNA_SET.LNA_ON_OFF;
						break;
					default:
						break;
				}
			}
			return lna_Set;
		}

		public IQImbMessage GetIQImbMessageParse(string a_message)
		{			
			string[] parseMessage = a_message.Split(' ');
			IQImbMessage iQImb = new IQImbMessage(parseMessage[0], parseMessage[1], parseMessage[2], parseMessage[3], parseMessage[4]
				, parseMessage[5], parseMessage[6], parseMessage[7], parseMessage[8], parseMessage[9], parseMessage[10]);
			return iQImb;
		}

		public IQImbalance GetIQImbalanceData(ulong freq, IQImbMessage iQImbPlus, IQImbMessage iQImbMinus)
		{
			int iGain = (iQImbPlus.iGain + iQImbMinus.iGain) / 2;
			int qGain = (iQImbPlus.qGain + iQImbMinus.qGain) / 2;
			int phaseSkew = (iQImbPlus.phaseSkew + iQImbMinus.phaseSkew) / 2;
			int iOffset = (iQImbPlus.iOffset + iQImbMinus.iOffset) / 2;
			int qOffset = (iQImbPlus.qOffset + iQImbMinus.qOffset) / 2;

			double g = (double)(iGain - qGain) / (iGain + qGain);
			double cosVal = Math.Cos(phaseSkew * 0.00001 * Math.PI / 180);
			double sinVal = Math.Sin(phaseSkew * 0.00001 * Math.PI / 180);

			int A = (short)((1 + g) * cosVal * 8192);
			int B = (short)((1 + g) * sinVal * 8192);
			int C = (short)((1 - g) * sinVal * 8192);
			int D = (short)((1 - g) * cosVal * 8192);
			int E = (int)(iOffset * 2048); // 2048 => 8192 / 4, -1 is because FPGA perform '+' operation
			int F = (int)(qOffset * 2048);

			return new IQImbalance(freq, A, B, C, D, E, F);
		}
	}
}