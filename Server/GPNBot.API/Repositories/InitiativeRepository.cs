using GPNBot.API.Services;
using GPNBot.API.Models;


namespace GPNBot.API.Repositories
{
    public class InitiativeRepository : Repository<Initiative>
    {
        public InitiativeRepository(IConnectionService connectionService) : base(connectionService)
        {
        }
        protected override void LogTime(string query, string parameters = null)
        {
        }
    }
}
