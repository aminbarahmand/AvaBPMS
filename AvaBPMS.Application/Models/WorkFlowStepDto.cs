using AvaBPMS.Domain.Enums;

namespace AvaBPMS.Application.Models
{
    public class WorkFlowStepDto
    {
        public int Id { get; set; }
        public int WorkFlowInstanseId { get; set; }
        public int PoolId { get; set; }

        public string PoolTitle { get; set; }
        public DateTime ReceiveDate { get; set; }

        public DateTime? EndDate { get; set; }
        public int WorkFlowNodeId { get; set; }
        public string WorkFlowNodeTitle { get; set; }

        public WorkFlowNodeType WorkFlowNodeType { get; set; }
        public string UserId { get; set; }

        public int TransitionId { get; set; }
        public double? StepValue { get; set; }
        public bool IsALive { get; set; }

    }

    
}
