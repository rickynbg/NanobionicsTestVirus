using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using NanobionicsTestVirusDesktop.Models;

namespace NanobionicsTestVirusDesktop.Context
{
    public class DBContext : DbContext
    {
        public DbSet<FileMeasure> FileMeasures { get; set; }
        public DbSet<DataMeasure> DataMeasures { get; set; }


        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            string dbName = "NanobionicsDB.db";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);
            optionsBuilder.UseSqlite("Data source=" + path);
            //optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }
    }
}
