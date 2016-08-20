using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using RealEstate.Mvc.Model;

namespace RealEstate.Mvc
{
    public class RealEstateContext
    {
        public RealEstateContext()
        {
          // @TODO read from configuration
          var connectionString = "mongodb://127.0.0.1:27017";
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase("realestate");
            ImagesBucket = new GridFSBucket(Database);
        }

        public IMongoDatabase Database;

        public GridFSBucket ImagesBucket { get; set; }

        public IMongoCollection<Rental> Rentals => Database.GetCollection<Rental>("rentals");
    }
}
