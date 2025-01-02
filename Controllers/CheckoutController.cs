using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Vnpay;
using System.Security.Claims;
using System.Text;

namespace Shopping_Tutorial.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly IVnPayService _vnPayService;
		private static readonly HttpClient client = new HttpClient();

		public CheckoutController(IEmailSender emailSender, DataContext context, IVnPayService vnPayService)
		{
			_dataContext = context;
			_emailSender = emailSender;
			_vnPayService = vnPayService;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel
				{
					OrderCode = ordercode,
					UserName = userEmail,
					Status = 1,
					CreatedDate = DateTime.Now
				};

				// Nhận shipping giá từ cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;
				//Nhận Coupon code từ cookie
				var coupon_code = Request.Cookies["CouponTitle"];

				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				orderItem.ShippingCost = shippingPrice;
				orderItem.CouponCode = coupon_code;

				_dataContext.Add(orderItem);
				await _dataContext.SaveChangesAsync();

				// Tạo order detail
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				StringBuilder emailMessage = new StringBuilder();
				emailMessage.AppendLine($"Cảm ơn {userEmail} đã đặt hàng tại siêu thị của chúng tôi");
				emailMessage.AppendLine($"Sau đây là thông tin đơn hàng bạn đã đặt:");
				foreach (var cart in cartItems)
				{
					var orderdetail = new OrderDetail
					{
						UserName = userEmail,
						OrderCode = ordercode,
						ProductId = cart.ProductId,
						Price = cart.Price,
						Quantity = cart.Quantity
					};

					// Cập nhật số lượng sản phẩm
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.Sold += cart.Quantity;
					_dataContext.Update(product);
					_dataContext.Add(orderdetail);
					await _dataContext.SaveChangesAsync();

					// Thêm thông tin sản phẩm vào email
					emailMessage.AppendLine($"----------------------------------------");
					emailMessage.AppendLine($"Sản phẩm: {product.Name},  ");
					emailMessage.AppendLine($"Giá: {cart.Price:C} VNĐ");
					emailMessage.AppendLine($"Số lượng: {cart.Quantity}");
					emailMessage.AppendLine($"Tổng tiền: {cart.Total}");


				}
				emailMessage.AppendLine($"Đơn hàng sẽ được giao đến bạn trong thời gian sớm nhất.");
				HttpContext.Session.Remove("Cart");

				// Gửi email với thông tin chi tiết sản phẩm
				var receiver = userEmail;
				var subject = "Đặt hàng thành công";
				var message = emailMessage.ToString();

				await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "Đơn hàng đã được tạo, vui lòng chờ duyệt đơn hàng nhé.";
				return RedirectToAction("History", "Account");
			}
		}

		[HttpGet]
		public IActionResult PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);
			return Json(response);
		}
	}
}
