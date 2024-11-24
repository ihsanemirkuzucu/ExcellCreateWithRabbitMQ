using ExcellCreateWithRabbitMQ.Shared;
using ExcellCreateWithRabbitMQ.Web.Models;
using ExcellCreateWithRabbitMQ.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExcellCreateWithRabbitMQ.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RabbitMQPublisher _rabbitMqPublisher;
        private readonly AppDbContext _context;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext context, RabbitMQPublisher rabbitMqPublisher)
        {
            _userManager = userManager;
            _context = context;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcell()
        {
            var currenUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var fileName = $"product-excel-{Guid.NewGuid().ToString()}";
            UserFile userFile = new()
            {
                UserId = currenUser.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();

            _rabbitMqPublisher.Publish(new CreateExcellMessage(){FileId = userFile.Id});

            TempData["StartCreatingExcell"] = true;
            return RedirectToAction(nameof(Files));
        }


        public async Task<IActionResult> Files()
        {
            var currenUser = await _userManager.FindByNameAsync(User.Identity.Name);
            var data = await _context.UserFiles.Where(x => x.UserId == currenUser.Id).OrderByDescending(x=>x.Id).ToListAsync();
            return View(data);
        }


    }
}
