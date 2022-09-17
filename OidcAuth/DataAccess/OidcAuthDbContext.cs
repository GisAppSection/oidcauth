using System;
using OidcAuth.Models;
using Microsoft.EntityFrameworkCore;

// https://docs.microsoft.com/en-us/ef/core/modeling/keys

namespace OidcAuth.DataAccess
{
    public partial class OidcAuthDbContext : DbContext
    {
        public OidcAuthDbContext()
        {
        }

        public OidcAuthDbContext(DbContextOptions<OidcAuthDbContext> options)
            : base(options)
        {
        }

        
        public virtual DbSet<ServiceInfo> ServiceInfo { get; set; }

        public virtual DbSet<ExceptionLog> ExceptionLog { get; set; }


    }


}
