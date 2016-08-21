using System.Collections.Generic;

namespace RealEstate.Mvc.Model
{
    public class RentalViewModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int NumberOfRooms { get; set; }
        public decimal Price { get; set; }
        public List<string> Address { get; set; }
    }
}
