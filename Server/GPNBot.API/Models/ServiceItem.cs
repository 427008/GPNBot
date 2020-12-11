using System;


namespace GPNBot.API.Models
{
    public class ServiceItem
    {
        public uint Id { get; set; }
        public Guid GUID  { get; set; }
        public string Title  { get; set; }
        public string Image  { get; set; }
        public string Href { get; set; }
    }
}
