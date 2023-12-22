namespace AvaBPMS.Application.Models
{
    public class WorkFlowInstanseDto
    {
        public int Id { get; set; }

        public DateTime AssignDate { get; set; }

        public int PoolId { get; set; }
        public string PoolTitle { get; set; }

        public string UserId { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsStarted { get;internal set; }
        public bool HasAliveStep { get; internal set; }
    }
    public class WorkFlowInstanseCreateDto
    {   
        public DateTime AssignDate { get; set; }
        public int PoolId { get; set; }        
        public string UserId { get; set; }       


    }

 
}
