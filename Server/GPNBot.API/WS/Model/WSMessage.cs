using GPNBot.API.Models;

namespace GPNBot.API.WS.Model
{
    public class WSMessage<T>
    {
        public WSMessage(T t, string Type, string Cmd) 
        { 
            type = Type;
            cmd = Cmd;
            dst = typeof(T).Name;
            data = new T[1] { t };
        }

        public WSMessage(T[] tArray, string Type = "event")
        { 
            cmd = "newrec";
            data = tArray;
        }


        public static WSMessage<T> EventNew(T t)
        { 
            return new WSMessage<T>(t, "event", "newrec");
        }

        public string type { get; set; }
        public string dst { get; set; }
        public string cmd { get; set; }
        public T[] data { get; set; }

/*
                     {
                        type: 'event',
                        dst: 'message',
                        cmd: 'newrec',
                        data: [
                            {},
                        ]
                    }
 */
    }
}
