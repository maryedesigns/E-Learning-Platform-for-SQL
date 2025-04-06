namespace E_LearningProject.Entities
{
    public class QuizQuestions
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string CorrectAnswer { get; set; }  // Store "A", "B", or "C"
        public Courses Course { get; set; }  // Navigation property to the associated course

    }
}
