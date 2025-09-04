using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace XenERP.Controllers
{
    public class InvoicePaymentController : Controller
    {
        //
        // GET: /InvoicePayment/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CreatePayment(int id)
        {


            return View();

        }

    }
}
