using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Property_Management.Models
{
    public class ResponseModel

    {

        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }

       public struct StatusCodes
        {
            public static int success = 0;
            public static int fail   = 1;
            public static int unauthorized   = 2;

        }
    }
}
