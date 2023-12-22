

namespace AvaBPMS.Application
{
    public interface IApplicationDbContext
    {
        DbSet<Pool> PoolList { get; }
        DbSet<Lane> LaneList { get; }
        DbSet<WorkFlowNode> WorkFlowNodeList { get; }
        DbSet<Transition> TransitionList { get; }

        DbSet<WorkFlowInstanse> WorkFlowInstanseList { get; }
        DbSet<WorkFlowStep> WorkFlowStepList { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }

}
