using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;



namespace _4sqChat.Models
{
    public class FoursquareUserContext : DbContext
    {
        public DbSet<FoursquareUserModel> FoursquareUsers { get; set; }
        public DbSet<MessageModel> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public FoursquareUserContext() : base("DefaultConnection")
        {

        }
    }

    public class FoursquareUserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FoursquareUserId { get; set; }
        public Guid UserGuid { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string LastVenueID { get; set; }
        [DefaultValue(false)]
        public bool IsPremium { get; set; }
    }
}