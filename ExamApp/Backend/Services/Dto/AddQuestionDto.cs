using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dto
{
    public class AddQuestionDto
    {
        public string SetName { get; set; }

        public IFormFile File { get; set; }
    }
}
