using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Samples.SqlServer
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }

        public virtual List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public virtual Blog Blog { get; set; }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // use LocalDB for ease of use
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=BlogDatabase;Integrated Security=true");
        }
    }
}