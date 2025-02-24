using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskConnectProject.Models
{
    public class TaskModel
    {
        public string Task { get; set; }
        public string Category { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string createdBy{ get; set; }
        public string Status{ get; set; }
        public bool isBooked { get; set; } = false;
        public string FirebaseKey { get; set; }
        public string LoggedInUserEmail { get; set; }
    }
}
