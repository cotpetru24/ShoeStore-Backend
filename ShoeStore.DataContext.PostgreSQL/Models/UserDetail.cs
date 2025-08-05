﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models
{
    public partial class UserDetail
    {
        public string AspNetUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual IdentityUser AspNetUser { get; set; }
    }
}
