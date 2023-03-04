using Microsoft.EntityFrameworkCore;
using Open_When.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open_When.Persistence
{
    public class AppDbContext : DbContext
    {

        public DbSet<Letter> Letters { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=data.db;");
        }
    }
}
