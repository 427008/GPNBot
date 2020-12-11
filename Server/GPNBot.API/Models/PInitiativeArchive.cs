using System;

using GPNBot.API.Attributes;
using GPNBot.API.Extensions;


namespace GPNBot.API.Models
{
    public class PInitiativeArchive
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
        public string PLACE1 { get; set; }
        [Persist]
        public string PLACE2 { get; set; }
        [Persist]
        public string PLACE3 { get; set; }

        [Persist]
        public string DIRECTION { get; set; }
        [Persist]
        public string RESPONSIBLE { get; set; }
        [Persist]
        public string DESCRIPTION { get; set; }
        [Persist]
        public string SOLUTION { get; set; }
        [Persist]
        public string EFFECT { get; set; }

        [Persist]
        public long EDITOR { get; set; }
        [Persist]
        public long COMMENT_COUNT { get; set; }

        public static string Select => $"SELECT {typeof(PInitiativeArchive).GetSelectableFieldListStr()} FROM ?.initiative_archive";

        public static string SelectCount => "SELECT Count(1) FROM ?.initiative_archive";
    }
}


/*
   `STATUS` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Статус',
  `PLACE1` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Предприятие',
  `PLACE2` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Производство / Подразделение',
  `PLACE3` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Комплекс / Отдел',
  `DIRECTION` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Рекомендованное направление',
  `ADVANCE_DATE` datetime DEFAULT NULL COMMENT 'Рекомендованя дата реализации',
  `RESPONSIBLE` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT 'Ответственный за инициативу',
  `DESCRIPTION` text COLLATE utf8_unicode_ci COMMENT 'Описание',
  `SOLUTION` text COLLATE utf8_unicode_ci COMMENT 'Решение',
  `EFFECT` text COLLATE utf8_unicode_ci COMMENT 'Ожидаемый эффект',
  `EDITOR` int(11) DEFAULT NULL COMMENT 'Ответственный за редактирование',
  `COMMENT_COUNT` int(11) DEFAULT NULL COMMENT 'Количество комментариев',
*/