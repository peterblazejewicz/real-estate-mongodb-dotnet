using Xunit;
using RealEstate.Mvc.Model;

namespace RealEstate.Tests
{
    public class RentalTests
    {
        [Fact]
        public void PriceRepresentedAsDoubleTest()
        {
            var rental = new Rental();
			      rental.Price = 1;
            Assert.IsType<decimal>(rental.Price);
        }
    }
}
