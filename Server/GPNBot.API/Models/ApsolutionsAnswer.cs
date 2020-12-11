namespace GPNBot.API.Models
{
    public class ApsolutionsAnswer
    {
        public long ServiceID { get; set; }
        public string QuestionCategory { get; set; }

        public ApsolutionsResult[] Results { get; set; }

        public ApsolutionsRecord[] Records { get; set; }

        public ApsolutionsCommand[] Commands { get; set; }
    }

    public class ApsolutionsResult
    { 
    }
    public class ApsolutionsRecord
    { 
        public string Text { get; set; }
        public float Similarity { get; set; }
    }
    public class ApsolutionsCommand
    { 
    }

    public class ApsolutionsAggregate
    {
        public bool Success { get; set; }
        public Aggregate Res { get; set; }
    }

    public class Aggregate
    { 
        public long Count { get; set; }
    }
}
