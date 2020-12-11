namespace GPNBot.API.Models
{
    public class QueryFilter
    {
        public string Property { get; set; }

        private string _operator;
        public string Operator
        {
            get => _operator;
            set
            {
                switch (value)
                {
                    case "ne":
                    case "neq":
                        _operator = "!="; break;
                    case "lt":
                        _operator = "<"; break;
                    case "le":
                        _operator = "<="; break;
                    case "gt":
                        _operator = ">"; break;
                    case "ge":
                        _operator = ">="; break;
                    case "in":
                        _operator = " IN "; break;

                    default:
                        _operator = "=";
                        break;
                }
            }
        }

        public object Value { get; set; }
    }
}
