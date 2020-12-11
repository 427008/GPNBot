using GPNBot.API.Attributes;
using GPNBot.API.Extensions;

namespace GPNBot.API.Models
{
    public class AILog
    {
        [Key]
        [Persist]
        [ReadOnly]
        public ulong ID  { get; set; }

        [Persist]
        public ulong MessageID  { get; set; }
        [Persist]
        public string UserMeasuring { get; set; }

        public static string Select => $"SELECT {typeof(AILog).GetSelectableFieldListStr()} FROM ?.ailog";
        public static string SelectCount => "SELECT Count(1) FROM ?.ailog";
        public static string Insert => "INSERT INTO ?.ailog";
    }
}
