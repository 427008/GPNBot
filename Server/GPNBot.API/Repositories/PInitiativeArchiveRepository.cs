using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GPNBot.API.Extensions;
using GPNBot.API.Models;
using GPNBot.API.Services;


namespace GPNBot.API.Repositories
{
    public class PInitiativeArchiveRepository : Repository<PInitiativeArchive>
    {
        public PInitiativeArchiveRepository(IConnectionService connectionService) : base(connectionService)
        {
        }

        protected override void LogTime(string query, string parameters = null)
        {
        }

        public async Task<(bool, string, string)> GetPIQueryAsync(long queryType, string place, string status, long periodType)
        { 
            var body = "";
            var dictWhere = new Dictionary<string, Condition[]>();

            if(periodType == 1)
            { 
                dictWhere.AddCondition(nameof(PInitiativeArchive.CREATE_DATE), new Condition { Operator = " like ", Value = "%2014%" });
                body = " за 2014 год";
            }
            else if(periodType == 2)
            { 
                dictWhere.AddCondition(nameof(PInitiativeArchive.CREATE_DATE), new Condition { Operator = " like ", Value = "%2015%" });
                body = " за 2015 год";
            }
            else if(periodType == 3)
            { 
                dictWhere.AddCondition(nameof(PInitiativeArchive.CREATE_DATE), new Condition { Operator = " like ", Value = "%2016%" });
                body = " за 2016 год";
            }
            else if(periodType == 4)
            { 
                dictWhere[nameof(PInitiativeArchive.CREATE_DATE)] = new Condition[2] 
                { 
                    new Condition { Operator = ">", Value = "2017-01-01" }, 
                    new Condition { Operator = "<", Value = "2018-01-01" } 
                };
                body = " за 2017 год";
            }

            if(!string.IsNullOrEmpty(place))
            { 
                dictWhere.AddValue(nameof(PInitiativeArchive.PLACE1), place);
                body += $" по '{place}'";
            }


            if(queryType == 4)
            { 
                var (success, error, count, _) = await GetAsync(dictWhere, onlyCount:true).ConfigureAwait(false);
                if (!success) return (false, error, null);

                return (true, null, $"Количество поданных инициатив{body} {count}");
            }
            else if(queryType == 3)
            { 
                dictWhere.AddValue(nameof(PInitiativeArchive.STATUS), status);
                body = $" в статусе '{status}'" + body;

                var (success, error, count, _) = await GetAsync(dictWhere, onlyCount:true).ConfigureAwait(false);
                if (!success) return (false, error, null);

                return (true, null, $"Количество инициатив{body} {count}");
            }
            else if(queryType == 2)
            { 
                var (success, error, count, _) = await GetAsync(dictWhere, onlyCount:true).ConfigureAwait(false);
                if (!success) return (false, error, null);
                var total = count;

                dictWhere.AddValue(nameof(PInitiativeArchive.STATUS), "Рекомендована");
                (success, error, count, _) = await GetAsync(dictWhere, onlyCount:true).ConfigureAwait(false);
                if (!success) return (false, error, null);

                var result = total == 0 ? "неопределено" : $"{(Math.Round((decimal)count/(decimal)total))}";
                return (true, null, $"Соотношение поданных и рекомендованных к реализации{body} {result}");
            }
            else if(queryType == 1)
            { 
            }
            return (false, null, null);
        }

    }
}

/*
Нужно реализовать 4 фиксированных запросов на обслуживание:
Кустов Александр, [27.11.20 21:17]
1. Количество поданных инициатив ОНПЗ (здесь может быть любое другое ДО - МНПЗ, Аэро, ОЗСМ, Марин Бункер и т.д.) за 2020 год (здесь может быть любая другая мера - квартал, месяц, день)

Кустов Александр, [27.11.20 21:18]
2. Количество инициатив, перешедших в реализацию/отклоненных/на модерации - дальше типы фильтров как в вопросе 1 (актив, период)

Кустов Александр, [27.11.20 21:18]
3. Пользователь, подавший наибольшее количество инициатив (фильтры те же, либо без них)

Кустов Александр, [27.11.20 21:18]
4. Соотношение поданных и рекомендованных к реализации инициатив (фильтры те же)
Это кому надо реализовать?
 */