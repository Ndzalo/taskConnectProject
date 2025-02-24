using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace taskConnectProject.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Timestamp { get; set; }
        public string idNumber { get; set; }
        public string phoneNumber{ get; set; }
        public string firebaseKey { get; set; }
    }
}
