using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SalesWebMvc.Models;
using SalesWebMvc.Models.ViewModels;
using SalesWebMvc.Services;
using SalesWebMvc.Services.Exceptions;

namespace SalesWebMvc.Controllers
{
    public class SellersController : Controller
    {
        private readonly SellerService _sellerService;
        private readonly DepartmentService _departmentService;

        public SellersController(SellerService sellerService, DepartmentService departmentService)
        {
            _sellerService = sellerService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _sellerService.FindAllAsync();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {

            return  View(await CreateSellerFormViewModel(new Seller { }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Seller seller)
        {
            if (!ModelState.IsValid)
            {
                return View(seller);
            }
            await _sellerService.InsertAsync(seller);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {

            var result = await ValidateIdSeller(id);
            if (!(result is Seller))
            {
                return result;
            }

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try {
                await _sellerService.RemoveAsync(id);
                return RedirectToAction(nameof(Index)); 
            }
            catch (IntegrityException e) { 
                return RedirectToAction(nameof(Error), new { message = e.Message });
            }
        }

        public async Task<IActionResult> Details(int? id)
        { //dynamic para retornar mais de um valor
            var result = await ValidateIdSeller(id);
            if (!(result is Seller))
            {
                return result;
            }
            return View(result);

        }

        public async Task<IActionResult> Edit(int? id)
        { //dynamic para retornar mais de um valor
            var obj = await ValidateIdSeller(id);

            if (!(obj is Seller))
            {
                return obj;
            }
            return View(CreateSellerFormViewModel(obj));

        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(int id, Seller seller)
        { //dynamic para retornar mais de um valor
            if (id != seller.Id)
            {
                return RedirectToAction(nameof(Error), new { message = "Id mismatch" });
            }
            try
            {
                await _sellerService.UpdateAsync(seller);
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException e)
            {
                return RedirectToAction(nameof(Error), new { message = e.Message });
            }
            catch (DBConcurrencyException e)
            {
                return RedirectToAction(nameof(Error), new { message = e.Message });
            }
            catch (ApplicationException e)
            {
                return RedirectToAction(nameof(Error), new { message = e.Message });
            }
        }


        private async Task<SellerFormViewModel> CreateSellerFormViewModel(Seller obj)
        {
            List<Department> deparments = await _departmentService.FindAllAsync();

            if (obj != null)
            {
                SellerFormViewModel viewModel = new SellerFormViewModel { Seller = obj, Departments = deparments };
                return viewModel;
            }
            else { 
                SellerFormViewModel viewModel = new SellerFormViewModel {Departments = deparments };
                return viewModel;
            }
        }
        private async Task<dynamic> ValidateIdSeller(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id Not Provided" });
            }
            var obj = await _sellerService.FindByIdAsync(id.Value);
            if (obj == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Id Not Found" });
            }
            return obj;
        }

        public IActionResult Error(string message)
        {
            var viewModel = new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(viewModel);
        }

    }
}