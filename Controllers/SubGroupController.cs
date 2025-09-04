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
    public class SubGroupController : Controller
    {
        //
        // GET: /SubGroup/
        XenERP.Models.Repository.TaxRepository rep = new Models.Repository.TaxRepository();

        InventoryEntities db = new InventoryEntities();
        public ActionResult Index()
        {
            return View();
        }



        public JsonResult GetGroupName(int type)
        {
            
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

            var group = rep.GetGroupName(type, companyid, Branchid);
            return Json(group, JsonRequestBehavior.AllowGet);
        }






        public ActionResult ShowAllSubgroup(string Msg, string Err)
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


            var subgroup = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.BranchId == Branchid && d.ParentGroupId != 0 && d.ParentGroupId != 1 && d.ParentGroupId != 2 && d.ParentGroupId != 3 && d.ParentGroupId != 4).ToList();
         return View(subgroup);

        }



        public JsonResult CheckgroupId(string id)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.gID == id && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Checkgroupname(string name, int parentid)
        {

            int companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);



            bool type = db.GroupMasters.Any(g => g.groupName == name && g.ParentGroupId == parentid && g.CompanyId == companyid && g.BranchId == Branchid && g.UserId == userid);
            return Json(type, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateSubGroup()
        {


            int userid = Convert.ToInt32(Session["userid"]);
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);

          //  ViewBag.Grouptype = db.GroupMasters.Where(d => d.CompanyId == companyid && d.BranchId == Branchid);
            ViewBag.Group = db.GroupMasters.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid && (d.ParentGroupId == 1 || d.ParentGroupId == 2 || d.ParentGroupId == 3 || d.ParentGroupId == 4)).ToList();
           // var subgroup = db.GroupMasters.Where(d => d.CompanyId == companyid && d.UserId == userid && d.BranchId == Branchid && d.ParentGroupId != 0 && d.ParentGroupId != 1 && d.ParentGroupId != 2 && d.ParentGroupId != 3 && d.ParentGroupId != 4).ToList();
            return View();
        }



        [HttpPost]
        public ActionResult CreateSubGroup(GroupMaster group, FormCollection collection)
        {


            try
            {

                GroupMaster gp = new GroupMaster();


                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);


                ViewBag.Group = db.GroupMasters.Where(d => d.UserId == userid && d.CompanyId == companyid && d.BranchId == Branchid && (d.ParentGroupId == 1 || d.ParentGroupId == 2 || d.ParentGroupId == 3 || d.ParentGroupId == 4)).ToList();

                string gid = Convert.ToString(collection["gID"]);
                int parentid = Convert.ToInt32(collection["ParentGroupId"]);
                string groupname = collection["groupName"];

                var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && d.CompanyId == companyid && d.BranchId == Branchid).Count();
                // var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroup > 0)
                {
                    ViewBag.Error = "Sub Group Code  Already Exists...";
                    return View(group);
                }
                var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && d.CompanyId == companyid && d.BranchId == Branchid).Count();
                //var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroupName > 0)
                {
                    ViewBag.Error = "Sub Group Name  Already Exists...";
                    return View(group);
                }
                var getGrouptype = db.GroupMasters.FirstOrDefault(d => d.groupID == group.ParentGroupId && d.CompanyId == companyid && d.BranchId == Branchid);
                string grouptypename = getGrouptype.GroupType;
                int grouptypeid =(int) getGrouptype.GrouptypeId;

                var countparentid = db.GroupMasters.Where(d => d.ParentGroupId == grouptypeid).Count() + 1;
                var parentsortorder = db.GroupMasters.FirstOrDefault(d => d.ParentGroupId == grouptypeid).SortOrder;
                var countpid = 10000 + countparentid;
                var splitl3 = Convert.ToString(countpid).Substring(2);
                string sortorder = "00" + grouptypeid + splitl3;

                db.Insertgroupmaster(gid, groupname, grouptypename, grouptypeid, parentid, sortorder, fyid, companyid, userid, Branchid);           
                return RedirectToAction("ShowAllSubgroup", new { Msg = "Data Updated Successfully...." });
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
                return RedirectToAction("ShowAllSubgroup", new { Err = "Subgroup details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllSubgroup", new { Err = "Subgroup details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllSubgroup", new { Err = "Subgroup details  not  saved successfully.." });

            }

        }


        [HttpGet]
        public ActionResult EditSubGroup(int id)
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);


            var subgroup = db.GroupMasters.Where(d => d.groupID==id).FirstOrDefault();

           // ViewBag.Grouptype = db.GroupTypes.ToList();
           // ViewBag.Grouptype = new SelectList(db.GroupMasters.Where(d=>d.ParentGroupId==0 && d.CompanyId==0 && d.BranchId==0 && d.UserId==0), "groupID", "groupName", subgroup.GrouptypeId);
           // ViewBag.Group = new SelectList(db.GroupMasters.Where(d => d.groupID==subgroup.ParentGroupId),"groupID","groupName",subgroup.groupID);
            ViewBag.Grouptype = db.GroupMasters.FirstOrDefault(d => d.groupID == subgroup.ParentGroupId).groupName;
            return View(subgroup);
        }






        [HttpPost]
        public ActionResult EditSubGroup(GroupMaster group, FormCollection collection)
        {
            try
            {
               
                int groupID = Convert.ToInt32(collection["groupID"]);

                var subgroup = db.GroupMasters.Where(d => d.groupID == groupID).FirstOrDefault();
                ViewBag.Grouptype = db.GroupMasters.FirstOrDefault(d => d.groupID == subgroup.ParentGroupId).groupName;
                int companyid = Convert.ToInt32(Session["companyid"]);
                long userid = Convert.ToInt64(Session["userid"]);
                int fyid = Convert.ToInt32(Session["fid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);

                //string grouptypename = collection["Grouptypename"];
                //int grouptypeid = Convert.ToInt32(collection["GrouptypeId"]);
                //string gID = Convert.ToString(collection["gID"]);
                //int parentid = Convert.ToInt32(collection["ParentGroupId"]);
                //string groupname = collection["groupName"];
                //var countparentid = db.GroupMasters.Where(d => d.ParentGroupId == groupID).Count() + 1;
                //string sortorder = "00" + groupID + "00" + countparentid;
                var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == group.gID && d.CompanyId == companyid && d.BranchId == Branchid && d.groupID != group.groupID).Count();
                // var checkduplicateGroup = db.GroupMasters.Where(d => d.gID == gid && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroup > 0)
                {
                    ViewBag.Error = "Sub Group Code  Already Exists...";
                    return View(group);
                }
                var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == group.groupName && d.CompanyId == companyid && d.BranchId == Branchid && d.groupID != group.groupID).Count();
                //var checkduplicateGroupName = db.GroupMasters.Where(d => d.groupName == groupname && ((d.CompanyId == companyid && d.BranchId == Branchid) || (d.CompanyId == 0 && d.BranchId == 0))).Count();
                if (checkduplicateGroupName > 0)
                {
                    ViewBag.Error = "Sub Group Name  Already Exists...";
                    return View(group);
                }
                int Createdby = Convert.ToInt32(Session["Createdid"]);

                subgroup.ModifiedOn = DateTime.Now;
                subgroup.ModifiedBy = Createdby;
                subgroup.gID = group.gID;
                subgroup.groupName = group.groupName;
              
                db.SaveChanges();

                return RedirectToAction("ShowAllSubgroup", new { Msg = "Data Updated Successfully...." });
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
                return RedirectToAction("ShowAllSubgroup", new { Err = "Company details  not  saved successfully.." });

            }
            catch (DataException)
            {
                //Log the error (add a variable name after DataException)
                ViewBag.Error = "Error:Data  not Saved Successfully.......";
                return RedirectToAction("ShowAllSubgroup", new { Err = "Company details  not  saved successfully.." });

            }
            catch (Exception exp)
            {
                return RedirectToAction("ShowAllSubgroup", new { Err = "Company details  not  saved successfully.." });

            }



        }


    }
}
