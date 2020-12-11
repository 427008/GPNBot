using System;
using System.Collections.Generic;

using GPNBot.API.Attributes;
using GPNBot.API.Commands;
using GPNBot.API.Extensions;


namespace GPNBot.API.Models
{
    public class Message
    {

        [Key]
        [Persist]
        [ReadOnly]
        public ulong? ID  { get; set; }
        
        public Guid GUID { get; set; }
        
        [Persist]
        public Guid UserGuid { get; set; }
        
        [Persist]
        public Guid? ServiceGuid { get; set; }
        public long? QuestionCategory { get; set; }

        public string Body { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public long Command { get; set; }
        public string CommandText { get; set; }
        public string NextCommand { get; set; }
        public string Arguments { get; set; }
        public long? Style { get; set; }
        public ulong? SourceId { get; set; }
        public bool? CanClick { get; set; }


        [Persist]
        public bool IsMy { get; set; }
        public bool IsSending { get; set; }

        [Persist]
        public ulong Iid { get; set; }

        public bool? IsFile { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
        public string FileType { get; set; }
        public DateTime? LastModified { get; set; }
        public string Src { get; set; }

        public List<CommandItem> Commands { get; set; }

        [Persist]
        [ReadOnly]
        public DateTime? Created { get; set; }

        [Persist]
        [JsonData]
        public string JsonData { get; set; }
        public bool? ValidAnswer { get; set; }
        public bool? CommandValid { get; set; }

        public static string Select => $"SELECT {typeof(Message).GetSelectableFieldListStr()} FROM ?.messages";
        public static string SelectCount => "SELECT Count(1) FROM ?.messages";
        public static string Insert => "INSERT INTO ?.messages";
        public static string PostSelect => $"SELECT {typeof(Message).GetReadOnlyFieldListStr()} FROM ?.messages WHERE id=LAST_INSERT_ID()";

        public static Message Answer(Message source, string body, long ticks = 0)
        { 
            return new Message
            {
                GUID = Guid.NewGuid(),

                UserGuid = source.UserGuid,
                Iid = source.Iid,
                ServiceGuid = source.ServiceGuid,

                Command = 0,
                IsFile = false,
                Commands = new List<CommandItem>(),
                
                Date = source.Date.AddTicks(ticks),
                Body = body,
            };
        }

    }
}
