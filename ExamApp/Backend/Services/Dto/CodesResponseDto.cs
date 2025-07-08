using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class CodesResponseDto
    {
        public string StudentCode { get; set; }

        public List<string> ExaminerCodes { get; set; }
    }
}
