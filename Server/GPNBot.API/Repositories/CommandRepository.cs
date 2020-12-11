using System;
using System.Collections.Generic;
using GPNBot.API.Commands;
using GPNBot.API.Models;

namespace GPNBot.API.Repositories
{
    public static class CommandRepository
    {
        private static List<CommandItem> _commandItems = new List<CommandItem>
        {
            new CommandItem (0),
            new CommandItem (1, "Правильно"), 
            new CommandItem (2, "Ошибся"), 

            new CommandItem (4, "Показать еще...") { Template = "Показать еще..." },
            new CommandItem (5), // email
            new CommandItem (6), // template
            new CommandItem (7), // action
            new CommandItem (8), // redirect
        };

        public static CommandItem GetCommandItem(long code)
        {
            if(code == 2) code = 1;
            return _commandItems.Find(c => c.Code == code) ?? _commandItems[0];
        }
        public static (long, Message) GetNextMessage(Message source)
        {
            var code = source.Command;
            if(code == 2) code = 1;
            var commandItem = _commandItems.Find(c => c.Code == code) ?? _commandItems[0];

            return (commandItem.Code,  new Message
            {
                GUID = Guid.NewGuid(),
                SourceId = source.SourceId ?? source.ID,

                UserGuid = source.UserGuid,
                ServiceGuid = source.ServiceGuid,
                Date = source.Date,
                Body = source.Body,
                CommandText = source.CommandText,
                Iid = source.Iid,

                Command = commandItem.Code,

                IsMy = false,
                Commands = new List<CommandItem>()
            });
        }
        public static List<CommandItem> GetYesNo(ulong? sourceId)
        { 
            return new List<CommandItem> 
            { 
                new CommandItem (1, "Правильно") { SourceId = sourceId }, 
                new CommandItem (2, "Ошибся") { SourceId = sourceId, CommandText = Guid.Empty.ToString() } 
            };
        }
        public static List<CommandItem> GetNext()
        { 
            return new List<CommandItem> { new CommandItem (4, "Показать еще...") { Template = "Показать еще..." } };
        }
    }
}