using System.Text.Json.Serialization;

namespace DoQuangThang_SE1885_A01_FE.Models.Accounts
{
    public class ODataResponse<T>
    {
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }

        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new();
    }

}
