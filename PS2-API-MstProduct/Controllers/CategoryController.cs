using Microsoft.AspNetCore.Mvc;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PS2_API_MstProduct.Controllers
{
    [ApiController]
    [Route("rest/v1/[controller]/[action]")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategory()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();

            return Json(objCategoryList);
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                var errors = "The DisplayOrder cannot exactly match the Name.";
                ModelState.AddModelError("name", errors);
                return BadRequest(new { status = "400", message = errors });
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
            }
            return Json(new { status = "200", message = "Success" });
        }

        [HttpGet]
        public IActionResult GetCategoryById(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category? category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category == null) { return NotFound(); }
            return Json(category);
        }
        [HttpPost]
        public IActionResult Update(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
            }

            return Json(new { status = "200", message = "Success" });
        }

        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null) { return NotFound(); }

            _unitOfWork.Category.Delete(obj);
            _unitOfWork.Save();
            return Json(new { status = "200", message = "Success" });
        }
    }
}
