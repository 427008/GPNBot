using System;
using System.Collections.Generic;
using System.Linq;

using GPNBot.API.Models;
using GPNBot.Sec;

namespace GPNBot.API.Repositories
{
    public class UsersRepository
    {
        private static string bearer = "Bearer ";
        private readonly List<User> _users;

        public UsersRepository()
        { 
            _users =  new List<User>
                {
                    new User
                    {
                        Login = "a.chukhonin",
                        Password = "gpnbot",
                        GUID = Guid.Parse("B0638D75-B655-4162-9C4D-F90FD8B0A938"),
                        Fio = "Андрей",
                        EmailPI = "",
                        Email = "gap00t@yandex.ru",
                        Position = "Сторож",
                        EmploymentDate = new DateTime(2020,1,3)
                    },
                    new User
                    {
                        Login = "s.palchun",
                    },
                    new User
                    {
                        Login = "o.vorobchikov",
                    },
                    new User
                    {
                        Login = "t.smirnova",
                    },
                    new User
                    {
                        Login = "andreeva.ei",
                    },
                    new User
                    {
                        Login = "salata.as",
                    },
                    new User
                    {
                        Login = "tisenkov.alp",
                    },
                    new User
                    {
                        Login = "a.kustov",
                    },
                    new User
                    {
                        Login = "ilya",
                    },
                };
        }

        internal object FirstOrDefault(Func<object, object> p)
        {
            throw new NotImplementedException();
        }

        public (bool, Queue<Message>) GetMessages(Guid userGuid, QueueType queueType) 
        {
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return (false, null);
            if(!user.Messages.ContainsKey(queueType)) return (false, null);
            
            return (true, user.Messages[queueType]);
        }

        public bool SetMessages(Guid userGuid, QueueType queueType, Queue<Message> messages) 
        {
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return false;
                
            if(!user.Messages.ContainsKey(queueType)) return false;

            user.Messages[queueType]= messages;
            return true;
        }

        public bool ClearMessages(Guid userGuid, QueueType queueType) 
        {
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return false;
                
            if(!user.Messages.ContainsKey(queueType)) return false;

            user.Messages[queueType] = null;
            user.Messages[queueType] = new Queue<Message>();
            return true;
        }

        public string GetFIO(Guid userGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return "";

            return user.Fio;
        }
        public string GetEmail(Guid userGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return "";

            return user.EmailPI;
        }
        public string GetUserEmail(Guid userGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return "";

            return user.Email;
        }

        public uint GetLastMessageID(Guid userGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return 0;

            return user.LastUserPost;
        }
        public bool SetLastMessageID(Guid userGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return false;
            
            user.LastUserPost++;
            return true;
        }

        public bool IsOlderThenLast(Guid userGuid, uint id)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(userGuid));
            if(user is null) return false;
            
            return id < user.LastUserPost;
        }

        public (bool, string, DateTime?) GetTokenForUser(string login, string password)
        { 
//            var request = new RestRequest(Method.GET);

//            var client = new RestClient("http://gpncb.skillunion.ru/auth");
//            client.Authenticator = new NtlmAuthenticator($"skillunion\\{login}", password);
//            client.Timeout = -1;

//            client.Execute(request);

//            var response = await client.ExecuteAsync(request).ConfigureAwait(false);
//            var ret = JsonSerializer.Deserialize<int>(response.Content);
//            if (ret == 1) ; // OK


            var user = _users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.InvariantCultureIgnoreCase));
            if (user == null || !password.Equals(user.Password))
                return (false, null, null);

            (user.Token, user.Expires) = JWTOptions.Token(user.Login);
            
            return (true, user.Token, user.Expires);
        }

        public (bool, Guid, string, string, DateTime) ValidateToken(string authorization)
        { 
            if(!authorization.StartsWith(bearer)) 
                return (false, default, null, null, default);

            var token = authorization.Substring(bearer.Length);
            if(JWTOptions.Validate(token, out var login, out _) != 0 || string.IsNullOrEmpty(login))
                return (false, default, login, null, default);

            var user = _users.FirstOrDefault(u => u.Login.Equals(login));
            if(user == null)
                return (false, default, login, null, default);

            if(string.IsNullOrEmpty(user.Token)) 
                user.Token = token;
            return (true, user.GUID, user.Fio, user.Position, user.EmploymentDate);
        }

        public (bool, Guid, string, string, DateTime) ValidateUser(string userLogin)
        { 
            var user = _users.FirstOrDefault(u => u.Login.Equals(userLogin));
            if(user == null)
                return (false, default, userLogin, null, default);

            return (true, user.GUID, user.Fio, user.Position, user.EmploymentDate);
        }

        public (bool, User) GetUser(string userLogin)
        { 
            var user = _users.FirstOrDefault(u => u.Login.Equals(userLogin));
            if(user == null) return (false, null);

            var userData = new User 
            { 
                GUID = user.GUID, 
                Fio = user.Fio, 
                Position = user.Position, 
                EmploymentDate = user.EmploymentDate,
                Login = user.Login
            };
            
            return (true, userData);
        }
        public (bool, User) GetUser(Guid GUID)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(GUID));
            if(user == null) return (false, null);

            var userData = new User 
            { 
                GUID = user.GUID, 
                Fio = user.Fio, 
                Position = user.Position, 
                EmploymentDate = user.EmploymentDate,
                Login = user.Login
            };
            
            return (true, userData);
        }

        public bool IsItLastToken(string userLogin, string token, DateTime validTo)
        { 
            var user = _users.FirstOrDefault(u => u.Login.Equals(userLogin));
            if(user == null) return false;

            if(string.IsNullOrEmpty(user.Token))
            { 
                user.Token = token;
                user.Expires = validTo;
            }

            return user.Expires.Value.Year == validTo.Year && user.Expires.Value.Month == validTo.Month  && user.Expires.Value.Day == validTo.Day && 
                   user.Expires.Value.Hour == validTo.Hour && user.Expires.Value.Minute == validTo.Minute && user.Expires.Value.Second == validTo.Second;
        }
        
        public bool Logout(Guid UserGuid)
        { 
            var user = _users.FirstOrDefault(u => u.GUID.Equals(UserGuid));
            if(user is null)
                return false;
    
            user.Token = null;
            return true;
        }
    }
}
