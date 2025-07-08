using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    [Table("Question")]
    public class Questions
    {
        [Key]
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public string Title { get; set; }
        public string Knowledge { get; set; }
        public string Understanding { get; set; }
        public string Discussion { get; set; }
        public int ExamPart { get; set; }
        public int QuestionSetId { get; set; }
        [ForeignKey("QuestionSetId")]
        public QuestionSet QuestionSet { get; set; }
    }
}
