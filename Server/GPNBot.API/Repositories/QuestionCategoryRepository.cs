using System;
using System.Collections.Generic;

using GPNBot.API.Models;


namespace GPNBot.API.Repositories
{
    public static class QuestionCategoryRepository
    {
        private static List<ServiceItem> _questionCategories = new List<ServiceItem>
        {
            new ServiceItem
            {
                Id = 1,
                GUID = Guid.Parse("EBEA64F6-0EEF-42D2-B829-C9B715B057F4"),
                Title = "Поиск информации" //InfoRequest
            },
            new ServiceItem
            {
                Id = 2,
                GUID = Guid.Parse("07E54933-82D0-4B2B-BE0B-9EBCE96753CD"),
                Title = "Запрос к данным" // DataRequest
            },
            new ServiceItem
            {
                Id = 3,
                GUID = Guid.Parse("4F907DBD-FCA4-4F78-95DE-0165B3006675"),
                Title = "Запрос на обслуживание" // ServiceRequest
            },
        };

        public static List<ServiceItem> GetQuestionCategories() => _questionCategories;
    }
}
