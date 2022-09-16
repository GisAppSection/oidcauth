using System;
using OidcAuthModels;
using Microsoft.EntityFrameworkCore;

// https://docs.microsoft.com/en-us/ef/core/modeling/keys

namespace OidcAuthDataAccess
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
