using Microsoft.AspNetCore.Mvc;
using Salongsongan_ChelxieConsumeAPI.Models;
using System.Net.Http.Json;

namespace Salongsongan_ChelxieConsumeAPI.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string Endpoint = "api/Product";

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ProductApi");
        }

        // Part 6 – Display all products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _httpClient
                    .GetFromJsonAsync<List<Product>>(Endpoint);

                return View(products ?? new List<Product>());
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"Could not reach the API: {ex.Message}";
                return View(new List<Product>());
            }
        }

        // Part 9 – Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var response = await _httpClient.PostAsJsonAsync(Endpoint, product);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", $"Create failed: {response.StatusCode}");
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        // Part 11 – Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{Endpoint}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var product = await response.Content.ReadFromJsonAsync<Product>();

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var response = await _httpClient
                .PutAsJsonAsync($"{Endpoint}/{product.Id}", product);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", $"Update failed: {response.StatusCode}");
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        // Part 13 – Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{Endpoint}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Delete failed: {response.StatusCode}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Part 14 – Search
        [HttpGet]
        public async Task<IActionResult> Search(string name)
        {
            List<Product> products;

            try
            {
                products = await _httpClient
                    .GetFromJsonAsync<List<Product>>(Endpoint) ?? new List<Product>();
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"Could not reach the API: {ex.Message}";
                return View("Index", new List<Product>());
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                products = products
                    .Where(p => p.Name != null &&
                                p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (products.Count == 0)
            {
                ViewBag.Message = "No product found.";
            }

            return View("Index", products);
        }
    }
}