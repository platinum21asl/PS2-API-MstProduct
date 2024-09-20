using Microsoft.AspNetCore.Mvc;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PS2_API_MstProduct.Controllers
{
    [ApiController]
    [Route("rest/v1/[controller]/[action]")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        [HttpGet]
        public IActionResult GetAllCompany()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

            return Json(objCompanyList);
        }
        [HttpGet]
        public IActionResult GetCompanyById(int id) {
         
            Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);
            return Json(new { status = "200", message = "Success", data = companyObj });
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                return Json(new { success = true, message = "Success" });
            }
            else
            {
                return BadRequest(new { status = "400", message = "Failed" });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Company.Get(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Delete(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
    }
}
