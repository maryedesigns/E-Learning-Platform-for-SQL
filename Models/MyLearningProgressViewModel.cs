namespace E_LearningProject.Models
{
    public class MyLearningProgressViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int Progress { get; set; }

    }
}
