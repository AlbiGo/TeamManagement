﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamMan.ViewModels
{
    public class LoginViewModel 
    {
        public string username { get; set; }

        public string password { get; set; }
        public bool RememberMe { get; set; }

    }
}
