using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Analytics.Models
{
    public class Tags
    {
        [Key]
        public Int64 Id { get; set; }
        public string TagType { get; set; }
        public string Tag { get; set; }
        public int TagCount { get; set; }
    }
}
