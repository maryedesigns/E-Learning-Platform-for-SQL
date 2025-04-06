using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Entities
{
    public class UserQuizResults
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } // User's unique ID from AspNetUsers table

        public int QuestionId { get; set; }

        public QuizQuestions Question { get; set; }

        public int CourseId { get; set; }

        public Courses Course { get; set; }

        public string UserAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime AttemptDate { get; set; } = DateTime.Now;
    }
}
