//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pawprint.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Like
    {
        public int PostID { get; set; }
        public string UserID { get; set; }
        public int LikeID { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Post Post { get; set; }
    }
}
