using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormPdfEasy.Entities
{
    public class FormPdfEasyContext : IdentityDbContext
    {
        public FormPdfEasyContext(DbContextOptions<FormPdfEasyContext> options)
            : base(options)
        {
            Database.EnsureCreated();
            //Database.Migrate();
        }

        public DbSet<CustomUser> CustomUsers { get; set; }
    }
    
}
