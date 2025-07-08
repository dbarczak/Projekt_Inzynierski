using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class ExaminerAnswerDto
    {
        public int BlockNumber { get; set; }
        public string ExaminerCode { get; set; }
        public bool IsLeadExaminer { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
