using System;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;


namespace DabinPACT {
	public class CommonFunctions { }
		// Set up the delegate for the logging function
		public delegate void UpdateLogCallback(LogInfo log);

		TextBox tbxLogging;

		public CommonFunctions(TextBox TBXLogging)
		{
			tbxLogging = TBXLogging;
		}

		public class LogInfo
		{
			public string info;
			public string extraInfo;
		}

		public void LogText(LogInfo log)
		{
			// show the message to the user
			tbxLogging.AppendText(log.info);

			// automatically scroll down to bottom of textbox
			tbxLogging.SelectionStart = tbxLogging.Text.Length;
			tbxLogging.ScrollToCaret();

			if (GV_.bLogToFile)
			{
				// Append to the logfile
				if (File.Exists(GV_.sLogFileName))
				{
					StreamWriter Log = new StreamWriter(GV_.sLogFileName, true);
					Log.Write(log.info);

					// If available write exception information in the logfile
					if (log.extraInfo != "")
					{
						Log.Write(log.extraInfo);
					}
					Log.Close();
				}
			}
		}

		public void Log(string info)
		{
			LogInfo log = new LogInfo();
			log.info = info + "\r\n";
			log.extraInfo = "";

			tbxLogging.Invoke(new UpdateLogCallback(LogText), new object[] { log });
		}

		public void Log(int info)
		{
			LogInfo log = new LogInfo();
			log.info = "Val: " + info.ToString() + "\r\n";
			log.extraInfo = "";

			tbxLogging.Invoke(new UpdateLogCallback(LogText), new object[] { log });
		}

		public void Log(string title, int info)
		{
			LogInfo log = new LogInfo();
			log.info = title + " = " + info.ToString() + "\r\n";
			log.extraInfo = "";

			tbxLogging.Invoke(new UpdateLogCallback(LogText), new object[] { log });
		}

		public void Log(string info, string extraInfo)
		{
			LogInfo log = new LogInfo();
			log.info = info + "\r\n";
			log.extraInfo = extraInfo + "\r\n";
			tbxLogging.Invoke(new UpdateLogCallback(LogText), new object[] { log });
		}

		public bool MakeSessionDir()
		{
			bool _result = false;

			try
			{
				GV_.sSessionCur = GV_.sSessionRoot + "\\" + GV_.sSessionName + "\\";
				GV_.sSessionCurNum = GV_.sSessionCur + GV_.nSessionNumber.ToString() + "\\";
				if (!Directory.Exists(GV_.sSessionCurNum))
				{
					Directory.CreateDirectory(GV_.sSessionCurNum);
					_result = true;
				}
				else
					MessageBox.Show(GV_.sSessionCurNum, "Directory exist. Aborting.");
			}
			catch (Exception ex)
			{
				Log(ex.ToString());
			}
			return _result;
		}
	}

	public class ListBoxItemType
	{
		public string LONG { get; set; }
		public string SHORT { get; set; }
	}

	// Simple한 것들은 static으로 선언하자.
	public static class SCF
	{

		public static int FindOldDevice(string deviceID)
		{
			int _i, _result = CS_.NotFound;

			for (_result = CS_.NotFound, _i = 0; _i < GV_.lsTT.Count; _i++)
			{
				if (GV_.lsTT[_i].deviceID == deviceID)
				{
					_result = _i;
					break;
				}
			}
			return _result;
		}

		public static string GetSimpleDeviceID(string deviceID)
		{
			int _pos;
			_pos = deviceID.LastIndexOf('\\');
			return deviceID.Substring(_pos + 1);
		}


		public static void SeparateConfigure(string MIXED, out string DEVICEID, out bool ASSIGNED, out int COL, out int ROW)
		{
			string _temp, _maruta;
			int _pos;
			// ASSSIGN + COL + ROW + DeviceID를 역순으로 뜯어낸다.
			_maruta = MIXED;
			_pos = _maruta.LastIndexOf(CS_.Delemeter);
			DEVICEID = _maruta.Substring(_pos + 1);

			_maruta = _maruta.Remove(_pos);
			_pos = _maruta.LastIndexOf(CS_.Delemeter);
			_temp = _maruta.Substring(_pos + 1);
			ROW = int.Parse(_temp);

			_maruta = _maruta.Remove(_pos);
			_pos = _maruta.LastIndexOf(CS_.Delemeter);
			_temp = _maruta.Substring(_pos + 1);
			COL = int.Parse(_temp);

			_maruta = _maruta.Remove(_pos);
			ASSIGNED = Convert.ToBoolean(_maruta);
		}

		public static Point? GetRowColIndex(TableLayoutPanel tlp, Point point)
		{
			if (point.X > tlp.Width || point.Y > tlp.Height)
				return null;

			int w = tlp.Width;
			int h = tlp.Height;
			int[] widths = tlp.GetColumnWidths();

			int i;
			for (i = widths.Length - 1; i >= 0 && point.X < w; i--)
				w -= widths[i];
			int col = i + 1;

			int[] heights = tlp.GetRowHeights();
			for (i = heights.Length - 1; i >= 0 && point.Y < h; i--)
				h -= heights[i];

			int row = i + 1;

			return new Point(col, row);
		}

		public static void AddList(ListBox LBL, string DEVICEID)
		{
			string _short = SCF.GetSimpleDeviceID(DEVICEID);
			LBL.Items.Add(new ListBoxItemType { LONG = DEVICEID, SHORT = _short });
		}

		public static void RemoveList(ListBox LBL, string DEVICEID)
		{
			string _short = SCF.GetSimpleDeviceID(DEVICEID);
			int _index;
			_index = LBL.FindString(_short);
			if (_index >= 0)
				LBL.Items.RemoveAt(_index);
		}


		public static bool IsOccupied(int COL, int ROW)
		{
			bool _result = false;

			for (int i = 0; i < GV_.lsTT.Count; i++)
			{
				if ((GV_.lsTT[i].nCol == COL) && (GV_.lsTT[i].nRow == ROW))
				{
					_result = true;
					break;
				}
			}
			return _result;
		}

		public static bool IsVacant(int COL, int ROW)
		{
			bool _result = false;

			if ((GV_.nCol < COL) || (GV_.nRow < ROW))
				_result = true;

			return _result;
		}

		public static string DeviceIDatColRow(int COL, int ROW)
		{
			string _result = "";

			for (int i = 0; i < GV_.lsTT.Count; i++)
			{
				if ((GV_.lsTT[i].nCol == COL) && (GV_.lsTT[i].nRow == ROW))
				{
					_result = GV_.lsTT[i].deviceID;
					break;
				}
			}
			return _result;
		}

		public static void OrderThread(enum_Thread_Switch EC)
		{
			for (int i = 0; i < GV_.lsTT.Count; i++)
			{
				GV_.lsTT[i].etsCommand = EC;
			}
		}

	}
}