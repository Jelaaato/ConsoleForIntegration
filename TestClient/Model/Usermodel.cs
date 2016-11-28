using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.Model
{
    public class Usermodel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TestModel
    {
        public string hospital_number { get; set; }
        public string date_of_birth { get; set; }
        public string request_type { get; set; }
    }
}
