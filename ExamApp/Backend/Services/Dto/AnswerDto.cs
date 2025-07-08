using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class AnswerDto
    {
        public int QuestionNumber { get; set; }
        public bool KnowledgeChecked { get; set; }
        public bool UnderstandingChecked { get; set; }
        public bool DiscussionChecked { get; set; }
    }
}
