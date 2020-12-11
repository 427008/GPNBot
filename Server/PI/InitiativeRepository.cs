using GPNBot.API.Repositories;
using GPNBot.API.Services;


namespace PIImport
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
