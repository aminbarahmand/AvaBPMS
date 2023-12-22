
using AvaBPMS.Domain.Enums;

namespace AvaBPMS.Domain
{
    public class Pool: BaseAuditableEntity
    {        
        public string Title { get; set; }
        public ICollection<Lane> LaneList { get; set; }
    }
    public class Lane:BaseAuditableEntity {

        public int PoolId { get; set; }
        public Pool Pool { get; set; }
        public string Title { get; set; }
        public string RelatedRoleId { get; set; }
        public string RelatedUserId {  get; set; }
        public ICollection<WorkFlowNode> WorkFlowNodeList { get; set; }

    }
    

}
