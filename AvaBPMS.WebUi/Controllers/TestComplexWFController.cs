using AvaBPMS.Application;
using AvaBPMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AvaBPMS.WebUi.Controllers
{
    public class TestComplexWFController : Controller
    {
        private readonly int poolId = 2;
        private readonly WorkFlowExecutionService _workFlowExecutionService;
        private readonly WorkFlowDesignService _workFlowDesignService;
        private readonly IUser _user;
        public TestComplexWFController(WorkFlowDesignService workFlowDesignService, WorkFlowExecutionService workFlowExecutionService, IUser user)
        {
            _workFlowDesignService = workFlowDesignService;
            _workFlowExecutionService = workFlowExecutionService;
            _user = user;
        }

        public async Task<IActionResult> Index()
        {
            var wfInstanses = await _workFlowExecutionService.GetAllInstanses(poolId);
            return View(wfInstanses);
        }

        public async Task<IActionResult> CreateNewInstance()
        {
            var wfpool = _workFlowDesignService.GetPoolById(poolId);
            var instanseId = await _workFlowExecutionService.CreateWorkFlowInstanse(new Application.Models.WorkFlowInstanseCreateDto
            {
                AssignDate = DateTime.Now,
                PoolId = wfpool.Id,
                UserId = "Admin"
            });

            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> StartWorkFlow(int Id)
        {
            await _workFlowExecutionService.StartWorkFlowInstanse(Id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UserView(string UserId)
        {
            HttpContext.Session.Set<string>("UserId", UserId);
            var wfInstanses = await _workFlowExecutionService.GetRelatedToUserWorkFlowInstanse(UserId);
            return View(wfInstanses);
        }
        [HttpGet]
        public async Task<string> GetUserInboxList()
        {
            if (_user.Id != null)
            {
                var items = await _workFlowExecutionService.GetCurrentUserActivities(_user.Id);
                return await RenderViewToStringAsync(this, "_Inbox", items);
            }
            return await RenderViewToStringAsync(this, "_Inbox", null);
        }
        public async Task<IActionResult> HandleChangeStep(int StepId, string Command, double? StepValue = null)
        {

            await _workFlowExecutionService.MoveNext(StepId, Command, StepValue);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UserStep(int StepId)
        {
            var wfStep = _workFlowExecutionService.GetWorkFlowStepById(StepId);
            if (wfStep != null)
            {
                ViewBag.NodeTitle = wfStep.WorkFlowNodeTitle;
                return View(wfStep);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<string> GetWorkFlowCommands(int StepId)
        {
            var wfStep = _workFlowExecutionService.GetWorkFlowStepById(StepId);
            ViewBag.StepId = StepId;
            if (wfStep != null)
            {
                var transitionList = await _workFlowDesignService.GetAllTransitionsFromNodeAsync(wfStep.WorkFlowNodeId);
                if (transitionList != null && transitionList.Count == 1 && wfStep.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.Activity)
                {
                    var nextStep = _workFlowDesignService.GetWorkFlowNodeById(transitionList[0].NextWorkFlowNodeId);
                    if (nextStep != null && nextStep.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.ExclusiveGateWay)
                    {

                        transitionList = await _workFlowDesignService.GetAllTransitionsFromNodeAsync(nextStep.Id);
                    }
                    else if (nextStep != null && nextStep.WorkFlowNodeType == Domain.Enums.WorkFlowNodeType.ParallelGateWay)
                    {
                        var allResievers = await _workFlowDesignService.GetAllTransitionsFromNodeAsync(nextStep.Id);
                        transitionList[0].Title = " Send To All ";
                    }
                }

                return await RenderViewToStringAsync(this, "_WfCommand", transitionList.Select(t => t.Command.ToString()).ToList());

            }
            return await RenderViewToStringAsync(this, "_WfCommand", new List<string>());
        }
        private static IView FindView(Controller controller, string viewNamePath)
        {
            IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

            ViewEngineResult viewResult = null;

            if (viewNamePath.EndsWith(".cshtml"))
                viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
            else
                viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, false);

            if (!viewResult.Success)
            {
                var endPointDisplay = controller.HttpContext.GetEndpoint().DisplayName;

                if (endPointDisplay.Contains(".Areas."))
                {
                    //search in Areas
                    var areaName = endPointDisplay.Substring(endPointDisplay.IndexOf(".Areas.") + ".Areas.".Length);
                    areaName = areaName.Substring(0, areaName.IndexOf(".Controllers."));

                    viewNamePath = $"~/Areas/{areaName}/views/{controller.HttpContext.Request.RouteValues["controller"]}/{controller.HttpContext.Request.RouteValues["action"]}.cshtml";

                    viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
                }

                if (!viewResult.Success)
                    throw new Exception($"A view with the name '{viewNamePath}' could not be found");

            }

            return viewResult.View;
        }
        public static async Task<string> RenderViewToStringAsync(Controller controller, string viewNamePath, object model = null)
        {
            if (string.IsNullOrEmpty(viewNamePath))
                viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    var view = FindView(controller, viewNamePath);

                    ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        view,
                        controller.ViewData,
                        controller.TempData,
                        writer,
                        new HtmlHelperOptions()
                    );

                    await view.RenderAsync(viewContext);

                    return writer.GetStringBuilder().ToString();
                }
                catch (Exception exc)
                {
                    return $"Failed - {exc.Message}";
                }
            }
        }



    }
}
