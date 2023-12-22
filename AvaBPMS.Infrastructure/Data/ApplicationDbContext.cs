using System.Reflection;

using Microsoft.EntityFrameworkCore;
using AvaBPMS.Application;
using AvaBPMS.Domain;
using AvaBPMS.Infrastructure.Data.Interceptors;

namespace AvaBPMS.Infrastructure.Data;

public class ApplicationDbContext : DbContext,IApplicationDbContext
{
    
    public DbSet<Pool> PoolList {get; set;}
    public DbSet<Lane> LaneList { get; set; }
    public DbSet<WorkFlowNode> WorkFlowNodeList { get; set; }
    public DbSet<Transition> TransitionList { get; set; }

    public DbSet<WorkFlowInstanse> WorkFlowInstanseList { get; set; }

    public DbSet<WorkFlowStep> WorkFlowStepList { get; set; }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
       ) 
        : base(options)
    {
         
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<WorkFlowNode>()
         .HasMany(f => f.SourceTransitionList).WithOne(t => t.SourceWorkFlowNode).OnDelete(DeleteBehavior.NoAction);

        builder.Entity<WorkFlowNode>()
         .HasMany(f => f.NextTransitionList).WithOne(t => t.NextWorkFlowNode).OnDelete(DeleteBehavior.NoAction);

        builder.Entity<WorkFlowNode>()
           .HasMany(f => f.WorkFlowStepList).WithOne(t => t.WorkFlowNode).OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(builder);
    }   
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {      

        return await base.SaveChangesAsync(cancellationToken);
    }
}
