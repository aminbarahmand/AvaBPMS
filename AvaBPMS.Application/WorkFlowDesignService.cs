

using AvaBPMS.Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace AvaBPMS.Application
{
    
    public class WorkFlowStepChangeResult
    {
        public virtual bool IsSuccess { get; set; }

        public virtual ICollection<string> Errors { get; set; }
    }
  
    public class WorkFlowExecutionService
    {
        #region Private Fields
        readonly IApplicationDbContext db;

        public WorkFlowExecutionService(IApplicationDbContext dbContext)
        {
            this.db = dbContext;
        }

        #endregion

        private IQueryable<WorkFlowInstanseDto> GetWorkFlowInstanseQuery()
        {
            return from p in db.WorkFlowInstanseList
                   select new WorkFlowInstanseDto
                   {
                       PoolId = p.PoolId,
                       PoolTitle = p.Pool.Title,
                       Id = p.Id,
                       AssignDate = p.AssignDate,
                       EndDate = p.EndDate,
                       UserId= p.UserId,
                       IsStarted=p.WorkFlowStepList.Any(),
                       HasAliveStep=p.WorkFlowStepList.Any(t=>t.IsALive),
                   };
        }
        public async Task<int> CreateWorkFlowInstanse(WorkFlowInstanseCreateDto workFlowCreateDto)
        {
            var itemToEdit = new WorkFlowInstanse();
            itemToEdit.AssignDate = workFlowCreateDto.AssignDate;
            itemToEdit.UserId = workFlowCreateDto.UserId;
            itemToEdit.PoolId = workFlowCreateDto.PoolId;
            var result = db.WorkFlowInstanseList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return 0;
            return result.Entity.Id;

        }
        public async Task<IList<WorkFlowInstanseDto>> GetAllInstanses(int PoolId)
        {
            return await GetWorkFlowInstanseQuery().Where(t=>t.PoolId==PoolId).ToListAsync();
        }
        public async Task<IList<WorkFlowInstanseDto>> GetAllInstanses()
        {
           return await GetWorkFlowInstanseQuery().ToListAsync();
        }
        private IQueryable<WorkFlowNodeDto> GetWorkFlowNodesQuery()
        {
            return from p in db.WorkFlowNodeList
                   join l in db.LaneList on p.LaneId equals l.Id
                   select new WorkFlowNodeDto
                   {
                       PoolId = l.Pool.Id,
                       PoolTitle = l.Pool.Title,
                       LaneId = l.Id,
                       LaneTitle = l.Title,
                       WorkFlowNodeType = p.WorkFlowNodeType,
                       Title = p.Title,
                       Id = p.Id,


                   };
        }
        public async Task<WorkFlowStepChangeResult> StartWorkFlowInstanse(int InstanseId)
        {
            var instanse = GetWorkFlowInstanseQuery().FirstOrDefault(t => t.Id == InstanseId);
            if (instanse == null)
                return new WorkFlowStepChangeResult { IsSuccess = false };

            var startNode = GetWorkFlowNodesQuery().First(t => t.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.Start && t.PoolId == instanse.PoolId);
            if (startNode == null)
                return new WorkFlowStepChangeResult { IsSuccess = false };
            var outputTransition = db.TransitionList.FirstOrDefault(t => t.SourceWorkFlowNodeId == startNode.Id);

            if (outputTransition == null)
                return new WorkFlowStepChangeResult { IsSuccess = false };


            var startStep = new WorkFlowStep
            {
                IsALive = true,
                WorkFlowNodeId = startNode.Id,
                WorkFlowInstanseId = InstanseId,
                ReceiveDate = DateTime.UtcNow,
                TransitionId = 0,                
                StepValue=null
            };
            var result = db.WorkFlowStepList.Add(startStep);
            await db.SaveChangesAsync(CancellationToken.None);

            if (result!=null)
            {
                return await MoveNext(result.Entity.Id);
               
            }
            return new WorkFlowStepChangeResult { IsSuccess = false };
        }

        public async Task<WorkFlowStepChangeResult> MoveNext(int currentStepId, string command="", double? stepValue = null)
        {
            var sourceStep = db.WorkFlowStepList.AsNoTracking().FirstOrDefault(t => t.Id == currentStepId);
            if(sourceStep != null && sourceStep.IsALive)
            {
                var sourceNode = GetWorkFlowNodesQuery().FirstOrDefault(t => t.Id == sourceStep.WorkFlowNodeId);
                if (sourceNode != null)
                {

                    if (sourceNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.End)
                    {
                        await KillStep(currentStepId);
                        return new WorkFlowStepChangeResult { IsSuccess = true };
                    }
                    else if (sourceNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.Activity
                        || sourceNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.Start)
                    {
                        var outputTransition = db.TransitionList.AsNoTracking().FirstOrDefault(t => t.SourceWorkFlowNodeId == sourceNode.Id);
                        if (outputTransition == null)
                            return new WorkFlowStepChangeResult { IsSuccess = false };
                        await KillStep(currentStepId);

                        var nextStepId = await MoveToNode(sourceStep.WorkFlowInstanseId, outputTransition.NextWorkFlowNodeId, outputTransition.Id);
                        if (nextStepId != null)
                        {
                            await CheckNextNodeAndMoveIfNecessary(nextStepId.Value, command, stepValue);
                        }
                       
                    }
                    else if (sourceNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.ExclusiveGateWay)
                    {
                        var outputTransitionList = db.TransitionList.Where(t => t.NextWorkFlowNodeId == sourceStep.WorkFlowNodeId).AsNoTracking().ToList();
                        if(outputTransitionList.Count > 1)
                        {
                            await HandleMoveToExclusiveGateWay(currentStepId, command, stepValue);
                        }
                       
                    }
                    return new WorkFlowStepChangeResult { IsSuccess = true };
                }
            }
            return new WorkFlowStepChangeResult { IsSuccess = false };
        }
        private async Task<WorkFlowStepChangeResult> CheckNextNodeAndMoveIfNecessary(int nextStepId, string command = "", double? stepValue = null)
        {
            var nextStep = db.WorkFlowStepList.AsNoTracking().FirstOrDefault(t => t.Id == nextStepId);
            if (nextStep != null && nextStep.IsALive)
            {
                var nextNode = GetWorkFlowNodesQuery().FirstOrDefault(t => t.Id == nextStep.WorkFlowNodeId);
                if (nextNode != null)
                {
                    //next node is end of flow and must move
                    if (nextNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.End)
                    {
                        await KillStep(nextStep.Id);                        
                        return new WorkFlowStepChangeResult { IsSuccess = true };
                    }
                    //next node is Acivity, stop
                    else if (nextNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.Activity)
                    {
                        return new WorkFlowStepChangeResult { IsSuccess = true };
                    }                    
                    else if (nextNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.ExclusiveGateWay)
                    {
                        await HandleMoveToExclusiveGateWay(nextStep.Id,command, stepValue);
                    }
                    else if (nextNode.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.ParallelGateWay)
                    {
                        await HandleMoveToParallelGateWay(nextStep.Id);
                    }
                    return new WorkFlowStepChangeResult { IsSuccess = true };
                }
            }
            return new WorkFlowStepChangeResult { IsSuccess = false };
        }



        private async Task<int?> MoveToNode(int InstanseId, int nextWorkFlowNodeId,int trnsitionId,double? stepValue=null)
        {
            
            var itemToEdit = new WorkFlowStep
            {
                IsALive = true,
                WorkFlowNodeId = nextWorkFlowNodeId,
                WorkFlowInstanseId = InstanseId,
                TransitionId= trnsitionId,
                ReceiveDate = DateTime.UtcNow,
                StepValue = stepValue
            };
            var result = db.WorkFlowStepList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return null;
            return result.Entity.Id;
        }
        private async Task<bool> KillStep(int StepId)
        {
            var sourceStep = db.WorkFlowStepList.FirstOrDefault(t => t.Id == StepId && t.IsALive);
            if (sourceStep != null)
            {
                sourceStep.EndDate = DateTime.UtcNow;
                sourceStep.IsALive = false;
                await db.SaveChangesAsync(CancellationToken.None);
                return true;
            }
            return false;
        }
        private bool KillNode(int WorkFlowInstanseId,int WorkFlowNodeId)
        {
            lock(this)
            {
                var sourceSteps = db.WorkFlowStepList.Where(t => t.WorkFlowInstanseId == WorkFlowInstanseId && t.WorkFlowNodeId == WorkFlowNodeId).ToList();
                foreach(var  sourceStep in sourceSteps)
                {
                    sourceStep.EndDate = DateTime.UtcNow;
                    sourceStep.IsALive = false;
                    db.SaveChangesAsync(CancellationToken.None).Wait();
                }
                
            }
            
            return true;
        }
        private async Task<bool> HandleMoveToParallelGateWay(int stepId)
        {
            var sourceStep = db.WorkFlowStepList.FirstOrDefault(t => t.Id == stepId);
            if (sourceStep != null)
            {
                bool hasAliveNode = false;
                //check All input node recieved
                var sourceTransitionList = db.TransitionList.Where(t => t.NextWorkFlowNodeId == sourceStep.WorkFlowNodeId).AsNoTracking().ToList();
                if (sourceTransitionList != null && sourceTransitionList.Count > 1)
                {
                    var maxReach = 0;
                    foreach (var transition in sourceTransitionList)
                    {
                        var reachCount = db.WorkFlowStepList.Count(t => t.WorkFlowInstanseId == sourceStep.WorkFlowInstanseId && 
                        t.WorkFlowNodeId == sourceStep.WorkFlowNodeId && t.TransitionId == transition.Id);
                        if (maxReach < reachCount)
                        {
                            maxReach = reachCount;
                        }
                    }
                    foreach (var transition in sourceTransitionList)
                    {
                        var reachCount = db.WorkFlowStepList.Count(t => t.WorkFlowInstanseId == sourceStep.WorkFlowInstanseId && 
                        t.WorkFlowNodeId == sourceStep.WorkFlowNodeId && t.TransitionId == transition.Id);
                        if (maxReach != reachCount)
                        {
                            hasAliveNode = true;
                            break;
                        }
                    }
                }

                //move to all exited node
                if (!hasAliveNode)
                {
                    KillNode(sourceStep.WorkFlowInstanseId, sourceStep.WorkFlowNodeId);
                    var nextTransitionList = db.TransitionList.Where(t => t.SourceWorkFlowNodeId == sourceStep.WorkFlowNodeId).AsNoTracking().ToList();
                    if (nextTransitionList != null && nextTransitionList.Count > 0)
                    {
                        
                        foreach (var transition in nextTransitionList)
                        {
                            var nextStepId = await MoveToNode(sourceStep.WorkFlowInstanseId, transition.NextWorkFlowNodeId, transition.Id);
                            if (nextStepId != null)
                            {
                                await CheckNextNodeAndMoveIfNecessary(nextStepId.Value, string.Empty, null);
                            }
                            
                        }
                    }


                    return true;
                }
            }
           
            return false;
        }
        private async Task<bool> HandleMoveToExclusiveGateWay(int stepId, string command = "", double? stepValue = null)
        {
            var sourceStep = db.WorkFlowStepList.FirstOrDefault(t => t.Id == stepId);
            if (sourceStep != null)
            {
                var cmd=convertToCommand(command);
                var nextTransitionList = db.TransitionList.Where(t => t.SourceWorkFlowNodeId == sourceStep.WorkFlowNodeId).AsNoTracking().ToList();
                if (nextTransitionList != null && nextTransitionList.Count > 1)
                {
                    foreach (var transition in nextTransitionList)
                    {
                        if (transition.Command == Domain.Enums.TransitionCommandType.DecisionByValue)
                        {
                            if (transition.TransitionCondition == Domain.Enums.TransitionCondition.None
                               || (transition.TransitionCondition == Domain.Enums.TransitionCondition.None && transition.NodeValue.HasValue && stepValue.HasValue && transition.NodeValue.Value < stepValue.Value)
                               || (transition.TransitionCondition == Domain.Enums.TransitionCondition.Equal && transition.NodeValue.HasValue && stepValue.HasValue && transition.NodeValue.Value == stepValue.Value)
                               || (transition.TransitionCondition == Domain.Enums.TransitionCondition.GreaterThan && transition.NodeValue.HasValue && stepValue.HasValue && transition.NodeValue.Value > stepValue.Value)
                               || (transition.TransitionCondition == Domain.Enums.TransitionCondition.NotEqual && transition.NodeValue.HasValue && stepValue.HasValue && transition.NodeValue.Value != stepValue.Value))
                            {
                                await KillStep(stepId);
                                var nextStep = await MoveToNode(sourceStep.WorkFlowInstanseId, transition.NextWorkFlowNodeId, transition.Id, stepValue);
                                if (nextStep.HasValue)
                                    await CheckNextNodeAndMoveIfNecessary(nextStep.Value, transition.Command.ToString());

                                return true;
                            }

                        }
                        else if (cmd.HasValue && transition.Command == cmd)
                        {
                            await KillStep(stepId);
                            var nextStep = await MoveToNode(sourceStep.WorkFlowInstanseId, transition.NextWorkFlowNodeId, transition.Id, stepValue);
                            if (nextStep.HasValue)
                                await CheckNextNodeAndMoveIfNecessary(nextStep.Value, transition.Command.ToString(), null);

                            return true;
                        }

                    }
                }
                else if(nextTransitionList != null && nextTransitionList.Count == 1)
                {
                    await KillStep(stepId);
                    var nextStep = await MoveToNode(sourceStep.WorkFlowInstanseId, nextTransitionList[0].NextWorkFlowNodeId, nextTransitionList[0].Id, stepValue);
                    if (nextStep.HasValue)
                        await CheckNextNodeAndMoveIfNecessary(nextStep.Value, string.Empty, null);
                }
            }
            return false;
        }
        private Domain.Enums.TransitionCommandType? convertToCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;

            cmd = cmd.ToLowerInvariant();
            return cmd == "approve" ? Domain.Enums.TransitionCommandType.Approve : (cmd == "reject" ? Domain.Enums.TransitionCommandType.Reject : Domain.Enums.TransitionCommandType.Send);
        }
        private IQueryable<WorkFlowStepDto> GetWorkFlowStepQuery()
        {
            return from step in db.WorkFlowStepList
                   join instanse in db.WorkFlowInstanseList on step.WorkFlowInstanseId equals instanse.Id
                   join node in db.WorkFlowNodeList on step.WorkFlowNodeId equals node.Id
                   join lane in db.LaneList on node.LaneId equals lane.Id
                   join pool in db.PoolList on lane.PoolId equals pool.Id
                   select new WorkFlowStepDto
                   {
                       Id = step.Id,
                       PoolId = pool.Id,
                       PoolTitle = pool.Title,
                       IsALive = step.IsALive,
                       ReceiveDate = step.ReceiveDate,
                       StepValue = step.StepValue,
                       TransitionId = step.TransitionId,
                       WorkFlowInstanseId = step.WorkFlowInstanseId,
                       WorkFlowNodeId = step.WorkFlowNodeId,
                       WorkFlowNodeTitle = node.Title,
                       WorkFlowNodeType=node.WorkFlowNodeType,
                       EndDate = step.EndDate,
                       UserId = lane.RelatedUserId,


                   };
        }
        public async Task<IList<WorkFlowStepDto>> GetCurrentUserActivities(string userId)
        {
            return await GetWorkFlowStepQuery().Where(t=>t.IsALive && t.UserId == userId && t.WorkFlowNodeType==Domain.Enums.WorkFlowNodeType.Activity).ToListAsync();   

        }
        public WorkFlowStepDto? GetWorkFlowStepById(int StepId)
        {
            return GetWorkFlowStepQuery().FirstOrDefault(t => t.Id == StepId);

        }
        public async Task<IList<WorkFlowInstanseDto>> GetRelatedToUserWorkFlowInstanse(string userId)
        {
            var query=  from instanse in GetWorkFlowInstanseQuery()
                        join lane in db.LaneList on instanse.PoolId equals lane.PoolId
                        where lane.RelatedUserId== userId
                        select instanse;

            return await query.ToListAsync();
        }
    }
    public class WorkFlowDesignService
    {
        #region Private Fields
        readonly IApplicationDbContext db;

        public WorkFlowDesignService(IApplicationDbContext dbContext)
        {
            this.db = dbContext;
        }

        #endregion

        private IQueryable<PoolDto> GetPoolsQuery()
        {
            return from p in db.PoolList
                   select new PoolDto
                   {

                       Title = p.Title,
                       Id = p.Id,

                   };
        }
        public PoolDto? GetPoolById(int Id)
        {
            return GetPoolsQuery().FirstOrDefault(t => t.Id == Id);

        }

        public async Task<bool> UpdatePoolAsync(Models.PoolEditDto item)
        {
            var itemToEdit = db.PoolList.FirstOrDefault(t => t.Id == item.Id);
            if (itemToEdit != null)
            {

                itemToEdit.Title = item.Title;
                await db.SaveChangesAsync(CancellationToken.None);
            }
            return false;
        }
        public async Task<int> InsertPoolAsync(Models.PoolCreateDto item)
        {
            var itemToEdit = new Pool();
            itemToEdit.Title = item.Title;
            var result = db.PoolList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return 0;
            return result.Entity.Id;
        }
        public async Task<bool> RemovePoolByIdAsync(int Id)
        {
            try
            {
                var toDelete = db.PoolList.FirstOrDefault(t => t.Id == Id);
                if (toDelete != null)
                {
                    db.PoolList.Remove(toDelete);
                    await db.SaveChangesAsync(CancellationToken.None);
                    return true;
                }

            }
            catch
            {

            }
            return false;

        }


        private IQueryable<LaneDto> GetLanesQuery()
        {
            return from p in db.LaneList
                   select new LaneDto
                   {
                       PoolId = p.PoolId,
                       PoolTitle = p.Pool.Title,
                       Title = p.Title,
                       Id = p.Id,
                       RelatedRoleId = p.RelatedRoleId,
                       RelatedUserId = p.RelatedUserId,
                   };
        }
        public LaneDto? GetLaneById(int Id)
        {
            return GetLanesQuery().FirstOrDefault(t => t.Id == Id);

        }
        public async Task<List<LaneDto>> GetAllLanesAsync(int PoolId)
        {

            var result = GetLanesQuery().Where(t => t.PoolId == PoolId);
            var resultList = await result.OrderByDescending(t => t.Id).ToListAsync();
            return resultList;
        }
        public async Task<bool> UpdateLaneAsync(Models.LaneEditDto item)
        {
            var itemToEdit = db.LaneList.FirstOrDefault(t => t.Id == item.Id);
            if (itemToEdit != null)
            {

                itemToEdit.Title = item.Title;
                itemToEdit.RelatedRoleId = item.RelatedRoleId;
                itemToEdit.RelatedUserId = item.RelatedUserId;
                itemToEdit.PoolId = item.PoolId;
                await db.SaveChangesAsync(CancellationToken.None);
            }
            return false;
        }
        public async Task<int> InsertLaneAsync(Models.LaneCreateDto item)
        {
            var itemToEdit = new Lane();

            itemToEdit.Title = item.Title;
            itemToEdit.RelatedRoleId = item.RelatedRoleId;
            itemToEdit.RelatedUserId = item.RelatedUserId;
            itemToEdit.PoolId = item.PoolId;

            var result = db.LaneList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return 0;
            return result.Entity.Id;
        }
        public async Task<bool> RemoveLaneByIdAsync(int Id)
        {
            try
            {
                var toDelete = db.LaneList.FirstOrDefault(t => t.Id == Id);
                if (toDelete != null)
                {
                    db.LaneList.Remove(toDelete);
                    await db.SaveChangesAsync(CancellationToken.None);
                    return true;
                }

            }
            catch
            {

            }
            return false;

        }


        private IQueryable<WorkFlowNodeDto> GetWorkFlowNodesQuery()
        {
            return from p in db.WorkFlowNodeList
                   join l in db.LaneList on p.LaneId equals l.Id
                   select new WorkFlowNodeDto
                   {
                       PoolId=l.Pool.Id,
                       PoolTitle = l.Pool.Title,
                       LaneId = l.Id,
                       LaneTitle = l.Title,
                       WorkFlowNodeType = p.WorkFlowNodeType,
                       Title = p.Title,
                       Id = p.Id,
                       

                   };
        }
        public WorkFlowNodeDto? GetWorkFlowNodeById(int Id)
        {
            return GetWorkFlowNodesQuery().FirstOrDefault(t => t.Id == Id);

        }
        public async Task<List<WorkFlowNodeDto>> GetAllWorkFlowNodesAsync(int LaneId)
        {

            var result = GetWorkFlowNodesQuery().Where(t => t.LaneId == LaneId);
            var resultList = await result.OrderByDescending(t => t.Id).ToListAsync();
            return resultList;
        }
        public async Task<bool> UpdateWorkFlowNodeAsync(Models.WorkFlowNodeEditDto item)
        {
            var itemToEdit = db.WorkFlowNodeList.FirstOrDefault(t => t.Id == item.Id);
            if (itemToEdit != null)
            {

                itemToEdit.Title = item.Title;
                itemToEdit.WorkFlowNodeType = item.WorkFlowNodeType;
                itemToEdit.LaneId = item.LaneId;

                await db.SaveChangesAsync(CancellationToken.None);
            }
            return false;
        }
        public async Task<int> InsertWorkFlowNodeAsync(Models.WorkFlowNodeCreateDto item)
        {
            var itemToEdit = new WorkFlowNode();

            itemToEdit.Title = item.Title;
            itemToEdit.WorkFlowNodeType = item.WorkFlowNodeType;

            itemToEdit.LaneId = item.LaneId;

            var result = db.WorkFlowNodeList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return 0;
            return result.Entity.Id;
        }
        public async Task<bool> RemoveWorkFlowNodeByIdAsync(int Id)
        {
            try
            {
                var toDelete = db.WorkFlowNodeList.FirstOrDefault(t => t.Id == Id);
                if (toDelete != null)
                {
                    db.WorkFlowNodeList.Remove(toDelete);
                    await db.SaveChangesAsync(CancellationToken.None);
                    return true;
                }

            }
            catch
            {

            }
            return false;

        }


       

        private IQueryable<TransitionDto> GetTransitionsQuery()
        {
            return from p in db.TransitionList
                   select new TransitionDto
                   {
                       SourceWorkFlowNodeId = p.SourceWorkFlowNodeId,
                       NextWorkFlowNodeId = p.NextWorkFlowNodeId,
                       Title = p.Title,
                       Id = p.Id,
                       Command = p.Command,
                       NodeValue = p.NodeValue,
                       TransitionCondition = p.TransitionCondition,
                   };
        }
        public TransitionDto? GetTransitionById(int Id)
        {
            return GetTransitionsQuery().FirstOrDefault(t => t.Id == Id);

        }
        public async Task<List<TransitionDto>> GetAllTransitionsToNodeAsync(int WorkFlowNodeId)
        {

            var result = GetTransitionsQuery().Where(t => t.NextWorkFlowNodeId == WorkFlowNodeId);
            var resultList = await result.OrderByDescending(t => t.Id).ToListAsync();
            return resultList;
        }
        public async Task<List<TransitionDto>> GetAllTransitionsFromNodeAsync(int WorkFlowNodeId)
        {

            var result = GetTransitionsQuery().Where(t => t.SourceWorkFlowNodeId == WorkFlowNodeId);
            var resultList = await result.OrderByDescending(t => t.Id).ToListAsync();
            return resultList;
        }
        public async Task<bool> UpdateTransitionAsync(Models.TransitionEditDto item)
        {
            var itemToEdit = db.TransitionList.FirstOrDefault(t => t.Id == item.Id);
            if (itemToEdit != null)
            {

                itemToEdit.Title = item.Title;
                itemToEdit.NextWorkFlowNodeId = item.NextWorkFlowNodeId;
                itemToEdit.SourceWorkFlowNodeId = item.SourceWorkFlowNodeId;
                itemToEdit.NodeValue = item.NodeValue;
                itemToEdit.Command = item.Command;
                itemToEdit.TransitionCondition = item.TransitionCondition;
                await db.SaveChangesAsync(CancellationToken.None);
            }
            return false;
        }
        public async Task<int> InsertTransitionAsync(Models.TransitionCreateDto item)
        {
            var itemToEdit = new Transition();

            itemToEdit.Title = item.Title;
            itemToEdit.NextWorkFlowNodeId = item.NextWorkFlowNodeId;
            itemToEdit.SourceWorkFlowNodeId = item.SourceWorkFlowNodeId;
            itemToEdit.NodeValue = item.NodeValue;
            itemToEdit.Command = item.Command;
            itemToEdit.TransitionCondition = item.TransitionCondition;

            var result = db.TransitionList.Add(itemToEdit);
            await db.SaveChangesAsync(CancellationToken.None);
            if (result == null)
                return 0;
            return result.Entity.Id;
        }
        public async Task<bool> RemoveTransitionByIdAsync(int Id)
        {
            try
            {
                var toDelete = db.TransitionList.FirstOrDefault(t => t.Id == Id);
                if (toDelete != null)
                {
                    db.TransitionList.Remove(toDelete);
                    await db.SaveChangesAsync(CancellationToken.None);
                    return true;
                }

            }
            catch
            {

            }
            return false;

        }

    }
}
