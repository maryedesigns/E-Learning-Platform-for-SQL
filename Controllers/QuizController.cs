using E_LearningProject.Entities;
using E_LearningProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_LearningProject.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quiz/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            // Return the view with course details
            var model = new QuizViewModel
            {
                CourseId = id,
                CourseName = course.CourseName
            };

            return View(model);
        }

        // Post: Save new quiz question
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newQuestion = new QuizQuestions
                {
                    QuestionText = model.QuestionText,
                    OptionA = model.OptionA,
                    OptionB = model.OptionB,
                    OptionC = model.OptionC,
                    CorrectAnswer = model.CorrectAnswer,
                    CourseId = model.CourseId
                    
                    
                };

                _context.QuizQuestions.Add(newQuestion);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Courses", new { id = model.CourseId }); // Redirect back to course details page
            }

            // If ModelState is not valid, pass the CourseName back to the View
            var course = await _context.Courses.FindAsync(model.CourseId);
            model.CourseName = course?.CourseName;

            return View(model);
        }
         
        // GET: Quiz Details for a Course
        public async Task<IActionResult> Details(int id)
        {
            var quizQuestions = await _context.QuizQuestions
                .Where(q => q.CourseId == id)
                .ToListAsync();

            if (quizQuestions == null || quizQuestions.Count == 0)
            {
                return NotFound("No quiz questions found for this course.");
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var model = new QuizViewModel
            {
                CourseId = id,
                CourseName = course.CourseName,
                Questions = quizQuestions
            };

            return View(model);
        }

    }
}
