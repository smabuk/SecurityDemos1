using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SecurityDemo1.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public static class AppRoles
        {
            public const string Admin = "admin";
            public const string Captain = "captain";
        }

        public static class AppClaims
        {
            public const string Chairman = "chairman";
            public const string CommitteeMember = "comitteemember";
            public const string King = "king";
            public const string Secretary = "secretary";
            public const string Treasurer = "treasurer";
        }

        public static class AppPolicies
        {
            public const string Admin = "admin";
            public const string BigCheese = "bigcheese";
            public const string Chairman = "chairman";
            public const string CommitteeMember = "comittemember";
            public const string King = "king";
            public const string Secretary = "secretary";
            public const string Treasurer = "treasurer";
        }

        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}
