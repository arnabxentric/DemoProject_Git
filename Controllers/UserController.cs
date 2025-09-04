using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models.Repository;
using XenERP.Models;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Net.Mail;

namespace XenERP.Controllers
{
    [SessionExpire]
    public class UserController : Controller
    {
        //
        // GET: /USer/


        InventoryEntities db = new InventoryEntities();
        TaxRepository tp = new TaxRepository();




        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public ActionResult ShowAllUser(string Msg, string Err)
        {





            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            int userid = Convert.ToInt32(Session["userid"]);

            int companyid = Convert.ToInt32(Session["companyid"]);

            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var user = db.Users.Where(d => d.UserId == userid && d.CompanyId == companyid);


            return View(user);
        }

        [HttpPost]
        public JsonResult Checkuserid(string UserName, string Id)
        {
            int id = 0;
            if (Id == "")
            {
                bool result = db.Users.Any(d => d.UserName == UserName);

                return Json(!result, JsonRequestBehavior.AllowGet);

            }
            else
            {
                id = Convert.ToInt32(Id);
                bool result = db.Users.Where(d => d.Id != id).Any(d => d.UserName == UserName);

                return Json(!result, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpGet]
        public ActionResult CreateUser()
        {

            int userid = Convert.ToInt32(Session["userid"]);

            int companyid = Convert.ToInt32(Session["companyid"]);
            //   var id = db.Users.Where(d => d.Id == userid).FirstOrDefault();

            //          ViewBag.company = db.Companies.Where(d => d.Userid==userid).ToList();
            //ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList().OrderBy(d => d.Name);
            List<BranchMaster> branchlist=new List<BranchMaster>();
            var hoBranch=new BranchMaster();
            hoBranch.Id = 0;
            hoBranch.Name = "H.O. Branch";
            branchlist.Add(hoBranch);
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
            foreach (var branch in branches)
            {
                branchlist.Add(branch);
            }

            ViewBag.role = db.Roles.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList().OrderBy(d => d.RoleName);

            ViewBag.branch = branchlist.Select(b => new {b.Id,b.Name });

            ViewBag.countrycode = db.CountryCodes.ToList().OrderBy(d => d.Country);

            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(User user)
        {
            try
            {
                int companyid = Convert.ToInt32(Session["companyid"]);


                int Branchid = Convert.ToInt32(Session["BranchId"]);

                int userid = Convert.ToInt32(Session["userid"]);

                int createdby = Convert.ToInt32(Session["Createdid"]);
                user.CreatedDate = DateTime.Now;
                user.UserId = userid;
                user.CompanyId = companyid;
                user.CreatedId = createdby;
                user.BranchId = user.BranchId;
                db.Users.Add(user);
                db.SaveChanges();


                var comp = db.Companies.Where(d => d.Id == companyid).FirstOrDefault();


                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.To.Add(user.UserEmailAddress);
                mail.From = new MailAddress("cabbooking7@gmail.com", "support", System.Text.Encoding.UTF8);
                mail.Subject = "Activate your Account";
                mail.SubjectEncoding = System.Text.Encoding.UTF8;


               
                string body ="Dear &nbsp;" + user.FirstName + ",<br/><br/>";
                body = body + "A warm welcome to " + comp.Name + " Inventory!:<br/><br/>";
                body = body + "It is an auto-generated mail to confirm you that your account has been created successfully.<br/><br/>";
                body = body + "User Name:"+user.UserName+"<br/><br/>";
                body = body + "Password:" + user.Password + "<br/><br/>";

                body = body + "With regards,<br/><br/>";
                body = body + "" + comp.Name + " Team<br/><br/>";
                body = body + "* This is an auto-generated mail. Replying to this will be sent to an unmonitored account.<br/><br/>";
                body = body + "* For any help please contact us at support@xpert.com.<br/><br/>";


                mail.Body = body;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential("cabbooking7@gmail.com", "/*Pa03041983");
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

                return RedirectToAction("ShowAllUser", new { Msg = "Data Saved Successfully...." });
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
                return RedirectToAction("ShowAllUser", new { Err = "User details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllUser", new { Err = "User details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllUser", new { Err = "User details  not  saved successfully.." });

            }










        }



        [HttpGet]
        public ActionResult EditUser(int id)
        {

            var user = db.Users.Where(d => d.Id == id).FirstOrDefault();

            try
            {

                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);

             
                List<BranchMaster> branchlist = new List<BranchMaster>();
                var hoBranch = new BranchMaster();
                hoBranch.Id = 0;
                hoBranch.Name = "H.O. Branch";
                branchlist.Add(hoBranch);
                var branches = db.BranchMasters.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList();
                foreach (var branch in branches)
                {
                    branchlist.Add(branch);
                }
                ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList().OrderBy(d => d.Name);
                ViewBag.role = db.Roles.Where(d => d.CompanyId == companyid && d.UserId == userid).ToList().OrderBy(d => d.RoleName);

                ViewBag.branch = branchlist.Select(b => new { b.Id, b.Name });
               

                return View(user);
            }
            catch
            {

                return View(user);
            }


        }

        [HttpPost]
        public ActionResult EditUser(User user)
        {
            try
            {
                var userss = db.Users.Where(d => d.Id == user.Id);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ShowAllUser", new { Msg = "Data Updated Successfully...." });
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
                return RedirectToAction("ShowAllUser", new { Err = "User details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllUser", new { Err = "Please Try Again..." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllUser", new { Err = "Please Try Again..." });

            }

        }




        #region User Profile



        [HttpGet]
        public ActionResult UserProfile(int id,string Msg, string Err)
        {

            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }

            var user = db.Users.Where(d => d.Id == id).FirstOrDefault();

            try
            {

                return View(user);
            }
            catch
            {

                return View(user);
            }


        }

        [HttpPost]
        public ActionResult UserProfile(User user,FormCollection collection)
        {
            try
            {

                string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

               

                if (filename != "")
                {
                    string extension = Path.GetExtension(filename);

                    string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                    if (img.Contains(extension))
                    {


                        string path = Server.MapPath("~/img/");

                        Request.Files["productimg"].SaveAs(Path.Combine(path, filename));



                    }

                    else
                    {
                        ViewBag.Error = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images";
                        return View();

                    }


                }
                if (filename == "" || filename == null)
                {
                    user.UserImage = user.UserImage;
                }
                else
                {
                    Session["Userimage"] = filename;
                    user.UserImage = filename;
                }
                
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("UserProfile", new { Msg = "Data Saved Successfully...." });
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
                return RedirectToAction("UserProfile", new { Err = "Please Try Again.."});

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("UserProfile", new { Err = "Please Try Again.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("UserProfile", new { Err = "Please Try Again.." });

            }

        }

        #endregion
    }
}
