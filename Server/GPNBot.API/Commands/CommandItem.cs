namespace GPNBot.API.Commands
{
    public class CommandItem
    {
        public CommandItem()
        { }

        public CommandItem(long code)
        {
            Code = code;
        }

        public CommandItem(long code, string title)
        {
            Code = code;
            Title = title;
            Template = title;
        }

        public long Code { get; set; }
        public string CommandText { get; set; }
        public string Title { get; set; }
        public string Template { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string Action { get; set; }

        public string ValidAnswer { get; set; }
        public ulong? SourceId { get; set; }
    }
}
