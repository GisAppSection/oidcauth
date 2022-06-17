using System;
using OidcAuthV3.Models;
using Microsoft.EntityFrameworkCore;

// https://docs.microsoft.com/en-us/ef/core/modeling/keys

namespace OidcAuthV3.DataAccess
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
       

    }


}
