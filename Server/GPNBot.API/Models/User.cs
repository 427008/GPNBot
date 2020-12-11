using System;
using System.Collections.Generic;

namespace GPNBot.API.Models
{
    public class User
    { 
        public string Login { get; set; }
        public string Password { get; set; }
        public Guid GUID { get; set; }
        public string Fio { get; set; }
        public string EmailPI  { get; set; }
        public string Email  { get; set; }
        public string Position  { get; set; }
        public DateTime EmploymentDate  { get; set; }
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
        
        public volatile uint LastUserPost;

        public volatile uint CurrentServiceID;

        public Dictionary<QueueType, Queue<Message>> Messages { get; set; } = new Dictionary<QueueType, Queue<Message>>() 
        { 
            { QueueType.service, new Queue<Message>() }, 
            { QueueType.category, new Queue<Message>() }, 
            { QueueType.information, new Queue<Message>() }, 
        };
    }
    public enum QueueType { service, category, information };
}
