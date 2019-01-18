using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace Server
{
    
    public class UserClass
    {
        public int ID { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public OperationContext operationContext { get; set; }
    }
}