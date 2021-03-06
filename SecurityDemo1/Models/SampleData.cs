﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecurityDemo1.Models
{
    public class SampleData
    {
        public static void InitializeData(IServiceProvider provider, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("InitializeData");

            using (var serviceScope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var env = serviceScope.ServiceProvider.GetService<IHostingEnvironment>();
                if (!env.IsDevelopment()) return;  // Only in Development

                CreateUsers(logger, serviceScope).GetAwaiter().GetResult();

                //var db = serviceScope.ServiceProvider.GetService<TripRepository>();

                //if (!db.Db.FAQ.Any())
                //{
                //    db.Db.FAQ.Add(new Faq
                //    {
                //        Question = "Why did the chicken cross the road?",
                //        Answer = "To Get to the other side"
                //    });
                //    db.Db.SaveChanges();
                //}
                //else
                //{
                //    logger.LogDebug("FAQ found, not creating sample FAQ");
                //}

                //// Exit now if we already have trips created
                //if (db.Get().Any())
                //{
                //    logger.LogDebug("Data found, not creating sample records");
                //    return;
                //}

                //var startDate = FirstFridayNextMonth(DateTime.Today);
                //var endDate = startDate.AddDays(2);

                //var newTrip = new Trip
                //{
                //    Name = "Weekend in NYC",
                //    Description = "Train to New York City for the weekend",
                //    StartDate = startDate,
                //    EndDate = startDate.AddDays(3).AddMinutes(-1)
                //};

                //var trainDepart = new Segment
                //{
                //    Name = "Amtrak Train PHL->NYP",
                //    StartDate = startDate.AddHours(17),
                //    EndDate = startDate.AddHours(19),
                //    DepartureAddress = "30th St. Station\n Philadelphia, PA",
                //    ArrivalAddress = "New York Penn Station\nNew York, NY",
                //    ReservationID = "123456",
                //    Trip = newTrip,
                //    Type = SegmentType.Train
                //};
                //newTrip.Segments.Add(trainDepart);

                //var lodging = new Segment
                //{
                //    Name = "Coolio Hotel at Times Square",
                //    StartDate = startDate.AddHours(19).AddMinutes(30),
                //    EndDate = endDate.AddHours(12),
                //    ArrivalAddress = "123456 Times Square, New York, NY",
                //    ReservationID = "ABCDE",
                //    Trip = newTrip,
                //    Type = SegmentType.Lodging
                //};
                //newTrip.Segments.Add(lodging);

                //var trainReturn = new Segment
                //{
                //    Name = "Amtrak Train NYP->PHL",
                //    StartDate = endDate.AddHours(15),
                //    EndDate = endDate.AddHours(17),
                //    DepartureAddress = "New York Penn Station\nNew York, NY",
                //    ArrivalAddress = "30th St. Station\n Philadelphia, PA",
                //    ReservationID = "654321",
                //    Trip = newTrip,
                //    Type = SegmentType.Train
                //};
                //newTrip.Segments.Add(trainReturn);

                //db.Add(newTrip);

                //var vegasTrip = new Trip
                //{
                //    Name = "Week in Vegas",
                //    Description = "Trip with friends to Las Vegas",
                //    StartDate = DateTime.Now.Date.AddDays(21),
                //    EndDate = DateTime.Now.Date.AddDays(28)
                //};
                //db.Add(vegasTrip);

                //db.Db.SaveChanges();
            }

        }

        private static async Task CreateUsers(ILogger logger, IServiceScope serviceScope)
        {

            logger.LogInformation("===> beginning create users");

            var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            const string DefaultEmail = "test@test.com";
            const string InitialPassword = "Pass.1234";
            const string AdminRole = "Admin";
            if (!roleManager.Roles.Any(role => role.Name == AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            var existingAdminUser = await userManager.FindByEmailAsync(DefaultEmail);
            if (existingAdminUser != null)
            {
                await userManager.DeleteAsync(existingAdminUser);
                logger.LogInformation("!==> Deleted Admin user");
            }
            if (!userManager.Users.Any(u => u.UserName == DefaultEmail))
            {

                var user = new ApplicationUser
                {
                    GivenName = "Admin",
                    Surname = "Admin",
                    UserName = DefaultEmail,
                    Email = DefaultEmail
                };
                var result = await userManager.CreateAsync(user, InitialPassword);
                logger.LogInformation($"==> Created user {user.UserName} with password {InitialPassword}");
                if (result.Succeeded)
                {

                    await userManager.AddClaimsAsync(user, new Claim[]
                    {
                new Claim(ClaimTypes.Name, $"{user.GivenName}, {user.Surname}"),
                new Claim(ClaimTypes.GivenName, user.GivenName),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ApplicationUser.AppClaims.CommitteeMember, "treasurer"),
                new Claim(ApplicationUser.AppClaims.CommitteeMember, "secretary"),
                new Claim(ClaimTypes.Email, user.Email)
                    });
                    await userManager.AddToRoleAsync(user, AdminRole);
                    logger.LogInformation($"Added user {user.UserName} to role {AdminRole}");

                }
            }
        }

        private static DateTime FirstFridayNextMonth(DateTime dateToCheck)
        {

            var firstOfNextMonth = dateToCheck.AddMonths(1).AddDays(-1 * (dateToCheck.Day - 1));
            var daysUntilFriday = 5 - (int)firstOfNextMonth.DayOfWeek;

            return daysUntilFriday > 0 ? firstOfNextMonth.AddDays(daysUntilFriday) : firstOfNextMonth.AddDays(7 + daysUntilFriday);

        }

    }
}