using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.Model
{
    class UpdateRequestModel
    {
        public Guid request_id { get; set; }
        public bool isValidated { get; set; }
    }
}
