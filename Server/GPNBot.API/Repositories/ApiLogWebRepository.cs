using GPNBot.API.Services;
using GPNBot.API.Models;


namespace GPNBot.API.Repositories
{
    public class ApiLogWebRepository : Repository<ApiLogWeb>
    {
        public ApiLogWebRepository(IConnectionService connectionService) : base(connectionService)
        {
        }
        protected override void LogTime(string query, string parameters = null)
        {
        }
    }
}
