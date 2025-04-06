using E_LearningProject.Entities;
using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Models
{
    public class QuizViewModel
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }

        [Display(Name = "Question Text")]
        [Required(ErrorMessage = "Question Text is required.")]
        public string QuestionText { get; set; }

        [Display(Name = "Option A")]

        [Required(ErrorMessage = "Option A is required.")]
        public string OptionA { get; set; }

        [Display(Name = "Option B")]

        [Required(ErrorMessage = "Option B is required.")]
        public string OptionB { get; set; }

        [Display(Name = "Option C")]

        [Required(ErrorMessage = "Option C is required.")]
        public string OptionC { get; set; }

        [Display(Name = "Correct Answer")]

        [Required(ErrorMessage = "Correct Answer is required.")]
        public string CorrectAnswer { get; set; }

        public List<QuizQuestions>? Questions { get; set; }
    }

}
