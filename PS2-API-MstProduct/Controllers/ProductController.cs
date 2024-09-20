using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PS2_DAL.Models;
using PS2_DAL.Models.ViewModel;
using PS2_DAL.Repositories.IRepository;

namespace PS2_API_MstProduct.Controllers
{
    [ApiController]
    [Route("rest/v1/[controller]/[action]")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }

        [HttpGet]
        public IActionResult GetAllProduct()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return Json(objProductList);
        }

        [HttpGet]
        public IActionResult GetProductById(int id)
        {
            Product companyObj = _unitOfWork.Product.Get(u => u.Id == id);
            return Json(new { status = "200", message = "Success", data = companyObj });
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                _unitOfWork.Save();
                return Json(new { status = "200", message = "Success create" });
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return Json(new { status = "200", message = "Success"});
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // delete the old image
            var oldImagePath =
                Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Delete(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
    }
}
