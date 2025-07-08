using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model;
using Services.Dto;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Services
{
    public class QuestionService
    {
        private readonly QuestionContext _context;
        private readonly Random _rng = new Random();

        private readonly ConcurrentDictionary<(int setId, int block), List<Questions>> _store
            = new ConcurrentDictionary<(int, int), List<Questions>>();

        public QuestionService(QuestionContext context)
        {
            _context = context;
        }

        public List<int> GetAvailableBlocks(int setId)
        {
            return _context.Questions
                           .Where(q=>q.QuestionSetId == setId)
                           .Select(q => q.ExamPart)
                           .Distinct()
                           .OrderBy(x => x)
                           .ToList();
        }

        public List<QuestionSet> GetAllSets()
        {
            return _context.QuestionSets
                           .OrderBy(s=>s.Name)
                           .ToList();
        }
        private List<int> GenerateNumbers(int setId, int examPart, int count)
        {
            var total = _context.Questions
                                .Count(q => q.QuestionSetId == setId && q.ExamPart == examPart);

            if (total <= count)
                return Enumerable.Range(1, total).ToList();

            var set = new HashSet<int>();
            while (set.Count < count)
            {
                int num = _rng.Next(1, total + 1);
                set.Add(num);
            }

            return set.ToList();
        }

        public List<Questions> GetRandomQuestions(int setId, int examPart, int count = 3)
        {
            var numbers = GenerateNumbers(setId, examPart, count);

            var result = _context.Questions
                          .Where(q => q.QuestionSetId == setId && 
                                 q.ExamPart == examPart &&
                                 numbers.Contains(q.QuestionNumber))
                          .ToList();

            return result;
        }

        public List<Questions> GenerateAndStore(int setId, int block, int count)
        {
            var list = GetRandomQuestions(setId, block, count);
            _store[(setId, block)] = list;
            return list;
        }

        public List<Questions> GetStored(int setId, int block)
        {
            return _store.TryGetValue((setId, block), out var list)
                 ? list
                 : new List<Questions>();
        }

        public void ClearAll()
        {
            _store.Clear();
        }

        public static List<Questions> ReadQuestionsFromFile(string[] lines)
        {
            var questions = new List<Questions>();
            Questions currentQuestion = null;
            int currentExamPart = 0;
            int lineIndex = 0;
            while (lineIndex < lines.Length)
            {
                var line = lines[lineIndex].Trim();
                if (Regex.IsMatch(line, @"^BLOK\s+[IVX]+", RegexOptions.IgnoreCase))
                {
                    var roman = Regex.Match(line, @"[IVX]+", RegexOptions.IgnoreCase).Value;
                    currentExamPart = RomanToInt(roman);
                    lineIndex++;
                    continue;
                }
                if (Regex.IsMatch(line, @"^\d+\."))
                {
                    if (currentQuestion != null) questions.Add(currentQuestion);
                    currentQuestion = new Questions
                    {
                        ExamPart = currentExamPart,
                        QuestionNumber = int.Parse(line.Split('.')[0]),
                        Title = line.Substring(line.IndexOf('.') + 1).Trim()
                    };
                    lineIndex++;
                    var parts = new List<string>();
                    while (lineIndex < lines.Length 
                        && !Regex.IsMatch(lines[lineIndex].Trim(), @"^\d+\.") 
                        && !Regex.IsMatch(lines[lineIndex].Trim(), @"^BLOK\s+[IVX]+"))
                    {
                        if (!string.IsNullOrWhiteSpace(lines[lineIndex]))
                            parts.Add(lines[lineIndex].Trim());
                        lineIndex++;
                    }
                    var relevantParts = parts
                        .Where(p => p.StartsWith("-")).ToList();
                    currentQuestion.Knowledge = relevantParts.ElementAtOrDefault(0)?.Substring(1).Trim();
                    currentQuestion.Understanding = relevantParts.ElementAtOrDefault(1)?.Substring(1).Trim();
                    currentQuestion.Discussion = relevantParts.ElementAtOrDefault(2)?.Substring(1).Trim();
                }
                else
                {
                    lineIndex++;
                }
            }
            if (currentQuestion != null) questions.Add(currentQuestion);
            return questions;
        }

        private static int RomanToInt(string roman)
        {
            roman = roman.ToUpper();
            var map = new Dictionary<char, int>
            {
                ['I'] = 1,
                ['V'] = 5,
                ['X'] = 10
            };

            int result = 0;
            for (int i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && map[roman[i]] < map[roman[i + 1]])
                    result -= map[roman[i]];
                else
                    result += map[roman[i]];
            }

            return result;
        }

        public bool AddQuestions(AddQuestionDto questionsDto)
        {
            if (questionsDto.File == null || questionsDto.File.Length == 0 || string.IsNullOrWhiteSpace(questionsDto.SetName))
                return false;

            var set = _context.QuestionSets
                        .FirstOrDefault(s => s.Name == questionsDto.SetName)
                      ?? new QuestionSet { Name = questionsDto.SetName, CreatedAt = DateTime.Now };

            if (set.Id == 0)
            {
                _context.QuestionSets.Add(set);
                _context.SaveChanges();
            }

            string[] lines;
            using (var reader = new StreamReader(questionsDto.File.OpenReadStream()))
                lines = reader.ReadToEnd()
                              .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var questions = ReadQuestionsFromFile(lines);
            questions.ForEach(q => q.QuestionSetId = set.Id);

            if (questions.Any())
            {
                _context.Questions.AddRange(questions);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        public bool DeleteQuestionSet(int setId)
        {
            var set = _context.QuestionSets.Find(setId);
            if (set == null) return false;

            _context.QuestionSets.Remove(set);
            _context.SaveChanges();
            return true;
        }

        public QuestionSetDetailDto GetSetDetails(int setId)
        {
            var set = _context.QuestionSets
                      .Include(s => s.Questions)
                      .FirstOrDefault(s => s.Id == setId);
            if (set == null) return null;

            return new QuestionSetDetailDto
            {
                Name = set.Name,
                CreatedAt = set.CreatedAt,
                Questions = set.Questions
                    .OrderBy(q => q.ExamPart)
                    .ThenBy(q => q.QuestionNumber)
                    .Select(q => new QuestionDto
                    {
                        ExamPart = q.ExamPart,
                        QuestionNumber = q.QuestionNumber,
                        Title = q.Title,
                        Knowledge = q.Knowledge,
                        Understanding = q.Understanding, 
                        Discussion = q.Discussion
                    })
                    .ToList()
            };
        }
    }
}
