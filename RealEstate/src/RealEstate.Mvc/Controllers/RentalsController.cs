using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
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

        [HttpGet]
        public ActionResult Post()
        {
          return View();
        }

        [HttpPost]
        public async Task<ActionResult> Post(PostRental postRental)
        {
          Rental rental = new Rental(postRental);
          await Context.Rentals.InsertOneAsync(rental);
          return RedirectToAction("Index");
        }

        public ActionResult AdjustPrice(string id)
        {
          var rental = GetRental(id);
          return View(rental);
        }

        [HttpPost]
        public async Task<ActionResult> AdjustPrice(string id,
          AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            rental.AdjustPrice(adjustPrice);
            await Context.Rentals
              .ReplaceOneAsync(r => r.Id == id, rental);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string id)
        {
          await Context.Rentals.DeleteOneAsync(r => r.Id == id);
          return RedirectToAction("Index");
        }

        [HttpGet]
        public string PriceDistribution()
        {
          return new QueryPriceDistribution()
            .RunLinq(Context.Rentals)
            .ToJson();
        }

        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, ICollection<IFormFile> files)
        {
            var rental = GetRental(id);
            if (rental.HasImage())
            {
              await DeleteImageAsync(rental);
            }
            var file = files.FirstOrDefault<IFormFile>();
            await StoreImageAsync(file, id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetImage(string id)
        {
          try
          {
            var stream = Context.ImagesBucket
              .OpenDownloadStream(new ObjectId(id));
            var contentType = stream.FileInfo.Metadata["contentType"].AsString;
            return File(stream, contentType);
          }
          catch (GridFSFileNotFoundException)
          {
            return NotFound();
          }
        }

        public ActionResult JoinPreLookup()
        {
          var rentals = Context.Rentals.Find(new BsonDocument()).ToList();
          var rentalZips = rentals.Select(r => r.ZipCode).Distinct().ToArray();
          var zipsById = Context.Database
            .GetCollection<ZipCode>("zips")
            .Find(z => rentalZips.Contains(z.Id))
            .ToList()
            .ToDictionary(d => d.Id);
          var report = rentals
            .Select(r => new
            {
              Rental = r,
              ZipCode = r.ZipCode != null && zipsById.ContainsKey(r.ZipCode)
                ? zipsById[r.ZipCode] : null
            });

          return Content(report.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict}), "application/json");
        }

        public ActionResult JoinWithLookup()
        {
          var report = Context.Rentals
            .Aggregate()
            .Lookup<Rental, ZipCode, RentalWithZipCodes>(
              Context.Database.GetCollection<ZipCode>("zips"),
              r => r.ZipCode,
              z => z.Id,
              w => w.ZipCodes
            )
            .ToList();
          return Content(report.ToJson(new JsonWriterSettings {OutputMode = JsonOutputMode.Strict}), "application/json");
        }

        public IActionResult Error()
        {
            return View();
        }

        private async Task DeleteImageAsync(Rental rental)
        {
          await Context.ImagesBucket
            .DeleteAsync(new ObjectId(rental.ImageId));
			    await SetRentalImageIdAsync(rental.Id, null);
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

        private Rental GetRental(string id)
        {
          var rental = Context.Rentals
            .Find(r => r.Id == id)
            .FirstOrDefault();
          return rental;
        }

        private async Task StoreImageAsync(IFormFile file, string rentalId)
        {
          var options = new GridFSUploadOptions
          {
            Metadata = new BsonDocument("contentType", file.ContentType)
          };
          IGridFSBucket bucket = new GridFSBucket(Context.Database);
          var imageId = await Context.ImagesBucket
				    .UploadFromStreamAsync(file.FileName, file.OpenReadStream(), options);
          await SetRentalImageIdAsync(rentalId, imageId.ToString());
        }

        private async Task SetRentalImageIdAsync(string rentalId, string imageId)
        {
          var setRentalImageId = Builders<Rental>.Update
            .Set(r => r.ImageId, imageId);
			    await Context.Rentals
            .UpdateOneAsync(r => r.Id == rentalId, setRentalImageId);
        }
    }
}
