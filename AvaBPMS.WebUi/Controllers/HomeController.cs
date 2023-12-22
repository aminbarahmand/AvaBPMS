using AvaBPMS.Application;
using AvaBPMS.WebUi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AvaBPMS.WebUi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WorkFlowDesignService _workFlowDesignService;
        public HomeController(ILogger<HomeController> logger, WorkFlowDesignService workFlowDesignService)
        {
            _logger = logger;
            _workFlowDesignService = workFlowDesignService;
        }

        public async Task<IActionResult> Index()
        {
            //await CreateTestWorkflow();
            //await CreateComplexWorkflow();
            return View();
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task CreateTestWorkflow()
        {
            var poolId = await _workFlowDesignService.InsertPoolAsync(new Application.Models.PoolCreateDto { Title = "Test WorkFlow" });
            var lane1Id = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "User1", RelatedRoleId = "User1", RelatedUserId = "User1" });
            
            var start=await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId= lane1Id,Title="Start",WorkFlowNodeType=Domain.Enums.WorkFlowNodeType.Start });
            var startActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = lane1Id, Title = "Request", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            var startTransition = await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId=startActivity,
                SourceWorkFlowNodeId=start,
                NodeValue=null,
            });

            var lane2Id = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "User2", RelatedRoleId = "User2", RelatedUserId = "User2" });
            var checkActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = lane2Id, Title = "Check", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            var sendToCheckTransition = await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = checkActivity,
                SourceWorkFlowNodeId = startActivity,
                NodeValue = null,
            });

            var approveRejectGate = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto
            {
                LaneId = lane2Id,
                Title = "?",
                WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.ExclusiveGateWay
            });
            var sendToApproveRejectGate = await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = approveRejectGate ,
                SourceWorkFlowNodeId = checkActivity,
                NodeValue = null,
            });
            var end = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = lane2Id, Title = "End", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.End });
            var approveTransition = await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = "Approve",
                Command = Domain.Enums.TransitionCommandType.Approve,
                TransitionCondition = Domain.Enums.TransitionCondition.Equal,
                NextWorkFlowNodeId = end,
                SourceWorkFlowNodeId =approveRejectGate ,
                NodeValue = 1,
            });
            var rejectTransition = await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = "Send To Edit",
                Command = Domain.Enums.TransitionCommandType.Reject,
                TransitionCondition = Domain.Enums.TransitionCondition.Equal,
                NextWorkFlowNodeId = startActivity,
                SourceWorkFlowNodeId = approveRejectGate,
                NodeValue = 0,
            });


        }


        private async Task CreateComplexWorkflow()
        {
            var poolId = await _workFlowDesignService.InsertPoolAsync(new Application.Models.PoolCreateDto { Title = "Complex WorkFlow" });
            
            var laneDccId = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "Dcc", RelatedRoleId = "Dcc", RelatedUserId = "Dcc" });
            var laneD1Id = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "D1", RelatedRoleId = "D1", RelatedUserId = "D1" });
            var laneD2Id = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "D2", RelatedRoleId = "D2", RelatedUserId = "D2" });
            var laneD3Id = await _workFlowDesignService.InsertLaneAsync(new Application.Models.LaneCreateDto { PoolId = poolId, Title = "D3", RelatedRoleId = "D3", RelatedUserId = "D3" });

            var start = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneDccId, Title = "Start Workflow", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Start });
            var startActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneDccId, Title = "Start", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = startActivity,
                SourceWorkFlowNodeId = start,
                NodeValue = null,
            });


            var initActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneD1Id, Title = "Init Document", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = initActivity,
                SourceWorkFlowNodeId = startActivity,
                NodeValue = null,
            });

            var sendToOthersDGate = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto
            {
                LaneId = laneD1Id,
                Title = "Send To D2 And D3",
                WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.ParallelGateWay
            });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = sendToOthersDGate,
                SourceWorkFlowNodeId = initActivity,
                NodeValue = null,
            });
            var d2CheckActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneD2Id, Title = "D2 Check", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = d2CheckActivity,
                SourceWorkFlowNodeId = sendToOthersDGate,
                NodeValue = null,
            });
            var d3CheckActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneD3Id, Title = "D3 Check", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = d3CheckActivity,
                SourceWorkFlowNodeId = sendToOthersDGate,
                NodeValue = null,
            });
            var getFromOthersDGate = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto
            {
                LaneId = laneD1Id,
                Title = "Get From D2 And D3",
                WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.ParallelGateWay
            });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = getFromOthersDGate,
                SourceWorkFlowNodeId =d2CheckActivity ,
                NodeValue = null,
            });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = getFromOthersDGate,
                SourceWorkFlowNodeId = d3CheckActivity,
                NodeValue = null,
            });

            var secondCheckActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneD1Id, Title = "Check D1,D2 Comment", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = secondCheckActivity ,
                SourceWorkFlowNodeId = getFromOthersDGate,
                NodeValue = null,
            });


            var dccCheckActivity = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneDccId, Title = "Final Dcc Check", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.Activity });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = dccCheckActivity,
                SourceWorkFlowNodeId = secondCheckActivity,
                NodeValue = null,
            });

            var approveRejectGate = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto
            {
                LaneId = laneDccId,
                Title = "?",
                WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.ExclusiveGateWay
            });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = string.Empty,
                Command = Domain.Enums.TransitionCommandType.Send,
                TransitionCondition = Domain.Enums.TransitionCondition.None,
                NextWorkFlowNodeId = approveRejectGate,
                SourceWorkFlowNodeId = dccCheckActivity,
                NodeValue = null,
            });
            var end = await _workFlowDesignService.InsertWorkFlowNodeAsync(new Application.Models.WorkFlowNodeCreateDto { LaneId = laneDccId, Title = "End", WorkFlowNodeType = Domain.Enums.WorkFlowNodeType.End });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = "Approve",
                Command = Domain.Enums.TransitionCommandType.Approve,
                TransitionCondition = Domain.Enums.TransitionCondition.Equal,
                NextWorkFlowNodeId = end,
                SourceWorkFlowNodeId = approveRejectGate,
                NodeValue = 1,
            });
            await _workFlowDesignService.InsertTransitionAsync(new Application.Models.TransitionCreateDto
            {
                Title = "Send To Edit",
                Command = Domain.Enums.TransitionCommandType.Reject,
                TransitionCondition = Domain.Enums.TransitionCondition.Equal,
                NextWorkFlowNodeId = initActivity,
                SourceWorkFlowNodeId = approveRejectGate,
                NodeValue = 0,
            });


        }
    }
}
