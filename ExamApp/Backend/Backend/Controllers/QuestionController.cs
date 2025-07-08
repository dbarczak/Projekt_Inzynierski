using Backend.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Model;
using Services;
using Services.Dto;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly EvaluationService _evaluationService;
        private readonly AccessService _accessService;
        private readonly QuestionService _questionService;
        private readonly IHubContext<ExamHub> _hub;

        public QuestionController(QuestionService questionService, EvaluationService evaluationService, AccessService accessService, IHubContext<ExamHub> hub)
        {
            _evaluationService = evaluationService;
            _accessService = accessService;
            _questionService = questionService;
            _hub = hub;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<List<Questions>>> Generate(
            [FromQuery] int setId,
            [FromQuery] int block,
            [FromQuery] int count = 3)
        {
            var questions = _questionService.GenerateAndStore(setId, block, count);

            await _hub.Clients.Group("exam")
              .SendAsync("ReceiveQuestions", questions);

            return Ok(questions);
        }

        [HttpGet("questions")]
        public ActionResult<List<Questions>> Get([FromQuery] int setId,
            [FromQuery] int block)
        {
            var questions = _questionService.GetStored(setId, block);
            if (!questions.Any()) return NotFound();
            return Ok(questions);
        }

        [HttpGet("sets")]
        public ActionResult<List<QuestionSet>> Sets()
        => Ok(_questionService.GetAllSets());


        [HttpGet("blocks")]
        public ActionResult<IEnumerable<int>> GetBlocks([FromQuery] int setId)
        {
            var blocks = _questionService.GetAvailableBlocks(setId);
            return Ok(blocks);
        }

        [HttpPost("add")]
        public IActionResult AddQuestions([FromForm] AddQuestionDto questions)
        {
            if (_questionService.AddQuestions(questions))
                return Ok("Pytania dodane pomyślnie.");
            return BadRequest("Brak pytań lub plik był pusty.");
        }

        [HttpDelete("delete")]
        public IActionResult DeleteSet([FromQuery] int setId)
        {
            if (_questionService.DeleteQuestionSet(setId))
                return NoContent();
            return NotFound($"Zestaw o ID={setId} nie istnieje.");
        }

        [HttpGet("sets/details")]
        public ActionResult<QuestionSetDetailDto> GetSetDetails([FromQuery] int setId)
        {
            var dto = _questionService.GetSetDetails(setId);
            if (dto == null) return NotFound($"Zestaw {setId} nie istnieje.");
            return Ok(dto);
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndExam()
        {
            _questionService.ClearAll();
            _evaluationService.ClearAll();

            await _hub.Clients.Group("exam").SendAsync("ExamEnded");
            return NoContent();
        }
    }
}
