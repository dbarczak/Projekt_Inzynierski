using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class QuestionDto
    {
        public int ExamPart { get; set; }
        public int QuestionNumber { get; set; }
        public string Title { get; set; }
        public string Knowledge { get; set; }
        public string Understanding { get; set; }
        public string Discussion { get; set; }
    }
}
