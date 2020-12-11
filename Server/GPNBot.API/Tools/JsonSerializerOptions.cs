using System.Text.Json;

namespace GPNBot.API.Tools
{
    public static class GPNJsonSerializer
    {
        public static JsonSerializerOptions Option() => new JsonSerializerOptions 
        { 
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
        };
    }
}
