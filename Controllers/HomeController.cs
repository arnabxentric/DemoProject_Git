using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Net.Mail;
using XenERP.Models.Repository;
using System.Web.Security;
using System.Globalization;
using System.Data;
using System.Data.Entity.Validation;
namespace XenERP.Controllers
{
    
    public class HomeController : Controller
    {
        
        InventoryEntities db = new InventoryEntities();
        TaxRepository tp = new TaxRepository();

        public ActionResult Index()
        {
         

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }
         [OutputCache(Duration = 60)]
          [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = DateTime.Now.ToString("T"); 

            return View();
        }



         #region Accounts-------------
         [HttpGet]
         public ActionResult Login(string Msg)
         {
            
             if (Msg != null)
             {
                 ViewBag.Meassge = Msg;

             }

             return View();

         }



         [HttpPost]
         public ActionResult Login(User user)
         {
             var userdet = db.Users.Where(d => d.UserName == user.UserName).FirstOrDefault();
             var fYear = db.FinancialYearMasters.Where(u => u.CompanyId == userdet.CompanyId).OrderByDescending(u => u.fYearID).FirstOrDefault();


             try
             {

                 Session["AssignId"] = userdet.Id;
                 Session["CreatedDate"] = userdet.CreatedDate;

                 Session["Userimage"] = userdet.UserImage;


                 if (userdet.UserId == null)
                 {
                     Session["userid"] = userdet.Id; // ---SuperAdmin Id--//
                 }
                 else
                 {
                     Session["userid"] = userdet.UserId; // ---SuperAdmin Id--//
                 }
                 Session["Createdid"] = userdet.Id;//---Created By Id--//

                 int companyidd;

                 if (userdet.CompanyId == null)
                 {
                     //Session["companyid"] = 0;
                     //companyidd = 0;

                 }
                 else
                 {
                     Session["companyid"] = userdet.CompanyId;
                     companyidd = Convert.ToInt32(userdet.CompanyId);


                     var Dtformat = db.Companies.Where(d => d.Id == companyidd).FirstOrDefault();
                     var dateformat = db.DateCultureFormats.Where(d => d.Id == Dtformat.DateCultureFormatId).FirstOrDefault();
                     Session["companylogo"] = Dtformat.CompanyLogo;
                     Session["companyname"] = Dtformat.Name;
                     Session["DateFormatLower"] = dateformat.ShortDatePatternLower;
                     Session["DateFormatUpper"] = "{0:" + dateformat.ShortDatePattern + "}";
                     Session["DateFormat"] = dateformat.ShortDatePattern;
                     Session["DateCulture"] = dateformat.Name;


                 }



                 if (userdet.BranchId == null)
                 {
                     Session["BranchId"] = 0;
                 }
                 else
                 {
                     Session["BranchId"] = userdet.BranchId;

                 }


                 if (fYear == null)
                 {
                     Session["fid"] = 0;
                 }
                 else
                 {
                     Session["fid"] = fYear.fYearID;
                     Session["FinYear"] = fYear.Year;

                }



                 Session["name"] = userdet.FirstName + " " + userdet.LastName;



             }

             catch
             {

             }

             int role = Convert.ToInt32(userdet.RoleId);

             if (role == 1)
             {

                 if (userdet.CompanyId == null)
                 {
                     Session["company"] = 0;
                 }

                 Session["Role"] = "SuperAdmin";

              //   return RedirectToAction("DashBoard", "DashBoard");

                 //int companyid = Convert.ToInt32(Session["companyid"]);


                 //long Branchid = Convert.ToInt64(Session["BranchId"]);

                 //int userid = Convert.ToInt32(Session["userid"]);

                 //var setup = db.CompanySetupGuides.Where(d => d.UserId == userid).FirstOrDefault();



                 //if (setup == null)
                 //{
                 //    return RedirectToAction("Next", "Start");
                 //}

                 //if (setup.Start == true && setup.Organization == false && setup.Financial == false && setup.Accounts == false && setup.Done == false)
                 //{
                 //    return RedirectToAction("CreateCompany", "Start", new { id = companyid });
                 //}

                 //if (setup.Organization == true && setup.Start == true && setup.Financial == false && setup.Accounts == false && setup.Done == false)
                 //{

                 //    return RedirectToAction("CreateFinancial", "Start");
                 //}

                 //if (setup.Organization == true && setup.Financial == true && setup.Start == true  && setup.Accounts == false && setup.Done == false)
                 //{
                 //    return RedirectToAction("ShowAllCurrencyrate", "Start");
                 //}

                 //if (setup.Organization == true && setup.Financial == true  && setup.Start == true &&  setup.Accounts == false && setup.Done == false)
                 //{
                 //    return RedirectToAction("TaxDetails", "Start");
                 //}


                 //if (setup.Organization == true && setup.Financial == true  && setup.Start == true && setup.Accounts == false && setup.Done == false)
                 //{
                 //    return RedirectToAction("ShowGroupMaster", "Start");
                 //}

                 //if (setup.Organization == true && setup.Financial == true && setup.Accounts == true && setup.Start == true && setup.Done == false)
                 //{
                 //    return RedirectToAction("Finish", "Start");
                 //}

                 //else
                 //{



               //  return RedirectToAction("CompanyList", "Company");
                return RedirectToAction("BranchList");

                //}


            }

             else
             {

                 var roles = db.Roles.Where(d => d.Id == role).FirstOrDefault();
                
                 int roleid = Convert.ToInt32(userdet.Id);
                Session["RoleId"]=roles.Id;
                 Session["Role"] = roles.RoleName;
                if (userdet.LID == 1)
                {
                    return RedirectToAction("BranchList");
                }
                else
                {
                    var menu = db.MenuaccessUsers.Where(d => d.AssignedUserId == roleid && d.CompanyId == userdet.CompanyId).ToList();
                    Session["company"] = 1;

                    Session["menu"] = menu;

                    return RedirectToAction("DashBoard", "DashBoard", new { id = userdet.BranchId });
                }

             }





             if (userdet.IsActive == true)
             {

                 FormsAuthenticationTicket tkt = null;
                 String CookieStr = null;
                 HttpCookie Ck = null;

                 tkt = new FormsAuthenticationTicket(1, userdet.UserName, DateTime.Now, DateTime.Now.AddMinutes(30), true, userdet.FirstName, FormsAuthentication.FormsCookiePath);

                 CookieStr = FormsAuthentication.Encrypt(tkt);
                 Ck = new HttpCookie(FormsAuthentication.FormsCookieName, CookieStr);


                 Ck.Expires = tkt.Expiration;

                 Ck.Path = FormsAuthentication.FormsCookiePath;

                 Response.Cookies.Add(Ck);
                 return RedirectToAction("Next", "Start");
             }
             else
             {

                 return RedirectToAction("Login", "Home", new { Msg = "You are not authorisez to Login.Please contact your Admin." });

             }
         }



         [HttpGet]
         public ActionResult Success(string Msg)
         {


             if (Msg != null)
             {
                 ViewBag.Meassge = Msg;

             }

             return View();

         }



         public ActionResult GetDateformatByCountry()
         {
             //var formats = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
             //.GroupBy(x => new RegionInfo(x.Name).DisplayName)
             // .ToDictionary(x => x.Key, x => x.SelectMany(y => y.DateTimeFormat.GetAllDateTimePatterns()).Distinct().ToArray());
             List<DateCulture> dclist = new List<Models.DateCulture>();
             var formats = CultureInfo.GetCultures(CultureTypes.SpecificCultures).GroupBy(x => new { new RegionInfo(x.Name).DisplayName, x.DateTimeFormat.ShortDatePattern, x.Name, x.DateTimeFormat.ShortTimePattern }).Select(x => new DateCulture { Name = x.Key.Name, ShortDatePattern = x.Key.ShortDatePattern, DisplayName = x.Key.DisplayName, ShortTimePattern = x.Key.ShortTimePattern });

             var filteredlist = formats.GroupBy(x => new { x.DisplayName, x.ShortDatePattern }).OrderBy(x => x.Key.DisplayName).Select(x => new DateCulture { DisplayName = x.Key.DisplayName, ShortDatePattern = x.Key.ShortDatePattern });

             // .ToDictionary(x => x.Name, x => x.DateTimeFormat.GetAllDateTimePatterns(),x);
             foreach (var filtered in filteredlist)
             {
                 DateCulture dc = new DateCulture();
                 var temp = formats.Where(d => d.DisplayName == filtered.DisplayName && d.ShortDatePattern == filtered.ShortDatePattern).Select(d => new { Name = d.Name, ShortTimePattern = d.ShortTimePattern }).FirstOrDefault(); ;
                 dc.Name = temp.Name;
                 dc.ShortTimePattern = temp.ShortTimePattern;
                 dc.DisplayName = filtered.DisplayName;
                 dc.ShortDatePattern = filtered.ShortDatePattern;
                 // if(temp.Name !=null && temp.ShortTimePattern)
                 dclist.Add(dc);
                 DateCultureFormat dcf = new DateCultureFormat();
                 dcf.Name = temp.Name;
                 dcf.ShortTimePattern = temp.ShortTimePattern;
                 dcf.DisplayName = filtered.DisplayName;
                 dcf.ShortDatePattern = filtered.ShortDatePattern;
                 db.DateCultureFormats.Add(dcf);
             }
             db.SaveChanges();
             return View(dclist);

         }



         [HttpGet]
         public ActionResult Register()
         {

             ViewBag.countrycode = db.CountryCodes.ToList().OrderBy(d=>d.Country);
            
             return View();
         }

         public JsonResult Checkusers(string UserName)
         {
            
                 bool ware = db.Users.Any(d => d.UserName == UserName);
                 return Json(!ware, JsonRequestBehavior.AllowGet);
            
         }



         public JsonResult GetCountryCode(int country)
         {

             long companyid = Convert.ToInt32(Session["companyid"]);

             int userid = Convert.ToInt32(Session["userid"]);


             var code = db.Countries.FirstOrDefault(d => d.CountryId == country).Code;
             return Json(code, JsonRequestBehavior.AllowGet);
         }


         [HttpPost]
         public ActionResult Register(User user, FormCollection collection)
         {
             try
             {


                 user.RoleId = 1;
                 user.PhoneNumber = collection["CountMobId"] + user.PhoneNumber;
                 user.CreatedDate = DateTime.Now;
                 user.BranchId = 0;
                 user.CompanyId = 0;
                 user.RoleId = 1;
                 user.CreatedDate = DateTime.Now;
              
                 db.Users.Add(user);
                 db.SaveChanges();
               
                 long id = tp.Getcountrycodeid(user);

                 int userid = Convert.ToInt32(id);
                 user.UserId = userid;
                 db.SaveChanges();


                 System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                    //  mail.To.Add("naresh.mahto3@gmail.com");
                    mail.To.Add(user.UserEmailAddress);
                    mail.From = new MailAddress("cabbooking7@gmail.com", "support", System.Text.Encoding.UTF8);
                    mail.Subject = "Activate your Account";
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;


                    string body = "<h3>Activate your Xen Inventory </h3><br/>";
                    body = body + "Dear &nbsp;"   + user.FirstName + ",<br/><br/>";
                    body = body + "A warm welcome to Xpert Inventory!:<br/><br/>";
                    body = body + "It is an auto-generated mail to confirm you that your account has been created successfully. You are just one step away from unlocking your account of the ERP system.:<br/><br/>";
                    body = body + "Please click the following link to activate your account.<br/><br/>";
               

                    //body = body + "http://localhost:2540/Home/ActivateAccount?userid=" + id + "<br/><br/><br/>";
                    body = body + " http://senbrothers.xentricserver.com/Home/ActivateAccount?userid=" + id + "<br/><br/><br/>";
                    body = body + "In case the link is not opening by clicking, please copy and paste the same in your browser and hit enter.Thanks for using Xen Inventory.<br/><br/>";

                    body = body + "With regards,<br/><br/>";
                    body = body + "Xen Inventory Team<br/><br/>";
                    body = body + "* This is an auto-generated mail. Replying to this will be sent to an unmonitored account.<br/><br/>";
                    body = body + "* For any help please contact us at support@xentric.com.<br/><br/>";


                    mail.Body = body;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = true;
                    SmtpClient client = new SmtpClient();
                    client.Credentials = new System.Net.NetworkCredential("cabbooking7@gmail.com", "03041983");
                    client.Port = 587;

                    client.Host = "smtp.gmail.com";
                    client.EnableSsl = true;
                    try
                    {
                        client.Send(mail);
                    }
                    catch (Exception ex)
                    {

                    }

                    return RedirectToAction("Success");
                }
             catch (DbEntityValidationException e)
             {
                 foreach (var eve in e.EntityValidationErrors)
                 {
                     Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                     eve.Entry.Entity.GetType().Name, eve.Entry.State);
                     foreach (var ve in eve.ValidationErrors)
                     {
                         Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                     }
                 }
                 throw;
                 ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                 return RedirectToAction("Register", new { Err = "Error! Please Try Again.." });

             }
             catch (DataException)
             {
                 //Log the error (add a variable name after DataException)
                 ViewBag.Error = "Error! Please Try Again..";
                 return RedirectToAction("Register", new { Err = "Error! Please Try Again.." });

             }
                // return View();
            
         }


        [HttpGet]
         public ActionResult Logout()
         {   //Disable back button In all browsers.
             Response.Cache.SetCacheability(HttpCacheability.NoCache);
             Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
             Response.Cache.SetNoStore();
             FormsAuthentication.SignOut();
             Session.Abandon();
             Session.Clear();
             return RedirectToAction("Login");
         }




         public JsonResult CheckUser(string UserID, string Id)
         {
           
             if (Id == "")
             {
                 bool user = db.Users.Any(d => d.UserName == UserID);
                 return Json(!user, JsonRequestBehavior.AllowGet);
             }
             else
             {
                 int id = 0;
                 bool ware = db.Users.Where(d => d.Id != id).Any(d => d.UserName == UserID);
                 return Json(ware, JsonRequestBehavior.AllowGet);

             
             }
         }


         public JsonResult CheckUserLogin(string UserName, string Id)
         {

             bool user = db.Users.Any(d => d.UserName == UserName && d.IsActive==true);
                 
             return Json(user, JsonRequestBehavior.AllowGet);
            

           
         }


         public JsonResult CheckPassword(string Password, string UserName)
         {
             bool password = db.Users.Any(d => d.Password == Password && d.UserName == UserName);

             return Json(password, JsonRequestBehavior.AllowGet);



         }



         public ActionResult ActivateAccount(string userid)
         {
             int id = Convert.ToInt32(userid);

             var user = db.Users.SingleOrDefault(d => d.Id == id);
             user.IsActive = true;
             db.SaveChanges();

             return RedirectToAction("Login");
         }


         public JsonResult Showcountrycode(int id)
         {
             XenERP.Models.Genral.Rates rate = new XenERP.Models.Genral.Rates();

             var country = db.CountryCodes.SingleOrDefault(d => d.Id == id);
             rate.countrycode = country.Code;
             return Json(rate, JsonRequestBehavior.AllowGet);
         }

    #endregion          -------------End Accounts------------


         #region //--------------Units--------------//
         [HttpGet]
        public ActionResult ShowUnits(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }



            int companyid = Convert.ToInt32(Session["companyid"]);


            long Branchid = Convert.ToInt64(Session["BranchId"]);

            int userid = Convert.ToInt32(Session["userid"]);



            var units = db.UOMs.Where(d => d.CompanyId == companyid || d.UserId == userid || d.CompanyId ==0 || d.UserId ==0);

            return View(units);
        }


        [HttpGet]
        public ActionResult CreateUnit(string Msg, string Err,string id)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            UOM unit = new UOM();
            if (id != null)
            {
                int rateid = Convert.ToInt32(id);

                var result = db.UOMs.SingleOrDefault(d => d.Id == rateid);

                unit.Id = rateid;
                unit.Code = result.Code;
                unit.Description = result.Description;
            }
            else
            {
                unit.Id = 0;
                unit.Code = null;
                unit.Description = null;
            }
            

       int companyid = Convert.ToInt32(Session["companyid"]);

       int userid = Convert.ToInt32(Session["userid"]);
       ViewBag.units = db.UOMs.Where(d => d.CompanyId == companyid || d.UserId == userid || d.CompanyId == 0 || d.UserId == 0);

            return View(unit);
        }


        [HttpPost]
        public ActionResult CreateUnit(UOM unit)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);

            
            if (unit.Id == 0)
            {

                try
                {
                    int userid = Convert.ToInt32(Session["userid"]);
                    ViewBag.units = db.UOMs.Where(d => d.UserId == userid).ToList();
                    unit.UserId = userid;
                   
                    unit.CompanyId = companyid;
                    db.UOMs.Add(unit);
                    db.SaveChanges();

                    return RedirectToAction("CreateUnit", new { Msg = "Unit created Successfully...." });
                }
                catch
                {


                    return RedirectToAction("CreateUnit", new { Err = "Unit cannot created Successfully...." });
                }
            }
            else
            {

                try
                {

                  
                    var result = db.UOMs.Find(unit.Id);

                    result.Code= unit.Code;
                    result.Description = unit.Description;
                 
                    db.SaveChanges();

                    return RedirectToAction("CreateUnit", new { Msg = "Unit data updated Successfully...." });
                }
                catch
                {


                    return RedirectToAction("CreateUnit", new { Err = "Unit data cannot be updated" });
                }
            
            }
        }



        [HttpGet]

        public ActionResult Unitcancel()
        {

            return RedirectToAction("CreateUnit");
        
        }

     

        public JsonResult CheckUnit(string Code,string Id)
        {
              int compid=  Convert.ToInt32(Session["companyid"]);

              string uppercode = Code.ToUpper();

              string trimcode = uppercode.Trim();

            int userid = Convert.ToInt32(Session["userid"]);
            int id=0;
            if (Id == "0")
            {
                bool isunit = db.UOMs.Any(d => d.Code == trimcode || d.UserId == userid && d.UserId == 0);
                return Json(!isunit, JsonRequestBehavior.AllowGet);
            }
            else
            {
                id = Convert.ToInt32(Id);
                bool isunit = db.UOMs.Where(d => d.Id != id).Any(d => d.Code == trimcode || d.UserId == userid && d.UserId == 0);
                return Json(!isunit, JsonRequestBehavior.AllowGet);

            }




        }

        #endregion //------------End Units--------------------//


        #region Branch List

        public ActionResult BranchList()
        {

            var culture = "es-AR";
            string dateFormat = "dd/MM/yyyy";
            var today = DateTime.Today;
            Session["todayDate"] = today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var branchList = db.BranchMasters.Where(r => r.CompanyId == 1 && r.IsDeleted == false).ToList();
            LoginModelView model = new LoginModelView();
            List<BranchList> brncLst = new List<BranchList>();

            foreach (var item in branchList)
            {
                BranchList lst = new BranchList();
                lst.Name = item.Name;
                lst.Id = item.Id;
                brncLst.Add(lst);
            }
            model.BranchListShow = brncLst;

            return View(model);
        }

        #endregion

    }
}
