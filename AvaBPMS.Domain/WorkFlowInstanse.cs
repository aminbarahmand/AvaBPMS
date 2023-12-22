namespace AvaBPMS.Domain
{
    public class WorkFlowInstanse
    {
        public int Id { get; set; }

        public DateTime AssignDate { get; set; }

        public int PoolId { get; set; }
        public Pool Pool { get; set; }

        public string UserId { get; set; }

        public DateTime? EndDate { get; set; }

        public ICollection<WorkFlowStep> WorkFlowStepList { get; set; }
        
    }
     
    public class WorkFlowStep
    {
        public int Id { get; set; }       
        public int WorkFlowInstanseId { get; set; }
        public WorkFlowInstanse WorkFlowInstanse { get; set; }
        
        public DateTime ReceiveDate { get; set; }

        public DateTime? EndDate { get;set; }
        public int WorkFlowNodeId { get; set; }
        public WorkFlowNode WorkFlowNode { get; set; }

        public int TransitionId { get; set; }
        public double? StepValue { get; set; }
        public bool IsALive { get; set; }

    }
}
