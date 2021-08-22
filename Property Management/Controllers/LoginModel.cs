using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Property_Management.Controllers
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username Field is empty")]

        public String username { get; set; }

        [Required(ErrorMessage = "Password Field is empty")]

        public String password { get; set; }
    }
}
