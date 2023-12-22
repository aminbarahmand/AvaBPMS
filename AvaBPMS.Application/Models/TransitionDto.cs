using AvaBPMS.Domain.Enums;

namespace AvaBPMS.Application.Models
{
    public class TransitionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int SourceWorkFlowNodeId { get; set; }
         
        public int NextWorkFlowNodeId { get; set; }
        
        public TransitionCommandType Command { get; set; }
        public TransitionCondition TransitionCondition { get; set; }
        public double? NodeValue { get; set; }
    }
    public class TransitionEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int SourceWorkFlowNodeId { get; set; }

        public int NextWorkFlowNodeId { get; set; }

        public TransitionCommandType Command { get; set; }
        public TransitionCondition TransitionCondition { get; set; }
        public double? NodeValue { get; set; }
    }
    public class TransitionCreateDto
    {
        
        public string Title { get; set; }
        public int SourceWorkFlowNodeId { get; set; }

        public int NextWorkFlowNodeId { get; set; }

        public TransitionCommandType Command { get; set; }
        public TransitionCondition TransitionCondition { get; set; }
        public double? NodeValue { get; set; }
    }
}
