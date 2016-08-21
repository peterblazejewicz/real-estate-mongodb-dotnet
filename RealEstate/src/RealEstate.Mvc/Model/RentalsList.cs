using System.Collections.Generic;

namespace RealEstate.Mvc.Model
{
    public class RentalsList
    {
        public IEnumerable<RentalViewModel> Rentals { get; set; }
		    public RentalsFilter Filters { get; set; }
    }
}
