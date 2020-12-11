using System;

using GPNBot.API.Attributes;
using GPNBot.API.Extensions;


namespace GPNBot.API.Models
{
    public class Initiative
    {
        
        [Persist]
        [Key]
        public ulong Iid { get; set; }
        [Persist]
        public string Status { get; set; }
        
        [Persist]
        public DateTime Created  { get; set; }
        [Persist]
        public string Author { get; set; }
        [Persist]
        public string Organization { get; set; }
        [Persist]
        public string Problem { get; set; }
        [Persist]
        public string Solution { get; set; }
        
        [Persist]
        public int? CommentsCount  { get; set; }
        
        [Persist]
        public string LastCommentDate { get; set; }

        [Persist]
        public string LastCommentText  { get; set; }

        public static string Select => $"SELECT {typeof(Initiative).GetSelectableFieldListStr()} FROM ?.initiatives";
        public static string SelectCount => "SELECT Count(1) FROM ?.initiatives";
        public static string Insert => "INSERT INTO ?.initiatives";
    }
}
