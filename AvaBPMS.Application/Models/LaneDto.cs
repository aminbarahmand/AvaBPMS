using AvaBPMS.Domain.Common;

namespace AvaBPMS.Application.Models
{
    public class LaneDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int PoolId { get; set; }
        public string PoolTitle { get; set; }       
        public string RelatedRoleId { get; set; }
        public string RelatedUserId { get; set; }
    }
    public class LaneCreateDto
    {        
        public string Title { get; set; }
        public int PoolId { get; set; }
       
        public string RelatedRoleId { get; set; }
        public string RelatedUserId { get; set; }
    }
    public class LaneEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int PoolId { get; set; }
         
        public string RelatedRoleId { get; set; }
        public string RelatedUserId { get; set; }
    }
}
