using System;


namespace GPNBot.API.Models
{
    public class MessageParams
    {
        public string Hello { get; set; }
        public string NotFound { get; set; }
        public string Found { get; set; }
        public string CommandError { get; set; }
        public string AIError { get; set; }
        public int HoursToNextHello { get; set; }
        public SmtpParams Smtp { get; set; }
    }

    public class Dialog
    { 
        public Message[] Templates { get; set; }
    }
}
