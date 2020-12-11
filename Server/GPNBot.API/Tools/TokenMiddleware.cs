using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Primitives;

using GPNBot.Sec;
using GPNBot.API.Repositories;
using GPNBot.API.Models;

namespace GPNBot.API.Tools
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UsersRepository _users;
        private static string bearer = "Bearer ";

        public TokenMiddleware(RequestDelegate next, UsersRepository users)
        {
            _next = next;
            _users = users;
        }
 
        public async Task InvokeAsync(HttpContext context)
        {
            var authorization = context.Request.Headers["Authorization"];
            if(!string.IsNullOrEmpty(authorization.ToString()))
            { 
                var token = authorization.ToString().Substring(bearer.Length);
                var result = JWTOptions.Validate(token, out var login, out var validTo);

                if(result != 0)
                {
                    if(result == 2)
                        context.Response.StatusCode = 403;
                    else
                        context.Response.StatusCode = 404;

                    await context.Response.WriteAsync("Token is invalid");
                }
                else
                {
                    if(_users.IsItLastToken(login, token, validTo))
                    {
                        context.Request.Headers.Add("UserLogin", new StringValues(login));
                        await _next.Invoke(context);
                    }
                    else
                    { 
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Token is invalid (new connection opened somewhere)");
                    }
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
