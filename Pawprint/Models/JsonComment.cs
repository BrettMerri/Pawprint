using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pawprint.Models
{
    public class JsonComment
    {
        public int PostID { get; set; }
        public string DisplayName { get; set; }
        public int CommentID { get; set; }
        public string Text { get; set; }
        public string FilePath { get; set; }
    }
}