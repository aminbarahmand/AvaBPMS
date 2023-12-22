using AvaBPMS.Domain.Enums;

namespace AvaBPMS.Application.Models
{
    public class WorkFlowNodeDto
    {
        public int Id { get; set; }
        public int PoolId { get; set; }

        public string PoolTitle { get; set; }
        public int LaneId { get; set; }
        public string LaneTitle { get; set; }
        public string Title { get; set; }
        public WorkFlowNodeType WorkFlowNodeType { get; set; }

        
    }
    public class WorkFlowNodeCreateDto
    {
        
        public int LaneId { get; set; }
        
        public string Title { get; set; }
        public WorkFlowNodeType WorkFlowNodeType { get; set; }


    }
    public class WorkFlowNodeEditDto
    {
        public int Id { get; set; }
        public int LaneId { get; set; }
        
        public string Title { get; set; }
        public WorkFlowNodeType WorkFlowNodeType { get; set; }


    }
}
