using ExcelApp.Models;
using ExcelApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExcelApp.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly RabbitMQPublisher _rabbitMQPublisher;
        public ProductController(AppDbContext context, UserManager<IdentityUser> userManager, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            string fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userfile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await _context.UserFiles.AddAsync(userfile);
            await _context.SaveChangesAsync();

            _rabbitMQPublisher.Publish(new CreateExcelMessage() { FileId = userfile.Id });
            TempData["StartCreatingExcel"] = true;
            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            return View(await _context.UserFiles.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Id).ToListAsync());
        }
    }
}
