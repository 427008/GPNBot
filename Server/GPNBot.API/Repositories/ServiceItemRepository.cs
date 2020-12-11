using System;
using System.Collections.Generic;

using GPNBot.API.Models;


namespace GPNBot.API.Repositories
{
    public static class ServiceItemRepository
    {
        private static List<ServiceItem> _serviceItem = new List<ServiceItem>
        {
            new ServiceItem
            { 
                Id = 1,
                GUID = Guid.Parse("8FE704C2-A590-4D31-ABAE-47F6CF07A112"),
                Title = "Портал инициатив",
                Image = "resources/images/img1.png",
                Href = "#main/initiatives"
            },
            new ServiceItem
            { 
                Id = 2,
                GUID = Guid.Parse("2D81B5B6-AF4C-4AEA-A4D8-81A1E89DA818"),
                Title = "Производственная безопасность",
                Image = "resources/images/img2.png",
            },
            new ServiceItem
            { 
                Id = 3,
                GUID = Guid.Parse("4284233B-A197-49FB-AECF-792D34BEEDFB"),
                Title = "T&D (HR)",
                Image = "resources/images/img3.png",
            },
            new ServiceItem
            { 
                Id = 4,
                GUID = Guid.Parse("CB705EA7-EEC6-4D92-8781-492C5C033EF6"),
                Title = "IT",
                Image = "resources/images/img4.png",
            },
        };

        public static List<ServiceItem> GetServiceItems() => _serviceItem;
    }
}
