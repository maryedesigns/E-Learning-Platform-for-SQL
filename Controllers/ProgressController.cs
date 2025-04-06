using E_LearningProject.Entities;
using E_LearningProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_LearningProject.Controllers
{
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Progress()
        {
            var userId = User.Identity.Name;

            var enrolledCourses = await _context.MyLearnings
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var progressList = enrolledCourses.Select(e => new MyLearningProgressViewModel
            {
                CourseId = e.CourseId,
                CourseName = e.Course.CourseName,
                TotalLessons = _context.QuizQuestions.Count(q => q.CourseId == e.CourseId),
                CompletedLessons = _context.MyLearnings.Count(r => r.UserId == userId && r.CourseId == e.CourseId),
                Progress = (_context.MyLearnings.Count(r => r.UserId == userId && r.CourseId == e.CourseId) * 100)
                           / (_context.QuizQuestions.Count(q => q.CourseId == e.CourseId) == 0 ? 1 : _context.QuizQuestions.Count(q => q.CourseId == e.CourseId))
            }).ToList();

            return View(progressList);
        }
    }


}
