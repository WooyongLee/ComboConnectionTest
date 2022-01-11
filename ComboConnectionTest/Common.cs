

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace ComboConnectionTest
{
    public static class CommonFunctions
    {
        public static readonly int READ_TIMEOUT = 1000 * 15; //  15 sec

        public static readonly string BLANK = " ";
    }

    public class JsonManager
    {
        // To Do :: Config 파일로 경로와 파일 이름 뺴기
        // Json 파일 경로
        private string path = "";

        // JSON 파일 내 모든 string을 갖는 버퍼
        public string JsonFullString = "";

        public JsonManager()
        {
            this.LoadJsonFile();
        }

        public void LoadJsonFile()
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), "menunode.json");
            this.JsonFullString = File.ReadAllText(path);
        }

        // 테이블에서 키에 일치하는 파라미터 데이터를 반환하는 함수
        public static ParamSetJson GetParamSet(string key, Hashtable hashtable)
        {
            if (hashtable.ContainsKey(key).Equals(true))
            {
                ParamSetJson set = hashtable[key] as ParamSetJson;
                return set;
            }
            return null;
        }

        // RBW Enum 값
        public static RBW_MAP GetRBWValue(ParamSetJson rbwSet)
        {
            RBW_MAP rbwMap = RBW_MAP.KHz_1000;
            try
            {
                rbwMap = (RBW_MAP)Enum.Parse(typeof(RBW_MAP), rbwSet.Unit + "_" + rbwSet.Value);
            }
            catch { }

            return rbwMap;
        }

        // VBW Enum 값
        public static VBW_MAP GetVBWValue(ParamSetJson vbwSet)
        {
            VBW_MAP vbwMap = VBW_MAP.KHz_1000;
            try
            {
                vbwMap = (VBW_MAP)Enum.Parse(typeof(VBW_MAP), vbwSet.Unit + "_" + vbwSet.Value);
            }
            catch { }

            return vbwMap;
        }


        // Json 파라미터 세트를 해당 item을 통해 Json 텍스트에서 찾아서 반환함
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

        // 해당 노드의 필드 엘리먼트 전체를 반환함
        public JsonElement GetTreeNodeParams(string node)
        {
            using (JsonDocument document = JsonDocument.Parse(JsonFullString))
            {
                // ?? :: 왜 root -> 1stLevelNode로 들어가서 필드이름을 찾는지
                // EnumerateArray에서 문제가 생겨 루트에서 한 레벨 더 들어가는듯
                JsonElement root = document.RootElement;
                JsonElement nodeNameElement = root.GetProperty("1stLevelNodes");

                var nodeNameArray = nodeNameElement.EnumerateArray();

                foreach (var fieldName in nodeNameArray)
                {
                    if (matchJsonItems(fieldName, node))
                    {
                        return fieldName.Clone();
                    }

                    // sub KeyWord가 나오는 경우에 Sub의 하위 그룹을 처리하는 부분
                    if (fieldName.TryGetProperty("Sub", out JsonElement Sub))
                    {
                        foreach (var subfieldName in Sub.EnumerateArray())
                        {
                            if (matchJsonItems(subfieldName, node))
                            {
                                return subfieldName.Clone();
                            }
                        }
                    }
                } // end foreach ( var fieldName in nodeNameArray)
            } // end using

            return new JsonElement();
        }

        private bool matchJsonItems(JsonElement fieldName, string node)
        {
            bool bFind = false;

            // NodeName 일치 여부 확인
            if (fieldName.TryGetProperty("NodeName", out JsonElement NodeName))
            {
                if (node == NodeName.ToString())
                {
                    bFind = true;
                }
            }

            return bFind;
        }
    }
}
