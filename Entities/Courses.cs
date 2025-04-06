using System.ComponentModel.DataAnnotations;

namespace E_LearningProject.Entities
{
    public class Courses
    {
        public int Id { get; set; }
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Word Document Path")]
        public string? WordDocumentPath { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "You Tube URL")]
        public string YouTubeUrl { get; set; }

        public int TotalLessons { get; set; } = 10;  // Default is 10 lessons

        public ICollection<QuizQuestions> ? QuizQuestions { get; set; }  // Link to quiz questions


        [Display(Name = "Course Details")]
        public string CourseDescription { get; set; }

        [Display(Name = "What you will Learn")]
        public string WhatYouWillLearn { get; set; }

        [Display(Name = "Who Should Take This Course")]
        public string WhoShouldTakeThisCourse { get; set; }

    }
}
