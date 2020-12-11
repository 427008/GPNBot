namespace GPNBot.API.Models
{
    public class ApsolutionsCategory
    {
        public ServiceFound[] ServiceIDs { get; set; }
    
        public QuestionCategories[] QuestionCategories { get; set; }
    }    
    public class ServiceFound
    { 
        public long ServiceID { get; set; }

        public double Similarity { get; set; }
    }

    public class QuestionCategories
    { 
        public long QuestionCategory { get; set; }

        public double Similarity { get; set; }
    }
}
