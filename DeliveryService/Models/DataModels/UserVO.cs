using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeliveryService.Models.DataModels
{
    public class UserVO
    {
        public string userId { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }
}