using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RealEstate.Mvc.Model;

namespace RealEstate.Mvc.Controllers
{
    public class RentalsController : Controller
    {
        public readonly RealEstateContext Context = new RealEstateContext();
        public async Task<ActionResult> Index(RentalsFilter filters)
        {
            var rentals = await FilterRentals(filters)
              .Select(r => new RentalViewModel
            {
              Id = r.Id,
                  Address = r.Address,
                  Description = r.Description,
                  NumberOfRooms = r.NumberOfRooms,
                  Price = r.Price
            })
            .OrderBy(r => r.Price)
              .ThenByDescending(r => r.NumberOfRooms)
              .ToListAsync();
            var model = new RentalsList
          {
            Rentals = rentals,
                Filters = filters
          };
          return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }

        private IMongoQueryable<Rental> FilterRentals(RentalsFilter filters)
        {
            var rentals = Context.Rentals.AsQueryable();

            if (filters.MinimumRooms.HasValue)
          {
            rentals = rentals
                  .Where(r => r.NumberOfRooms >= filters.MinimumRooms);
          }

          if (filters.PriceLimit.HasValue)
          {
            rentals = rentals
                  .Where(r => r.Price <= filters.PriceLimit);
          }
          return rentals;
        }
    }
}
