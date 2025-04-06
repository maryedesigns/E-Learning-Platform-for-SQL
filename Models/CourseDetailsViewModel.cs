using E_LearningProject.Entities;

namespace E_LearningProject.Models
{
    public class CourseDetailsViewModel
    {
        public Courses Course { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string WhoShouldTakeThisCourse { get; set; }
    }
}
