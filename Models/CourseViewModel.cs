using E_LearningProject.Entities;

namespace E_LearningProject.Models
{
    public class CourseViewModel
    {
        public Courses Course { get; set; }  // Stores course details
        public string CourseDescription { get; set; }  // Stores course description from the Word file
        public List<QuizQuestions>? Questions { get; set; }  // Stores quiz questions for the course
    }
}
