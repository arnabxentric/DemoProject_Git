using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data;

namespace XenERP.Controllers
{
    public class GroupMasterController : Controller
    {
        //
        // GET: /GroupMaster/
        InventoryEntities db = new InventoryEntities();
        public ActionResult Index()
        {
            return View();
        }



        #region ------------Group Master--------------


        public ActionResult ShowGroupMaster(string Msg, string Err)
        {

            int userid = Convert.ToInt32(Session["userid"]);

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

            var result = db.GroupMasters.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid && (d.ParentGroupId == 1 || d.ParentGroupId == 2 || d.ParentGroupId == 3 || d.ParentGroupId == 4));

            return View(result);
        }

        [HttpGet]
        public ActionResult CreateGroupMaster()
        {
            ViewBag.Grouptype = db.GroupMasters.Where(d => d.ParentGroupId == 0);
            return View();
        }

       
        [HttpPost]
        public ActionResult CreateGroupMaster(GroupMaster group, FormCollection collection)
        {
            try
            {

                ViewBag.Grouptype = db.GroupMasters.Where(d => d.ParentGroupId == 0);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);

              
                int Fyid = Convert.ToInt32(Session["fid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                string grouptypename = collection["Grouptypename"];
                int grouptypeid = Convert.ToInt32(collection["GroupType"]);

                string gid = Convert.ToString(collection["gID"]);

                string groupname = collection["groupName"];

                var countparentid = db.GroupMasters.Where(d => d.ParentGroupId == grouptypeid).Count() + 1;
                var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && d.CompanyId==companyid && d.BranchId==Branchid).Count();
               // var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroup > 0)
                {
                    ViewBag.Error = "Group Code  Already Exists...";
                    return View(group);
                }
                var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && d.CompanyId == companyid && d.BranchId == Branchid).Count();
                //var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroupName > 0)
                {
                    ViewBag.Error = "Group Name  Already Exists...";
                    return View(group);
                }
                var countpid = 10000 + countparentid;
                var splitl3 = Convert.ToString(countpid).Substring(2);
                string sorder = "00" + grouptypeid + splitl3;
                db.Insertgroupmaster(gid, groupname, grouptypename, grouptypeid, grouptypeid, sorder, Fyid, companyid, userid, Branchid);

                return RedirectToAction("ShowGroupMaster", new { Msg = "Data Saved Successfully...." });
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
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
        }


        public JsonResult GetGrouptype(int id,int parentid)
        {

            var type = db.GroupMasters.Where(global => global.GrouptypeId == id).FirstOrDefault();
            return Json(type.GroupType, JsonRequestBehavior.AllowGet);
        }




        public JsonResult CheckgroupId(string id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);            
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.gID == id && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Checkgroupname(string name,int parentid)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.groupName == name && g.ParentGroupId == parentid && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult EditGroupMaster(int id)
        {


            var result = db.GroupMasters.Where(d => d.groupID == id ).FirstOrDefault();
           // ViewBag.Grouptype = new SelectList(db.GroupMasters.Where(d => d.ParentGroupId == 0), "GroupID", "GroupName", result.GrouptypeId);
            ViewBag.Grouptype = db.GroupMasters.FirstOrDefault(d => d.groupID == result.ParentGroupId).groupName;
            return View(result);
        }



        [HttpPost]
        public ActionResult EditGroupMaster(GroupMaster group, FormCollection collection)
        {
            try
            {
                int companyid = Convert.ToInt32(Session["companyid"]);
                int userid = Convert.ToInt32(Session["userid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                ViewBag.Grouptype = db.GroupMasters.FirstOrDefault(d => d.groupID == group.ParentGroupId).groupName;

                var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == group.gID && d.CompanyId == companyid && d.BranchId == Branchid && d.groupID!=group.groupID).Count();
                // var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroup > 0)
                {
                    ViewBag.Error = "Group Code  Already Exists...";
                    return View(group);
                }
                var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == group.groupName && d.CompanyId == companyid && d.BranchId == Branchid && d.groupID != group.groupID).Count();
                //var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroupName > 0)
                {
                    ViewBag.Error = "Group Name  Already Exists...";
                    return View(group);
                }

                int Createdby = Convert.ToInt32(Session["Createdid"]);

                group.ParentGroupId = Convert.ToInt64(group.GrouptypeId);
                group.GroupType = collection["Grouptypename"];
                group.ModifiedBy = Createdby;
                group.ModifiedOn = DateTime.Now;
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("ShowGroupMaster", new { Msg = "Data Updated Successfully...." });
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
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  update successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  update successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowGroupMaster", new { Err = "Group details  not  saved successfully.." });

            }
        }

        [HttpPost]
        public JsonResult getGroupsByName(string query = "")
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var groups = db.GroupMasters.Where(p => p.groupName.Contains(query) && (p.CompanyId == companyid) && p.ParentGroupId != 0).Select(p => new { Name = p.groupName, Id = p.groupID }).ToList();
            return Json(groups, JsonRequestBehavior.AllowGet);


        }
        #endregion

    }
}
