namespace E_LearningProject.Entities
{
    public class MyLearning
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Store the user's unique ID
        public int CourseId { get; set; } // Reference to the course
        public DateTime EnrolledDate { get; set; } = DateTime.Now;

        public Courses Course { get; set; }
        public string Status { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int Progress => (int)((double)CompletedLessons / TotalLessons * 100);
    }
}
