using System.ComponentModel.DataAnnotations;

namespace GPNBot.API.Models
{
    public class UserLogin
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
