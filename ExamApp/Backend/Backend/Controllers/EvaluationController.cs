using Backend.Hubs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;
using Services.Dto;

namespace Backend.Controllers
{
    [ApiController]
    [Route("Evaluation")]
    public class EvaluationController : Controller
    {
        private readonly EvaluationService _eval;
        private readonly IHubContext<ExamHub> _hub;

        public EvaluationController(
            EvaluationService eval,
            IHubContext<ExamHub> hub)
        {
            _eval = eval;
            _hub = hub;
        }

        [HttpPost("submit-examiner")]
        public async Task<IActionResult> Submit([FromBody] ExaminerAnswerDto dto)
        {
            _eval.AddOrUpdateAnswers(dto);
            var count = _eval.GetSubmissionCount(dto.BlockNumber);
            await _hub.Clients.Group("exam")
                .SendAsync("SubmissionCountUpdated", count);
            return Ok();
        }

        [HttpGet("submissions/count")]
        public ActionResult<int> Count([FromQuery] int blockNumber)
            => Ok(_eval.GetSubmissionCount(blockNumber));

        [HttpPost("evaluate-block")]
        public async Task<ActionResult<BlockResultDto>> Evaluate([FromBody] SubmitBlockDto dto)
        {
            var result = _eval.EvaluateBlock(dto.BlockNumber);
            _eval.ClearAnswers(dto.BlockNumber);
            await _hub.Clients.Group("exam").SendAsync("BlockEvaluated", result);
            return Ok(result);
        }

        [HttpGet("result")]
        public ActionResult<int> Result()
        { 
            return Ok(_eval.GetResult());
        }
    }
}
