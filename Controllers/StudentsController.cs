using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace DotNetCoreSqlDb.Controllers
{
    [ActionTimerFilter]
    public class StudentsController : Controller
    {
        private readonly ILogger<StudentsController> _logger;
        private readonly MyDatabaseContext _context;
        private readonly IDistributedCache _cache;
        private readonly string _StudentItemsCacheKey = "StudentsList";

        public StudentsController(MyDatabaseContext context, IDistributedCache cache, ILogger<StudentsController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // GET: Todos
        // The cache logic is added with the help of GitHub Copilot
        public async Task<IActionResult> Index()
        {
            var studentItems = await _cache.GetAsync(_StudentItemsCacheKey);
            if (studentItems != null)
            {
                _logger.LogInformation("Data from cache.");
                var studentList = JsonConvert.DeserializeObject<List<Todo>>(Encoding.UTF8.GetString(studentItems));
                return View(studentList);
            }
            else
            {
                _logger.LogInformation("Data from database.");
                var studentList = await _context.Students.ToListAsync();
                var serializedStudentList = JsonConvert.SerializeObject(studentList);
                await _cache.SetAsync(_StudentItemsCacheKey, Encoding.UTF8.GetBytes(serializedStudentList));
                return View(studentList);
            }
        }

        // GET: Todos/Details/5
        // The cache logic is added with the help of GitHub Copilot
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _cache.GetAsync(GetStudentItemCacheKey(id));
            if (student != null)
            {
                _logger.LogInformation("Data from cache.");
                var studentItem = JsonConvert.DeserializeObject<Student>(Encoding.UTF8.GetString(student));
                return View(studentItem);
            }
            else
            {
                _logger.LogInformation("Data from database.");
                var studentItem = await _context.Students
                    .FirstOrDefaultAsync(m => m.ID == id);
                if (studentItem == null)
                {
                    return NotFound();
                }

                var serializedStudent = JsonConvert.SerializeObject(studentItem);
                await _cache.SetAsync(GetStudentItemCacheKey(id), Encoding.UTF8.GetBytes(serializedStudent));
                return View(studentItem);
            }
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todos/Create
        // The cache logic is added with the help of GitHub Copilot
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,GoogleMeetUrl")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();

                // Clear the todo items cache
                await _cache.RemoveAsync(_StudentItemsCacheKey);

                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Todos/Edit/5
        // The cache logic is added with the help of GitHub Copilot
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _cache.GetAsync(GetStudentItemCacheKey(id));
            if (student != null)
            {
                _logger.LogInformation("Data from cache.");
                var studentItem = JsonConvert.DeserializeObject<Student>(Encoding.UTF8.GetString(student));
                return View(studentItem);
            }
            else
            {
                _logger.LogInformation("Data from database.");
                var studentItem = await _context.Students.FindAsync(id);
                if (studentItem == null)
                {
                    return NotFound();
                }

                var serializedStudent = JsonConvert.SerializeObject(studentItem);
                await _cache.SetAsync(GetStudentItemCacheKey(id), Encoding.UTF8.GetBytes(serializedStudent));
                return View(studentItem);
            }
        }

        // POST: Todos/Edit/5
        // The cache logic is added with the help of GitHub Copilot
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,GoogleMeetUrl")] Student student)
        {
            if (id != student.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();

                    // Clear the todo item and todos list from the cache
                    await _cache.RemoveAsync(GetStudentItemCacheKey(id));
                    await _cache.RemoveAsync(_StudentItemsCacheKey);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Todos/Delete/5
        // The cache logic is added with the help of GitHub Copilot
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _cache.GetAsync(GetStudentItemCacheKey(id));
            if (student != null)
            {
                _logger.LogInformation("Data from cache.");
                var studentItem = JsonConvert.DeserializeObject<Student>(Encoding.UTF8.GetString(student));
                return View(studentItem);
            }
            else
            {
                _logger.LogInformation("Data from database.");
                var studentItem = await _context.Students
                    .FirstOrDefaultAsync(m => m.ID == id);
                if (studentItem == null)
                {
                    return NotFound();
                }

                var serializedStudent = JsonConvert.SerializeObject(studentItem);
                await _cache.SetAsync(GetStudentItemCacheKey(id), Encoding.UTF8.GetBytes(serializedStudent));
                return View(studentItem);
            }
        }

        // POST: Todos/Delete/5
        // The cache logic is added with the help of GitHub Copilot
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();

            // Clear the todo item and todos list from the cache
            await _cache.RemoveAsync(GetStudentItemCacheKey(id));
            await _cache.RemoveAsync(_StudentItemsCacheKey);

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }

        private string GetStudentItemCacheKey(int? id)
        {
            return $"{_StudentItemsCacheKey}_{id}";
        }
     }
}
