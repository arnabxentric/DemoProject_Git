using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
 

namespace XenERP.Controllers
{
    public class MenuController : Controller
    {
        InventoryEntities db = new InventoryEntities();
        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateMenu()
        {
            return View();
        }




        [HttpGet]
        public JsonResult ShowAllMenujson(string userid)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);

            int AssignId = Convert.ToInt32(Session["AssignId"]);

            int userids = Convert.ToInt32(userid);
           List<menuuser> user = new List<menuuser>();



           var acess = db.MenuaccessUsers.Where(d => d.CompanyId == companyid && d.AssignedUserId == userids);
            foreach(var menu in acess)
            {
                var usermenu = new menuuser();

                usermenu.Name = menu.Name;
                user.Add(usermenu);
            }
            return Json(user, JsonRequestBehavior.AllowGet);
        }



          [HttpGet]
        public ActionResult ShowAllMenu()
        {

            int userid = Convert.ToInt32(Session["userid"]);


            int companyid = Convert.ToInt32(Session["companyid"]);



           // ViewBag.company = db.Companies.Where(d => d.Userid == userid).ToList();
            ViewBag.Role = db.Roles.Where(d => d.CompanyId==companyid && d.UserId == userid).ToList();



            ViewBag.list = db.MenuMasters.OrderBy(d => d.AccessMenuHeaderId).ToList();
            
           
            return View();
        }

        [HttpPost]
        public ActionResult ShowAllMenu(string company,string role,string user,string menu)
        {

            try
            {

                int companyid = Convert.ToInt32(Session["companyid"]);
                int userid = Convert.ToInt32(Session["userid"]);
                int usermenuid = Convert.ToInt32(user);
                int roleid = Convert.ToInt32(role);
                var menuaccess = db.MenuaccessUsers.Where(d => d.CompanyId == companyid && d.AssignedUserId == usermenuid).ToList();
                foreach(var menulist in menuaccess)
                {
                    db.MenuaccessUsers.Remove(menulist);
                    db.SaveChanges();

                }

                int Createdby = Convert.ToInt32(Session["Createdid"]);

                
                MenuaccessUser access = new MenuaccessUser();


                string[] value = menu.Split(',');
                foreach (var key in value)
                {
                    access.CompanyId = Convert.ToInt32(companyid);
                    access.RoleId = Convert.ToInt32(role);
                    access.AssignedUserId = Convert.ToInt64(user);
                    access.CreatedBy = Createdby;
                    access.Userid = userid;
                        access.CreatedOn = DateTime.Now;
                    access.Name = key;

                    db.MenuaccessUsers.Add(access);
                   db.SaveChanges();
                }


                return RedirectToAction("ShowAllMenu", new { Msg = "Menu has been assign successfuly" });
            }
            catch {

                return RedirectToAction("ShowAllMenu", new { Err = "Menu cannot be assigned" });
            
            }
        }




        public JsonResult GetUserName(int type)
        {

            long companyid = Convert.ToInt32(Session["companyid"]);

            int userid = Convert.ToInt32(Session["userid"]);


            var group = rep.GetUsersName(type, companyid, userid);
            return Json(group, JsonRequestBehavior.AllowGet);
        }

    }
}
