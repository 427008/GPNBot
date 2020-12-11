using System;
using System.Linq;


namespace GPNBot.API.Services
{
    public static class PresetQuestionFilter
    {
        public static (bool, long, ulong) ParseQuestion(string question)
        { 
            var queryType = 0;
            var words = question.Split(' ').ToArray();

            if(ulong.TryParse(question ?? "", out var iid))
            { 
                return (true, 5, iid);
            }
            var tmpNumberTest = (question ?? "").Replace("инициатива", "").Trim();
            if(ulong.TryParse(tmpNumberTest, out iid))
            { 
                return (true, 5, iid);
            }

            var keyWord = "инициатив";
            var queryTypes = new string[6] { "количество", "пользовател", "соотношение", "статусе", "кол-во", "сколько" };
            
            if (question.StartsWith(queryTypes[1],StringComparison.InvariantCultureIgnoreCase) &&
                words.Any(w => (w.Contains(queryTypes[0],StringComparison.InvariantCultureIgnoreCase) || 
                                w.Contains(queryTypes[4],StringComparison.InvariantCultureIgnoreCase))) && 
                words.Any(w => w.Contains(keyWord,StringComparison.InvariantCultureIgnoreCase)))
            { //пользователь количество (кол-во) инициатив
                queryType = 1;
            }
            else if(question.StartsWith(queryTypes[2],StringComparison.InvariantCultureIgnoreCase) && 
                words.Any(w => w.Contains(keyWord,StringComparison.InvariantCultureIgnoreCase)))
            { //соотношение инициатив
                queryType = 2;
            }
            else if((question.StartsWith(queryTypes[0],StringComparison.InvariantCultureIgnoreCase) || 
                     question.StartsWith(queryTypes[4],StringComparison.InvariantCultureIgnoreCase) || 
                     question.StartsWith(queryTypes[5],StringComparison.InvariantCultureIgnoreCase)) &&
                words.Any(w => w.Contains(queryTypes[3],StringComparison.InvariantCultureIgnoreCase)) && 
                words.Any(w => w.Contains(keyWord,StringComparison.InvariantCultureIgnoreCase)))
            { //количество (кол-во, сколько) инициатив статусе 
                queryType = 3;
            }
            else if((question.StartsWith(queryTypes[0],StringComparison.InvariantCultureIgnoreCase) || 
                     question.StartsWith(queryTypes[4],StringComparison.InvariantCultureIgnoreCase) || 
                     question.StartsWith(queryTypes[5],StringComparison.InvariantCultureIgnoreCase)) && 
                words.Any(w => w.Contains(keyWord,StringComparison.InvariantCultureIgnoreCase)))
            { //количество инициатив
                queryType = 4;
            }
            else
            { // not found
                return (false, 0, 0);
            }
            
            return (true, queryType, 0);
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