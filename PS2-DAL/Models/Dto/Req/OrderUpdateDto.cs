using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS2_DAL.Models.Dto.Req
{
    public class OrderUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string? StreetAddress { get; set; } 
        public string? City { get; set; } 
        public string? State { get; set; } 
        public string? PostalCode { get; set; } 
        public string? Carrier { get; set; } 
        public string? TrackingNumber { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime ShippingDate { get; set; }
        public DateTime PaymentDueDate { get; set; }
    }
}
