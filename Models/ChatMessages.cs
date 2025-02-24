using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskConnectProject.Models
{
    public class ChatMessages
    {
        //public string? Message { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
    }
}
