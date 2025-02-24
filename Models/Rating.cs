using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskConnectProject.Models
{
    public class Rating
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string FromUserEmail { get; set; }
        public int Stars { get; set; }  // 1-5 stars
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TaskName { get; set; }
    }
}
