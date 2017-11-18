using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MvcMovie.Models
{
    public class MvcMovieContext: DbContext
    {
        public MvcMovieContext(DbContextOptions<MvcMovieContext> options)  
            : base(options)
        { }

        public DbSet<UserInformation> UserInformations {get;set;}
    }
}