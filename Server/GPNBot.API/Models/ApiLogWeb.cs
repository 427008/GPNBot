using System;

using GPNBot.API.Attributes;


namespace GPNBot.API.Models
{
    public class ApiLogWeb
    {
        [Key]
        [Persist]
        [ReadOnly]
        public ulong? ID  { get; set; }
        
        [Persist]
        public string Data { get; set; }
        [Persist]
        [ReadOnly]
        public DateTime? Created { get; set; }
        [Persist]
        public string Level { get; set; }
        [Persist]
        public string Address { get; set; }

        public static string Insert => "INSERT INTO ?.web";

    }
}
