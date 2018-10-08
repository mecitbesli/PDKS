using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace PDKS
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        { }
        public DbSet<StandartWorkHours> StandartWorkhoursSet { get; set; }
        public DbSet<User> UserSet { get; set; }
        public DbSet<AdminAuthorization> AdminAuthorizationSet { get; set; }
        public DbSet<UserWorked> UserWorkedSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .Property(b => b.Role)
                        .HasDefaultValue(2);
            modelBuilder.Entity<User>()
                        .Property(b => b.DaysOff)
                        .HasDefaultValue(15);
            modelBuilder.Entity<AdminAuthorization>()
                        .Property(b => b.Customize)
                        .HasDefaultValue(false);
            modelBuilder.Entity<AdminAuthorization>()
                        .Property(b => b.Authority)
                        .HasDefaultValue(false);
            modelBuilder.Entity<AdminAuthorization>()
                        .Property(b => b.Requests)
                        .HasDefaultValue(false);
            modelBuilder.Entity<UserWorked>()
                        .Property(b => b.ReqApproved)
                        .HasDefaultValue(2);
        }
    }


    public class StandartWorkHours
    {
        public int Id { get; set; }
        public DateTime StartHr { get; set; }
        public DateTime EndHr { get; set; }
        public int BreakTime { get; set; }

        public List<User> Users { get; set; }
    }

    //enum ERole {Admin, User, Inactive}
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public int Role { get; set; } //default User
        public int DaysOff { get; set; } //default 15

        public List<UserWorked> UserWorkedDays { get; set; }

        [Required]
        public int StandartWorkHoursId { get; set; }
        public StandartWorkHours StandartWorkHours { get; set; }

        public AdminAuthorization AdminAuthorization { get; set; }
    }

    public class AdminAuthorization
    {
        public int Id { get; set; }
        public bool Customize { get; set; } //default false
        public bool Requests { get; set; } //default false
        public bool Authority { get; set; } //default false

        public int UserId { get; set; }
        public User User { get; set; }
    }

    //enum ERequest {ChangeStatus=0, UseDayOff=1}
    //enum EReqApproved {No=0, Yes=1, NotProcessedYet=2}
    public class UserWorked
    {
        public int Id { get; set; }
        public int WorkedTime { get; set; }
        public DateTime date { get; set; }
        public string Excuse { get; set; }
        public int Request { get; set; }
        public int ReqApproved { get; set; } //default NotProcessedYet

        public int UserId { get; set; }
        public User User { get; set; }
    }
}