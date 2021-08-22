using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Property_Management
{
    public class Configs
    {
        public static string defaultErrorMsg = ConfigurationManager.AppSetting["Messages:default_error_msg"];
        public static string user_exist_msg = ConfigurationManager.AppSetting["Messages:user_exist_msg"];
        public static string user_not_created_msg = ConfigurationManager.AppSetting["Messages:user_not_created"];
        public static string user_created_msg = ConfigurationManager.AppSetting["Messages:user_created"];
        public static string login_failed_msg = ConfigurationManager.AppSetting["Messages:login_failed"];
        public static string login_successful_msg = ConfigurationManager.AppSetting["Messages:login_successful"];
    }
}
