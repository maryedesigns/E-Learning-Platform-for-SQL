using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using E_LearningProject.Entities;
using E_LearningProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;


namespace E_LearningProject.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var courses = _context.Courses.ToList(); 
            return View(courses);
        }

        // GET: Course/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseName,Description,ImageUrl,YouTubeUrl, CourseDescription, WhatYouWillLearn, WhoShouldTakeThisCourse")] Courses courses, IFormFile wordDocument, IFormFile imageFile)
        {
            // Ensure the Word document is uploaded
            if (wordDocument == null || wordDocument.Length == 0)
            {
                ModelState.AddModelError("WordDocumentPath", "You must upload a Word document.");
                return View(courses);
            }

            // Ensure the Image is uploaded
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("ImageUrl", "You must upload an image.");
                return View(courses);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Define paths for uploading files
                    var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    var uploadImg = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploadsImg");

                    // Ensure the upload directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }
                    if (!Directory.Exists(uploadImg))
                    {
                        Directory.CreateDirectory(uploadImg);
                    }


                    // Generate unique file names for Word document and image
                    var wordFileName = System.IO.Path.GetFileNameWithoutExtension(wordDocument.FileName) + "_" + Guid.NewGuid() + System.IO.Path.GetExtension(wordDocument.FileName);
                    var wordFilePath = System.IO.Path.Combine(uploadPath, wordFileName);

                    var imageFileName = System.IO.Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid() + System.IO.Path.GetExtension(imageFile.FileName);
                    var imageFilePath = System.IO.Path.Combine(uploadImg, imageFileName);

                    // Save Word document
                    using (var wordFileStream = new FileStream(wordFilePath, FileMode.Create))
                    {
                        await wordDocument.CopyToAsync(wordFileStream);
                    }

                    // Save Image file
                    using (var imageFileStream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(imageFileStream);
                    }

                    // After saving files, assign their paths to the model
                    courses.WordDocumentPath = "/uploads/" + wordFileName;  // Relative path for the Word document
                    courses.ImageUrl = "/uploadsImg/" + imageFileName;         // Relative path for the image

                    // Create and save the course to the database
                    var course = new Courses
                    {
                        CourseName = courses.CourseName,
                        Description = courses.Description,
                        ImageUrl = courses.ImageUrl,
                        YouTubeUrl = courses.YouTubeUrl,
                        CourseDescription = courses.CourseDescription,
                        WhatYouWillLearn = courses.WhatYouWillLearn,
                        WhoShouldTakeThisCourse = courses.WhoShouldTakeThisCourse,
                        WordDocumentPath = courses.WordDocumentPath  // Save Word document path here
                    };

                    _context.Add(course);
                    await _context.SaveChangesAsync();

                    var notificationMessage = $"A new course titled '{course.CourseName}' has been added!";

                    var users = await _userManager.Users.ToListAsync();  // Fetch all users from the database

                    foreach (var user in users)
                    {
                        var notification = new Notification
                        {
                            UserId = user.Id,
                            Message = notificationMessage,
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        };

                        _context.Notifications.Add(notification);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction("StartLearning", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving course: " + ex.Message);
                }
            }

            return View(courses); 
        }



        // GET: Courses/Details/{id}
        public IActionResult Details(int id)
        {
            var course = _context.Courses
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            string extractedDescription = "";
            if (!string.IsNullOrEmpty(course.WordDocumentPath))
            {
                string absolutePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.WordDocumentPath.TrimStart('/'));

                if (System.IO.File.Exists(absolutePath))
                {
                    extractedDescription = ConvertWordToHtml(absolutePath);
                }
            }

            var viewModel = new CourseViewModel
            {
                Course = course,
                CourseDescription = extractedDescription, 
                Questions = course.QuizQuestions?.ToList()
            };

            return View(viewModel);
        }

        private string ConvertWordToHtml(string wordFilePath)
        {
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(wordFilePath, false))
                {
                    StringBuilder htmlContent = new StringBuilder();
                    htmlContent.Append("<html><body>");

                    Body body = wordDoc.MainDocumentPart.Document.Body;
                    if (body != null)
                    {
                        foreach (var element in body.Elements())
                        {
                            if (element is Paragraph paragraph)
                            {
                                htmlContent.Append(ConvertParagraphToHtml(paragraph, wordDoc));
                            }
                            else if (element is Table table)
                            {
                                htmlContent.Append(ConvertTableToHtml(table, wordDoc));
                            }
                        }
                    }

                    htmlContent.Append("</body></html>");
                    return htmlContent.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Error reading Word document: " + ex.Message;
            }
        }

        private string ConvertTableToHtml(Table table, WordprocessingDocument wordDoc)
        {
            StringBuilder tableHtml = new StringBuilder(@"
        <table style='border-collapse: collapse; width: 100%; border: 1px solid black;'>
    ");
            foreach (DocumentFormat.OpenXml.Wordprocessing.TableRow row in table.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>())
            {
                tableHtml.Append(@"
            <tr style='border: 1px solid black;'>
        ");
                foreach (DocumentFormat.OpenXml.Wordprocessing.TableCell cell in row.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>())
                {
                    tableHtml.Append(@"
                <td style='border: 1px solid black; padding: 8px; text-align: center;'>
            ");
                    foreach (var cellContent in cell.Elements<Paragraph>())
                    {
                        tableHtml.Append(ConvertParagraphToHtml(cellContent, wordDoc));
                    }
                    tableHtml.Append("</td>");
                }
                tableHtml.Append("</tr>");
            }

            tableHtml.Append("</table>");
            return tableHtml.ToString();
        }

        private string ConvertParagraphToHtml(Paragraph paragraph, WordprocessingDocument wordDoc)
        {
            StringBuilder formattedText = new StringBuilder();

            List<string> imageTags = new List<string>();
            bool isItalicParagraph = false;

            var paragraphProperties = paragraph.ParagraphProperties;
            var numberingProperties = paragraphProperties?.NumberingProperties;
            bool isListItem = numberingProperties != null;
            string listTag = "ul";

            if (isListItem)
            {
                var numId = numberingProperties.NumberingId?.Val?.Value;
                var numPart = wordDoc.MainDocumentPart.NumberingDefinitionsPart;
                var abstractNumId = numPart?.Numbering.Elements<NumberingInstance>()
                    .FirstOrDefault(n => n.NumberID == numId)?.AbstractNumId?.Val?.Value;

                var abstractNum = numPart?.Numbering.Elements<AbstractNum>()
                    .FirstOrDefault(a => a.AbstractNumberId == abstractNumId);

                if (abstractNum?.Elements<Level>().FirstOrDefault()?.NumberingFormat?.Val == NumberFormatValues.Decimal)
                    listTag = "ol";
            }

            StringBuilder runTextBuilder = new StringBuilder();

            foreach (var run in paragraph.Elements<Run>())
            {
                string runText = run.InnerText;
                if (string.IsNullOrWhiteSpace(runText)) continue;

                var drawing = run.Elements<Drawing>().FirstOrDefault();
                if (drawing != null)
                {
                    string imagePartPath = ExtractImagePartPath(drawing, wordDoc);
                    if (!string.IsNullOrEmpty(imagePartPath))
                    {
                        byte[] imageBytes = ExtractImageAsBytes(wordDoc, imagePartPath);
                        if (imageBytes != null)
                        {
                            string base64Image = Convert.ToBase64String(imageBytes);
                            string mimeType = "image/png";
                            string imgTag = $"<div style='text-align: center; margin: 10px 0;'><img src='data:{mimeType};base64,{base64Image}' alt='Image' style='max-width: 90%; height: auto;' /></div>";
                            imageTags.Add(imgTag);
                        }
                    }
                }
                else
                {
                    if (run.RunProperties != null)
                    {
                        var color = run.RunProperties.Color?.Val;
                        var font = run.RunProperties.RunFonts?.Ascii;
                        var size = run.RunProperties.FontSize?.Val;

                        string style = "";
                        if (!string.IsNullOrEmpty(color)) style += $"color: #{color};";
                        if (!string.IsNullOrEmpty(font)) style += $"font-family: {font};";
                        if (!string.IsNullOrEmpty(size))
                        {
                            double pt = int.Parse(size) / 2.0;
                            style += $"font-size: {pt}pt;";
                        }

                        runText = $"<span style='{style}'>{runText}</span>";

                        if (run.RunProperties.Bold != null) runText = $"<b>{runText}</b>";
                        if (run.RunProperties.Italic != null)
                        {
                            isItalicParagraph = true;
                            runText = $"<i>{runText}</i>";
                        }
                        if (run.RunProperties.Underline != null) runText = $"<u>{runText}</u>";
                    }

                    runTextBuilder.Append(runText + " ");
                }
            }

            // Handle paragraph content (text or list)
            if (runTextBuilder.Length > 0)
            {
                string pStyle = isItalicParagraph ? " style='line-height:1.0;'" : "";

                if (isListItem)
                {
                    formattedText.Append($"<li{pStyle}>{runTextBuilder}</li>");
                }
                else
                {
                    formattedText.Append($"<p{pStyle}>{runTextBuilder}</p>");
                }
            }

            // Handle image display (center or side-by-side)
            if (imageTags.Count > 0)
            {
                if (imageTags.Count == 1)
                {
                    formattedText.Append($"<div style='text-align: center;'>{imageTags[0]}</div>");
                }
                else
                {
                    formattedText.Append("<div style='display: flex; flex-wrap: wrap; gap: 20px;'>");
                    foreach (var img in imageTags)
                    {
                        formattedText.Append(img);
                    }
                    formattedText.Append("</div>");
                }
            }

            return formattedText.ToString();
        }

        private string ExtractImagePartPath(Drawing drawing, WordprocessingDocument wordDoc)
        {
            var blip = drawing.Descendants<Blip>().FirstOrDefault();
            if (blip != null)
            {
                return blip.Embed?.Value;
            }
            return null;
        }
        private byte[] ExtractImageAsBytes(WordprocessingDocument wordDoc, string imagePartId)
        {
            if (imagePartId != null)
            {
                var imagePart = wordDoc.MainDocumentPart.GetPartById(imagePartId) as ImagePart;
                if (imagePart != null)
                {
                    using (Stream stream = imagePart.GetStream())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            return null;
        }

        [Authorize]
        public async Task<IActionResult> Enroll(int id)
        {

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            var existingEnrollment = await _context.MyLearnings
                .FirstOrDefaultAsync(m => m.UserId == userId && m.CourseId == id);

            if (existingEnrollment == null)
            {
                // Create a new learning record
                var enrollment = new MyLearning
                {
                    UserId = userId,
                    CourseId = id,
                    Status = "Not Started",
                    IsCompleted = false,
                    EnrolledDate = DateTime.Now,
                    TotalLessons = course.TotalLessons, 
                    CompletedLessons = 0
                };

                _context.MyLearnings.Add(enrollment);
                await _context.SaveChangesAsync();

                return RedirectToAction("MyLearning", "Account");
            }

            return RedirectToAction("MyLearning", "Account");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int courseId, int progress)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var learning = await _context.MyLearnings
                .FirstOrDefaultAsync(l => l.CourseId == courseId && l.UserId == userId);

            if (learning != null)
            {
                // ✅ If the user just clicked "Start Course"
                if (progress == 30 && learning.Progress == 0)
                {
                    learning.Status = "In Progress";
                    learning.CompletedLessons = 3;
                }

                // ✅ If the user has taken the quiz (Progress = 70%)
                else if (progress == 70)
                {
                    learning.Status = "In Progress";
                    learning.CompletedLessons = 7;
                }

                // ✅ If the user has marked the course as completed
                else if (progress == 100)
                {
                    learning.Status = "Completed";
                    learning.IsCompleted = true;
                    learning.CompletedLessons = learning.TotalLessons;
                }

                // ✅ Update and save changes
                _context.Update(learning);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    newStatus = learning.Status,
                    progress = learning.Progress
                });
            }

            return Json(new { success = false, message = "Course not found." });
        }

        public IActionResult CourseDetails(int id)
        {
            var course = _context.Courses
                .Where(c => c.Id == id)
                .Select(c => new CourseDetailsViewModel
                {
                    Course = c,
                    CourseName = c.CourseName,
                    CourseDescription = c.Description,
                    WhatYouWillLearn = c.WhatYouWillLearn,
                    WhoShouldTakeThisCourse = c.WhoShouldTakeThisCourse,
                    // Include other properties from the Course model as necessary
                })
                .FirstOrDefault();

            if (course == null)
            {
                return NotFound();  // Return a 404 page if the course doesn't exist
            }

            return View(course);  // Return the valid model to the view
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return View("SearchResults", new List<Courses>());
            }

            var results = _context.Courses
                .Where(c => c.CourseName.Contains(query) || c.Description.Contains(query))
                .ToList();

            ViewBag.Query = query;

            return View("SearchResults", results);
        }
    }

}


