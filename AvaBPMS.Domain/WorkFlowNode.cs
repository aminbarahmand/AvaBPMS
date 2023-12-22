
using AvaBPMS.Domain.Enums;
using System.IO;
using System.Runtime.InteropServices.Marshalling;

namespace AvaBPMS.Domain
{
    public class WorkFlowNode : BaseAuditableEntity
    {
        public int LaneId { get; set; }
        public Lane Lane { get; set; }
        public string Title { get; set; }
        public WorkFlowNodeType WorkFlowNodeType { get; set; }

        public ICollection<Transition> SourceTransitionList { get; set; }
        public ICollection<Transition> NextTransitionList { get; set; }

        public ICollection<WorkFlowStep> WorkFlowStepList { get; set; }
    }
    public class Transition
    {
        public int Id { get; set; }
        public string Title { get; set; }


        public int SourceWorkFlowNodeId { get; set; }
        public WorkFlowNode SourceWorkFlowNode { get; set; }
        
        
        public int NextWorkFlowNodeId { get; set; }
        public WorkFlowNode NextWorkFlowNode { get; set; }


        public TransitionCommandType Command { get; set; }
        public TransitionCondition TransitionCondition {  get; set; }
        public double? NodeValue { get; set; }
    }
}
