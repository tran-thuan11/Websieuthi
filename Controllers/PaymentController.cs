using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models.Order;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        // Constructor chính, sử dụng Dependency Injection
        public PaymentController(IMomoService momoService, IVnPayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        // Action tạo URL thanh toán với MoMo
        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);

            // Kiểm tra URL trả về
            if (string.IsNullOrEmpty(response?.PayUrl))
            {
                return BadRequest("Không thể tạo URL thanh toán MoMo.");
            }

            return Redirect(response.PayUrl);
        }

        // Action tạo URL thanh toán với VNPay
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }


    }
}