using BookStore.Domain.ViewModels;
using BookStore.Domain;
using BookStore.Infrastructure.Repository.IRepositpry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using BookStore.Utility;

namespace BookStore.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return View(companyList);
        }

        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null || id == 0)
            {
                //Create
                return View(company);
            }
            else
            {
                //Update
                company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index", "Company");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToDeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.Company.Remove(companyToDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Succesfull" });
        }
        #endregion
    }
}
