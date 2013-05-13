﻿namespace WPCloudApp.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using WPCloudApp.Web.Models;

    [CLSCompliant(false)]
    public interface IUserRepository
    {
        void CreateUser(string userId, string userName, string email);

        IEnumerable<User> GetAllUsers();

        User GetUser(string userId);

        bool UserExists(string userId);
    }
}
