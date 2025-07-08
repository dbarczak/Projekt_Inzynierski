using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class BlockResultDto
    {
        public int KnowledgeCount { get; set; }
        public int UnderstandingCount { get; set; }
        public int DiscussionCount { get; set; }
        public bool Passed { get; set; }
        public double Grade { get; set; }
    }
}
