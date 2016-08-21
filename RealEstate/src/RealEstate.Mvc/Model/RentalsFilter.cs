using MongoDB.Driver;

namespace RealEstate.Mvc.Model
{
    public class RentalsFilter
    {
        public decimal? PriceLimit { get; set; }
        public int? MinimumRooms { get; set; }

        public FilterDefinition<Rental> ToFilterDefinition()
        {
          // new BsonDocument()
          var filterDefinition = Builders<Rental>.Filter.Empty;
          if (PriceLimit.HasValue)
          {
            filterDefinition &=
              Builders<Rental>.Filter.Lte(r => r.Price, PriceLimit.Value);
          }
          if (MinimumRooms.HasValue)
          {
            filterDefinition &= Builders<Rental>.Filter
              .Where(r => r.NumberOfRooms >= MinimumRooms);
          }
          return filterDefinition;
        }
    }
}
