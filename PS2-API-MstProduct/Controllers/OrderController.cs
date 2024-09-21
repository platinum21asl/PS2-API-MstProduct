using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PS2_BAL;
using PS2_DAL.Models;
using PS2_DAL.Models.Dto.Req;
using PS2_DAL.Models.ViewModel;
using PS2_DAL.Repositories.IRepository;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace PS2_API_MstProduct.Controllers
{
    [ApiController]
    [Route("rest/v1/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }

        [HttpGet]
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return Json(OrderVM);
        }

        [HttpPost("updateOrderDetails/{orderId}")]
        public IActionResult UpdateOrderDetails(int orderId, [FromBody] OrderUpdateDto orderDto)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (orderHeaderFromDb == null)
            {
                return NotFound(new { status = "404", message = "Order not found" });
            }

            // Update fields from the DTO
            orderHeaderFromDb.Name = orderDto.Name;
            orderHeaderFromDb.PhoneNumber = orderDto.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderDto.StreetAddress;
            orderHeaderFromDb.City = orderDto.City;
            orderHeaderFromDb.State = orderDto.State;
            orderHeaderFromDb.PostalCode = orderDto.PostalCode;

            if (!string.IsNullOrEmpty(orderDto.Carrier))
            {
                orderHeaderFromDb.Carrier = orderDto.Carrier;
            }
            if (!string.IsNullOrEmpty(orderDto.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderDto.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            return Json(new { status = "200", message = "Order updated successfully", data = new { orderId = orderHeaderFromDb.Id } });
        }


        [HttpPost]
        public IActionResult StartProcessing(int orderId)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (orderHeaderFromDb == null)
            {
                return Json(new { status = "404", message = "Order not found" });
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusInProcess);
            _unitOfWork.Save();

            return Json(new { status = "200", message = "Success", data = new { orderId = orderId } });
        }


        [HttpPost]
        public IActionResult ShipOrder(int orderId, string trackingNumber, string carrier)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);

            if (orderHeader == null)
            {
                return Json(new { status = "404", message = "Order not found" });
            }

            orderHeader.TrackingNumber = trackingNumber;
            orderHeader.Carrier = carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShoppingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            return Json(new { status = "200", message = "Success", data = new { orderId = orderHeader.Id } });
        }


        [HttpPost]
        public IActionResult CancelOrder([FromBody] int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);

            if (orderHeader == null)
            {
                return Json(new { status = "404", message = "Order not found" });
            }

            // If payment is already approved, initiate refund
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            return Json(new { status = "200", message = "Success", data = new { orderId = orderHeader.Id } });
        }

        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        [HttpPost("paymentConfirmation/{orderHeaderId}")]
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                // Ini adalah pesanan oleh perusahaan
                var service = new SessionService();
                var session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return Json(orderHeaderId);
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;

            }
            return Json(objOrderHeaders);
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            // delete the old image
            var oldImagePath =
                Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Delete(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
    }
}
