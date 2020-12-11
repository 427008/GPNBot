using System;

using GPNBot.API.Attributes;
using GPNBot.API.Extensions;


namespace GPNBot.API.Models
{
    public class PInitiative
    {
        [Key]
        [Persist]
        public long ID  { get; set; }
        
        [Persist]
        public string TITLE { get; set; }
        [Persist]
        public char ACTIVE { get; set; }
        [Persist]
        public DateTime CREATE_DATE { get; set; }
        [Persist]
        public long CREATE_BY { get; set; }
        [Persist]
        public string STATUS { get; set; }
        [Persist]
        public long AUTHOR { get; set; }
        [Persist]
        public string PROBLEM { get; set; }
        [Persist]
        public string SOLUTION { get; set; }
        [Persist]
        public string EFFECT { get; set; }
        [Persist]
        public long TREND { get; set; }

        [Persist]
        public long PLACE1 { get; set; }
        [Persist]
        public long PLACE2 { get; set; }
        [Persist]
        public long PLACE3 { get; set; }
        [Persist]
        public long PLACE4 { get; set; }
        [Persist]
        public char TEST { get; set; }
        [Persist]
        public char STAGE1_APPROVED { get; set; }
        [Persist]
        public char STAGE1_NEED_NETWORK_GROUP { get; set; }
        [Persist]
        public char STAGE1_CAN_REALIZE_INDEPENDENTLY { get; set; }
        [Persist]
        public char STAGE2_APPROVED { get; set; }

        [Persist]
        public double STAGE2_SCORE { get; set; }
        [Persist]
        public long ADMINISTRATOR { get; set; }
        [Persist]
        public string IMPLEMENTATION { get; set; }
        [Persist]
        public long RESPONSIBLE { get; set; }
        [Persist]
        public string REASON { get; set; }
        [Persist]
        public DateTime STAGE1_DEADLINE { get; set; }
        [Persist]
        public DateTime STAGE2_DEADLINE { get; set; }
        [Persist]
        public string ERROR { get; set; }

        [Persist]
        public long EDITOR { get; set; }
        [Persist]
        public long COMMENT_COUNT { get; set; }
        [Persist]
        public long LIKES_COUNT { get; set; }
        [Persist]
        public long TASK { get; set; }
        [Persist]
        public long IMPLEMENTATION_RESPONSIBLE { get; set; }
        [Persist]
        public string IMPLEMENTATION_STATUS { get; set; }
        [Persist]
        public long IMPLEMENTATION_PROGRAM { get; set; }
        [Persist]
        public DateTime RATING_DATE { get; set; }
        [Persist]
        public string SITUATION { get; set; }
        [Persist]
        public DateTime STATUS_DATE { get; set; }
        [Persist]
        public DateTime CONFIRMED_DATE { get; set; }
        [Persist]
        public DateTime IMPL_DATE_1 { get; set; }
        [Persist]
        public DateTime IMPL_DATE_2 { get; set; }
        [Persist]
        public DateTime PUBLICATION_DATE { get; set; }
        [Persist]
        public string IMPLEMENTATION_JOURNAL { get; set; }
        [Persist]
        public string CATEGORY { get; set; }
        [Persist]
        public char STAGE2_CAN_REALIZE_INDEPENDENTLY { get; set; }
        [Persist]
        public char REWARD { get; set; }
        [Persist]
        public string STATUS_BEFORE { get; set; }
        [Persist]
        public char FROZEN { get; set; }


        public static string StatusRu(string engString)
        { 
            if(engString.EqIgnoreCase("REJECTED"))
                return "Отклонена";
            else if(engString.EqIgnoreCase("REQUIRES_REJECT"))
                return "Подана на отклонение";
            else if(engString.EqIgnoreCase("DRAFT"))
                return "Черонвик";
            else if(engString.EqIgnoreCase("REQUIRES_MODERATION"))
                return "Подана на модерацию";
            else if(engString.EqIgnoreCase("MODERATION"))
                return "Модерация";
            else if(engString.EqIgnoreCase("ERROR"))
                return "Ошибка";
            else if(engString.EqIgnoreCase("REWORK"))
                return "Пересмотр";
            else if(engString.EqIgnoreCase("STAGE1"))
                return "Этап 1";
            else if(engString.EqIgnoreCase("STAGE2"))
                return "Этап 2";
            else if(engString.EqIgnoreCase("IMPLEMENTATION"))
                return "Реализация";

            return "Неопределен";
        }

        public static string Select => $"SELECT {typeof(PInitiative).GetSelectableFieldListStr()} FROM ?.initiative";

    }
}


/*
  `TITLE` varchar(255) COLLATE utf8_unicode_ci NOT NULL COMMENT 'Название',
  `ACTIVE` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL DEFAULT 'Y' COMMENT 'Активность',
  `CREATE_DATE` datetime NOT NULL COMMENT 'Дата создания',
  `CREATE_BY` int(11) NOT NULL COMMENT 'Кем создан',
  `UPDATE_DATE` datetime DEFAULT NULL COMMENT 'Дата изменения',
  `UPDATE_BY` int(11) DEFAULT NULL COMMENT 'Кем изменен',
  `STATUS` enum('REJECTED','REQUIRES_REJECT','DRAFT','REQUIRES_MODERATION','MODERATION','ERROR','REWORK','STAGE1','STAGE2','IMPLEMENTATION') COLLATE utf8_unicode_ci NOT NULL DEFAULT 'DRAFT' COMMENT 'Статус инициативы',
  `AUTHOR` int(11) NOT NULL COMMENT 'Автор инициативы',
  `PROBLEM` text COLLATE utf8_unicode_ci NOT NULL COMMENT 'Проблема',
  `SOLUTION` text COLLATE utf8_unicode_ci NOT NULL COMMENT 'Решение',
  `EFFECT` text COLLATE utf8_unicode_ci NOT NULL COMMENT 'Ожидаемый эффект',
  `TREND` int(11) NOT NULL COMMENT 'Направление / Функция / Процесс',
  `PLACE1` int(11) NOT NULL COMMENT 'Предприятие',
  `PLACE2` int(11) DEFAULT NULL COMMENT 'Производство / Подразделение',
  `PLACE3` int(11) DEFAULT NULL COMMENT 'Комплекс / Отдел',
  `PLACE4` int(11) DEFAULT NULL COMMENT 'Установка',
  `TEST` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL COMMENT 'Тестовая',
  `STAGE1_APPROVED` enum('Y','N') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Одобрена на 1 этапе',
  `STAGE1_NEED_NETWORK_GROUP` enum('Y','N') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Требуется привлечение дополнительных экспертов',
  `STAGE1_CAN_REALIZE_INDEPENDENTLY` enum('Y','N') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Реализация своими силами',
  `STAGE2_APPROVED` enum('Y','N') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Одобрена на 2 этапе',
  `STAGE2_SCORE` float(13,2) DEFAULT NULL COMMENT 'Итоговый рейтинг',
  `ADMINISTRATOR` int(11) DEFAULT NULL COMMENT 'Ответственный администратор',
  `IMPLEMENTATION` enum('INCLUDED','IMPLEMENTATION','REJECTED','IMPLEMENTED','CONFIRMED','RECOMMENDED','APPROVED') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Статус реализации',
  `RESPONSIBLE` int(11) DEFAULT NULL COMMENT 'Ответственный за инициативу',
  `REASON` text COLLATE utf8_unicode_ci COMMENT 'Причина отклонения / отправки на доработку',
  `STAGE1_DEADLINE` datetime DEFAULT NULL COMMENT 'Крайний срок 1 этапа',
  `STAGE2_DEADLINE` datetime DEFAULT NULL COMMENT 'Крайний срок 2 этапа',
  `ERROR` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Ошибка автоматической обработки',
  `COMMENT_COUNT` int(11) DEFAULT NULL COMMENT 'Количество комментариев',
  `LIKES_COUNT` int(11) DEFAULT NULL COMMENT 'Количество лайков',
  `TASK` int(11) DEFAULT NULL COMMENT 'Задача от бизнеса',
  `IMPLEMENTATION_RESPONSIBLE` int(11) DEFAULT NULL COMMENT 'Ответственный за реализацию',
  `IMPLEMENTATION_STATUS` enum('RECOMMEND','INCLUDED','IMPLEMENTATION','REJECTED','IMPLEMENTED','CONFIRMED','RECOMMENDED','APPROVED') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Статус реализации',
  `IMPLEMENTATION_PROGRAM` int(11) DEFAULT NULL COMMENT 'Программа реализации',
  `RATING_DATE` datetime DEFAULT NULL COMMENT 'Дата, с которой начинает участие в рейтинге',
  `SITUATION` text COLLATE utf8_unicode_ci NOT NULL COMMENT 'Описание ситуации',
  `ANONYMOUSLY` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL COMMENT 'Анонимно',
  `STAGE1_AUTO_RESOLVE` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL DEFAULT 'Y' COMMENT 'Автоматический переход с 1 этапа',
  `STAGE2_AUTO_RESOLVE` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL DEFAULT 'Y' COMMENT 'Автоматический переход со 2 этапа',
  `STATUS_DATE` datetime DEFAULT NULL COMMENT 'Дата последнего изменения статуса',
  `CONFIRMED_DATE` datetime DEFAULT NULL COMMENT 'Дата назначения ответственного за реализацию',
  `IMPL_DATE_1` datetime DEFAULT NULL COMMENT 'Дата начала реализации',
  `IMPL_DATE_2` datetime DEFAULT NULL COMMENT 'Дата конца реализации',
  `PUBLICATION_DATE` datetime DEFAULT NULL COMMENT 'Дата первой отправки на модерацию',
  `IMPLEMENTATION_JOURNAL` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Журнал ЭЖМ',
  `CATEGORY` enum('Other','Function','Technology','Mechanics','Energetics','Metrology','Construction','Guard','OZSM','Supply') COLLATE utf8_unicode_ci NOT NULL COMMENT 'Категория',
  `STAGE2_CAN_REALIZE_INDEPENDENTLY` enum('Y','N') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Реализация своими силами',
  `REWARD` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL COMMENT 'Премирование',
  `STATUS_BEFORE` enum('REQUIRES_MODERATION','MODERATION','STAGE1','STAGE2','IMPLEMENTATION') COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Статус инициативы до отклонения',
  `FROZEN` enum('N','Y') COLLATE utf8_unicode_ci NOT NULL DEFAULT 'N' COMMENT 'Заморожена',
*/