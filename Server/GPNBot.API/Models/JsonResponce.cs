namespace GPNBot.API.Models
{
    public class JsonResponce
    {
        public bool Success { get; set; } = true;
        public long Total { get; set; } = 1;
        public string Message { get; set; }
        public object Data { get; set; }

    }
}
