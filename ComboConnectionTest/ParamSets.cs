using Newtonsoft.Json;

namespace ComboConnectionTest
{
	// JSON 파라미터 셋트
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
}
