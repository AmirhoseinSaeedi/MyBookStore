using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Domain.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BookStore.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objCategoryList = _unitOfWork.Product.GetAll("Category").ToList(); 
            return View(objCategoryList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitOfWork.Category
                    .GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString(),
                    }),
                Product = new Product()
            };
            if(id==null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file!=null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\products");
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagesPath = Path.Combine(wwwRootPath,productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagesPath))
                        {
                            System.IO.File.Delete(oldImagesPath);
                        }
                    }
                    using(var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName;
                }
                if(productVM.Product.Id==0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
               
                return View(productVM);
            }
        } 

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objCategoryList = _unitOfWork.Product.GetAll("Category").ToList();
            return Json(new { data = objCategoryList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToDeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            else
            {
                var oldImagesPath = Path.Combine(_webHostEnvironment.WebRootPath, productToDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagesPath))
                {
                    System.IO.File.Delete(oldImagesPath);
                }
            }
            _unitOfWork.Product.Remove(productToDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Succesfull" });
        }
        #endregion
    }
}
