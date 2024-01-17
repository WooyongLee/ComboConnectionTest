using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DabinPACT
{
    public class FileUtil
    {
		private int _MAX_LENGTH = 100000;
		public float _LNA_ON_GOFFSET = 00.00F;
		public float _LNA_OFF_GOFFSET = 00.00F;

		public string _GLOBAL_OFFSET_FNAME = "SA_Common.cal";

		public string _ATTEN_OFFSET_FNAME = "SA_AttenOffset.cal";
		public string _ATTEN_OFFSET_LNA_ON_FNAME = "SA_LNA_AttenOffset.cal";

		public string _IQ_IMBALANCE_FNAME = "SA_IQ_Imb.cal";
		public string _IQ_IMBALANCE_LNA_ON_FNAME = "SA_LNA_IQ_Imb.cal";

		public string _POWER_OFFSET_FNAME = "SA_PwrOffset.cal";
		public string _POWER_OFFSET_LNA_ON_FNAME = "SA_LNA_PwrOffset.cal";


		public Hashtable _CABLE_OFFSET = new Hashtable();

		public async Task ProcessWriteAsync(string fileName, Hashtable a_hashTable)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/log/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/log/" + fileName + DateTime.Now.ToString("_yyyyMMdd_HHmm");

			await WriteTextAsync(filePath, a_hashTable);
		}

		private async Task WriteTextAsync(string filePath, Hashtable a_hashTable)
		{
			SortedList s = new SortedList(a_hashTable);

			foreach (DictionaryEntry d in s)
			{
				string text = d.Key + "," + d.Value + "\r\n";
				byte[] encodedText = Encoding.UTF8.GetBytes(text);

				//using (FileStream sourceStream = new FileStream(filePath,
				//	FileMode.Create, FileAccess.Write, FileShare.None,
				//	bufferSize: 4096, useAsync: true))
				using (FileStream sourceStream = new FileStream(filePath,
				FileMode.Append, FileAccess.Write, FileShare.None,
				bufferSize: 4096, useAsync: true))
				{
					await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
				};
			}
		}

		public async Task ProcessReadAsync(string fileName)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			string filePath = fileDir + "/output/" + fileName;

			if (File.Exists(filePath) == false)
			{
				Debug.WriteLine("file not found: " + filePath);
			}
			else
			{
				try
				{
					string text = await ReadTextAsync(filePath);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

		private async Task<string> ReadTextAsync(string filePath)
		{
			using (FileStream sourceStream = new FileStream(filePath,
				FileMode.Open, FileAccess.Read, FileShare.Read,
				bufferSize: 4096, useAsync: true))
			{
				StringBuilder sb = new StringBuilder();

				byte[] buffer = new byte[_MAX_LENGTH];
				int numRead;
				while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
				{
					string text = Encoding.Unicode.GetString(buffer, 0, numRead);
					sb.Append(text);
				}

				return sb.ToString();
			}
		}

		public async Task WriteBinaryAsync(string fileName)
		{
			await Task.Delay(10);
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;
			//using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
			//{
			//	writer.Write(00.00F);
			//	writer.Write('\n');
			//	writer.Write(1.250F);
			//	writer.Write('\n');
			//	writer.Close();
			//}
			List<float> globalOffset = new List<float>();
			globalOffset.Add(_LNA_OFF_GOFFSET);
			globalOffset.Add(_LNA_ON_GOFFSET);

			using (FileStream stream = File.OpenWrite(filePath))
			{
				foreach (float item in globalOffset)
				{
					await stream.WriteAsync(BitConverter.GetBytes(item), 0, sizeof(float));
				}
			}
		}

		public async Task WriteBinPowerOffsetAsync(string fileName, List<PowerOffSet> a_list)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream stream = File.OpenWrite(filePath))
			{
				foreach (PowerOffSet item in a_list)
				{
					await stream.WriteAsync(BitConverter.GetBytes(item.freqValue), 0, sizeof(ulong));
					await stream.WriteAsync(BitConverter.GetBytes(item.tempValue), 0, sizeof(float));
					await stream.WriteAsync(BitConverter.GetBytes(item.powerAccuracy), 0, sizeof(int));
				}
			}
		}

		public async Task WriteBinIQImbalanceAsync(string fileName, List<IQImbalance> a_list)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream stream = File.OpenWrite(filePath))
			{
				foreach (IQImbalance item in a_list)
				{
					await stream.WriteAsync(BitConverter.GetBytes(item.freqValue), 0, sizeof(ulong));
					await stream.WriteAsync(BitConverter.GetBytes(item.A), 0, sizeof(int));
					await stream.WriteAsync(BitConverter.GetBytes(item.B), 0, sizeof(int));
					await stream.WriteAsync(BitConverter.GetBytes(item.C), 0, sizeof(int));
					await stream.WriteAsync(BitConverter.GetBytes(item.D), 0, sizeof(int));
					await stream.WriteAsync(BitConverter.GetBytes(item.E), 0, sizeof(int));
					await stream.WriteAsync(BitConverter.GetBytes(item.F), 0, sizeof(int));
				}
			}
		}

		public async Task WriteBinAttenOffsetAsync(string fileName, List<AttenuatorOffSet> a_list)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream stream = File.OpenWrite(filePath))
			{
				foreach (AttenuatorOffSet item in a_list)
				{
					await stream.WriteAsync(BitConverter.GetBytes(item.freqValue), 0, sizeof(ulong));
					await stream.WriteAsync(BitConverter.GetBytes(item.attenValue), 0, sizeof(uint));
					await stream.WriteAsync(BitConverter.GetBytes(item.attenAccuracy), 0, sizeof(int));
				}
			}
		}

		public void ReadBinPowerOffsetAsync(string fileName)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream sourceStream = new FileStream(filePath,
			FileMode.Open, FileAccess.Read, FileShare.Read,
			bufferSize: 4096, useAsync: true))
			{
				using (BinaryReader reader = new BinaryReader(sourceStream))
				{
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						ulong read1 = reader.ReadUInt64();
						float read2 = reader.ReadSingle();
						int read3 = reader.ReadInt32();
						Console.WriteLine("=========> {0}, {1}, {2}", read1, read2, read3);
					}
				}
			}
		}

		public void ReadBinIQImbalanceAsync(string fileName)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream sourceStream = new FileStream(filePath,
			FileMode.Open, FileAccess.Read, FileShare.Read,
			bufferSize: 4096, useAsync: true))
			{
				using (BinaryReader reader = new BinaryReader(sourceStream))
				{
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						ulong read1 = reader.ReadUInt64();
						int A = reader.ReadInt32();
						int B = reader.ReadInt32();
						int C = reader.ReadInt32();
						int D = reader.ReadInt32();
						int E = reader.ReadInt32();
						int F = reader.ReadInt32();
						Console.WriteLine("=========> {0}, {1}, {2}, {3}, {4}, {5}, {6}", read1, A, B, 
							C, D, E, F);
					}
				}
			}
		}

		public void ReadBinAttenOffsetAsync(string fileName)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			DirectoryInfo di = new DirectoryInfo(fileDir + "/output/");
			if (di.Exists == false) di.Create();
			string filePath = fileDir + "/output/" + fileName;

			using (FileStream sourceStream = new FileStream(filePath,
			FileMode.Open, FileAccess.Read, FileShare.Read,
			bufferSize: 4096, useAsync: true))
			{
				using (BinaryReader reader = new BinaryReader(sourceStream))
				{
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						ulong read1 = reader.ReadUInt64();
						uint read2 = reader.ReadUInt32();
						int read3 = reader.ReadInt32();
						Console.WriteLine("=========> {0}, {1}, {2}", read1, read2, read3);
					}
				}
			}
		}

		public async Task ReadBinaryAsync(string fileName)
		{
			await Task.Delay(10);
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			string filePath = fileDir + "/output/" + fileName;
			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				try
				{
					float read1 = reader.ReadSingle();
					float read2 = reader.ReadSingle();
				}
				catch (Exception ex)
				{
					Console.WriteLine("====>{0}", ex.ToString());
				}
				reader.Close();
			}
		}

		private async Task WriteMD5CheckSumAsync(string fileDir, string fileName, string checkSum)
		{
			string filePath = fileDir + fileName + ".md5";
			string text = checkSum + "  " + fileName;
			byte[] encodedText = Encoding.UTF8.GetBytes(text);

			using (FileStream sourceStream = new FileStream(filePath,
				FileMode.Create, FileAccess.Write, FileShare.None,
				bufferSize: 4096, useAsync: true))
			{
				await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
			};
		}

		public async Task<string> GetMD5CheckSum(string fileDir, string fileName)
		{
			string output;
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(fileDir + fileName))
				{
					byte[] checksum = md5.ComputeHash(stream);
					output = BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
				}
			}
			await WriteMD5CheckSumAsync(fileDir, fileName, output);
			return output;
		}

		public async Task LoadCableOffset(string fileName)
		{
			string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory());
			string filePath = fileDir + "/output/" + fileName;

			if (File.Exists(filePath) == false)
			{
				Debug.WriteLine("file not found: " + filePath);
			}
			else
			{
				try
				{
					if (_CABLE_OFFSET.Count > 0)
					{
						_CABLE_OFFSET.Clear();
					}
					string[] readCableOffset = File.ReadAllLines(filePath, Encoding.UTF8);
					foreach (string cableOffset in readCableOffset)
					{
						string[] splitData = cableOffset.Split(',');
						_CABLE_OFFSET.Add(float.Parse(splitData[0]), float.Parse(splitData[1]));
					}
					await Task.Delay(10);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
		}

		public float FindCurFreqCableOffset(float curFreq)
		{
			float result = 0;

			if (_CABLE_OFFSET.ContainsKey(curFreq).Equals(true))
			{
				result = (float)_CABLE_OFFSET[curFreq];
			} else
			{
				result = GetNearestValue(curFreq, _CABLE_OFFSET);
			}

			return result;
		}

		private static float GetNearestValue(float curFreq, Hashtable cableOffsetTable)
		{
			float min = 10000f;
			float near = 0f;

			float[] keyArray = new float[cableOffsetTable.Count];
			cableOffsetTable.Keys.CopyTo(keyArray, 0);

			for (int i = 0; i < keyArray.Length; i++)
			{
				float tmp = keyArray[i] - curFreq;

				if (Math.Abs(min) > Math.Abs(tmp))
				{
					min = tmp;
					near = (float)cableOffsetTable[keyArray[i]];
				}
			}

			return near;
		}
	}
}
