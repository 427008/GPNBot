using GPNBot.API.Attributes;
using GPNBot.API.Extensions;
using System;

namespace PIImport
{
    public class Initiative
    {
        public Initiative()
        {
            Last_comment = new LastComment();
        }

        public string ID { get; set; }

        
        private ulong _Iid;
        [Persist]
        public ulong Iid
        { 
            get 
            { 
                if(_Iid == 0)
                    _Iid = ulong.Parse(ID);
                return _Iid;
            }
            set { _Iid = value; }
        }
        [Persist]
        public string Status { get; set; }
        
        private DateTime _Created;
        [Persist]
        public DateTime Created 
        { 
            get 
            {  
                if(_Created.Equals(DateTime.MinValue))
                    _Created = string.IsNullOrEmpty(Date) ? DateTime.MinValue : DateTime.Parse(Date);
                return _Created;
            }
            set { _Created = value; }
        }
        public string Date { get; set; }
        [Persist]
        public string Author { get; set; }
        [Persist]
        public string Organization { get; set; }
        [Persist]
        public string Problem { get; set; }
        [Persist]
        public string Solution { get; set; }
        public string Comments_COUNT { get; set; }
        
        private int? _CommentsCount;
        [Persist]
        public int? CommentsCount
        { 
            get 
            {  
                if(!_CommentsCount.HasValue)
                    _CommentsCount = int.Parse(Comments_COUNT ?? "0");
                return _CommentsCount;
            }
            set { _CommentsCount = value; }
        }
        public LastComment Last_comment { get; set; }
        
        private string _LastCommentDate;
        [Persist]
        public string LastCommentDate
        { 
            get 
            {  
                if(string.IsNullOrEmpty(_LastCommentDate))
                    _LastCommentDate = Last_comment.Date;
                return _LastCommentDate;
            }
            set { _LastCommentDate = value; }
        }

        private string _LastCommentText;
        [Persist]
        public string LastCommentText 
        { 
            get 
            {  
                if(string.IsNullOrEmpty(_LastCommentText))
                    _LastCommentText = Last_comment.Text;
                return _LastCommentText;
            }
            set { _LastCommentText = value; }
        }

        public class LastComment
        {
            public string Date { get; set; }
            public string Text { get; set; }
        }

        public static string Select => $"SELECT {typeof(Initiative).GetSelectableFieldListStr()} FROM ?.Initiatives";
        public static string SelectCount => "SELECT Count(1) FROM ?.Initiatives";
        public static string Insert => "INSERT INTO ?.Initiatives";
    }
}
