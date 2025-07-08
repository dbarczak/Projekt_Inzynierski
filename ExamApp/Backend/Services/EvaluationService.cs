using Services.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class EvaluationService
    {
        private readonly ConcurrentDictionary<int, Dictionary<string, ExaminerAnswerDto>>
        _store = new ConcurrentDictionary<int, Dictionary<string, ExaminerAnswerDto>>();
        private BlockResultDto _result;


        public BlockResultDto GetResult()
        {
            return _result;
        }
        public void AddOrUpdateAnswers(ExaminerAnswerDto dto)
        {
            var dict = _store.GetOrAdd(dto.BlockNumber, _ => new());
            dict[dto.ExaminerCode] = dto;
        }

        public int GetSubmissionCount(int blockNumber)
        {
            return _store.TryGetValue(blockNumber, out var d) ? d.Count : 0;
        }

        private List<ExaminerAnswerDto> GetAllAnswers(int blockNumber)
        { 
           return _store.TryGetValue(blockNumber, out var d)
           ? d.Values.ToList()
           : new List<ExaminerAnswerDto>();
        }

        public BlockResultDto EvaluateBlock(int blockNumber)
        {
            var submissions = GetAllAnswers(blockNumber);
            var flat = submissions
              .SelectMany(ex => ex.Answers.Select(a => new {
                  ex.IsLeadExaminer,
                  a.QuestionNumber,
                  a.KnowledgeChecked,
                  a.UnderstandingChecked,
                  a.DiscussionChecked
              }))
              .ToList();
            var questionNumbers = flat
              .Select(x => x.QuestionNumber)
              .Distinct()
              .OrderBy(n => n)
              .ToList();
            int kn = 0, un = 0, di = 0;
            foreach (var qn in questionNumbers)
            {
                var grp = flat.Where(x => x.QuestionNumber == qn).ToList();
                int half = grp.Count / 2;
                var lead = grp.FirstOrDefault(x => x.IsLeadExaminer);
                bool Resolve(Func<dynamic, bool> sel)
                {
                    int yes = grp.Count(sel);
                    if (yes > half) return true;
                    if (yes < half) return false;
                    return lead != null && sel(lead);
                }
                if (Resolve(x => x.KnowledgeChecked)) kn++;
                if (Resolve(x => x.UnderstandingChecked)) un++;
                if (Resolve(x => x.DiscussionChecked)) di++;
            }
            bool passed = kn == questionNumbers.Count && un >= 2;
            int pts = kn + un + di;
            double grade = passed
              ? 3.0 + (pts - 5) * 0.5
              : 2.0;
            _result = new BlockResultDto
            {
                KnowledgeCount = kn,
                UnderstandingCount = un,
                DiscussionCount = di,
                Passed = passed,
                Grade = Math.Round(grade, 1)
            };
            return _result;
        }

        public void ClearAnswers(int blockNumber)
        {
            _store.TryRemove(blockNumber, out _);
            _result = null;
        }

        public void ClearAll()
        {
            _store.Clear();
            _result = null;
        }
    }
}
