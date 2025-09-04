using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;
using System.IO;
using System.Globalization;

namespace XenERP.Controllers
{
    public class EmployeeController : Controller
    {
        //
        // GET: /Employee/

        [HttpGet]
        public ActionResult CreateEmployee()
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var type = db.EmployeeTypes.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.Id, s.TypeName }).ToList();
                ViewBag.EmployeeType = type;
                ViewBag.Designation = db.Designations.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.Id, s.Name }).ToList();
                ViewBag.Grade = db.Grades.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.GradeId, s.GradeName }).ToList();
                ViewBag.Document = db.Documents.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).ToList();

                List<SelectListItem> li = new List<SelectListItem>();
                li.Add(new SelectListItem { Text = "Single", Value = "Single" });
                li.Add(new SelectListItem { Text = "Married", Value = "married" });
                li.Add(new SelectListItem { Text = "Separated", Value = "separated" });
                li.Add(new SelectListItem { Text = "Divorced", Value = "divorced" });
                li.Add(new SelectListItem { Text = "Widowed", Value = "widowed" });

                ViewBag.maritial = li;

                List<SelectListItem> li1 = new List<SelectListItem>();
                li1.Add(new SelectListItem { Text = "Male", Value = "male" });
                li1.Add(new SelectListItem { Text = "Female", Value = "female" });
                ViewBag.sex = li1;
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateEmployee(FormCollection collection)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    Employee employee = new XenERP.Models.Employee();

                    try
                    {
                        var culture = Session["DateCulture"].ToString();
                        var dateFormat = Session["DateFormat"].ToString();
                        int Branchid = Convert.ToInt32(Session["BranchId"]);
                        int userid = Convert.ToInt32(Session["userid"]);
                        int companyid = Convert.ToInt32(Session["companyid"]);
                        int fyid = Convert.ToInt32(Session["fid"]);
                        //var filename = string.Empty;
                        string fn = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
        

                        //string date = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        //var sdate = DateTime.ParseExact(model.Date, dateFormat, CultureInfo.CreateSpecificCulture(culture));

                        string filename = Path.GetFileName(Request.Files["productimg"].FileName.ToString());

                        if (filename != "")
                        {
                            string extension = Path.GetExtension(filename);
                            string[] img = { ".jpeg", ".png", ".gif", ".bmp", ".jpg" };

                            if (img.Contains(extension))
                            {
                                string path = Server.MapPath("~/EmployeeImages/");
                                Request.Files["productimg"].SaveAs(Path.Combine(path, filename));
                            }

                            else
                            {
                                return RedirectToAction("CreateCompany", new { Err = "Please select  .jpeg, .png, .gif, .bmp, .jpg Images" });
                            }

                        }

                        //foreach (string upload in Request.Files)
                        //{
                        //    if (Request.Files[upload].ContentLength == 0) continue;
                        //    string pathToSave = Server.MapPath("~/EmployeeImages/");
                        //    filename = fn + Path.GetFileName(Request.Files[upload].FileName);
                        //    string uploadpath = Path.Combine(pathToSave, filename);
                        //    FileInfo fi1 = new FileInfo(uploadpath);
                        //    if (fi1.Exists)
                        //    {
                        //        System.IO.File.Delete(uploadpath);
                        //    }
                        //    Request.Files[upload].SaveAs(Path.Combine(pathToSave, filename));
                        //}


                      
                        employee.Code = collection["Code"];
                        employee.FirstName = collection["FirstName"];
                        employee.LastName = collection["LastName"];
                        employee.Mobile = collection["Mobile"];
                        employee.EmployeeTypeID = Convert.ToInt32(collection["EmployeeTypeID"]);
                        employee.DesignationID = Convert.ToInt32(collection["DesignationID"]);
                        employee.GradeID = Convert.ToInt32(collection["GradeID"]);
                        var status =collection["Status"];
                        if (status == "false")
                        {
                            employee.Status = false;
                        }
                        else {
                            employee.Status = true;
                        }
                        employee.PresentAddress = collection["PresentAddress"];
                        employee.PresentState = collection["PresentState"];
                        employee.PresentCity = collection["PresentCity"];
                        employee.PresentPIN = collection["PresentPIN"];
                        employee.PermanentAddress = collection["PermanentAddress"];
                        employee.PermanentState = collection["PermanentState"];
                        employee.PermanentCity = collection["PermanentCity"];
                        employee.PermanentPIN = collection["PermanentPIN"];
                        employee.AltPhone = collection["AltPhone"];
                        employee.Email = collection["Email"];
                        employee.MaritialStatus = collection["MaritialStatus"];
                        employee.Sex = collection["Sex"];
                        var id = collection["EmpIDProf"];
                        var dob = collection["DateOfBirth"];
                        if (dob != "")
                        {
                            employee.BirthDate = DateTime.ParseExact(collection["DateOfBirth"], dateFormat, CultureInfo.CreateSpecificCulture(culture));
                        }
                        //employee.BirthDate = Convert.ToDateTime(collection["DateOfBirth"]);
                        //employee.DateOfBirth = collection["DateOfBirth"];
                        employee.Qualification = collection["Fax"];
                        employee.Specialization = collection["Specialization"];
                        employee.Experience = collection["Experience"];
                        employee.PreCompany = collection["PreCompany"];
                        if (id != "")
                        {
                            employee.EmpIDProf = Convert.ToInt32(collection["EmpIDProf"]);
                        }
                        employee.PFNo = collection["PFNo"];
                        employee.BankAccntNo = collection["BankAccntNo"];
                        employee.ESICNo = collection["ESICNo"];

                        employee.empPhoto = filename;

                        if (collection["chkpostal"] == "on")
                        {

                            employee.IsSamePresentAddress = true;
                        }
                        else
                        {
                            employee.IsSamePresentAddress = false;
                        }
                        employee.companyid = companyid;
                        employee.userId = userid;
                        employee.BranchId = Branchid;


                        db.Employees.Add(employee);
                        db.SaveChanges();

                        scope.Complete();

                        return RedirectToAction("ShowAllEmployee", new { Msg = "Employee Created Successfully....." });
                    }


                    catch (Exception exp)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllEmployee", new { Err = "Please Try Again......." });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ShowAllEmployee(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var empployee = db.Employees.Include("EmployeeType").Include("Grade").Include("Designation").Where(d =>d.companyid == companyid && d.BranchId==Branchid).ToList();
           
                return View(empployee);
            }
        }

        [HttpGet]
        public ActionResult EditEmployee(int id)
        { 
            using (InventoryEntities db =new InventoryEntities())
            {
                var culture = Session["DateCulture"].ToString();
                var dateFormat = Session["DateFormat"].ToString();
               
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var type = db.EmployeeTypes.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.Id, s.TypeName }).ToList();
                ViewBag.EmployeeType = type;
                ViewBag.Designation = db.Designations.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.Id, s.Name }).ToList();
                ViewBag.Grade = db.Grades.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).Select(s => new { s.GradeId, s.GradeName }).ToList();
                ViewBag.Document = db.Documents.Where(r => r.CompanyId == companyid && r.BranchId == Branchid).ToList();

                List<SelectListItem> li = new List<SelectListItem>();
                li.Add(new SelectListItem { Text = "Single", Value = "Single" });
                li.Add(new SelectListItem { Text = "Married", Value = "married" });
                li.Add(new SelectListItem { Text = "Separated", Value = "separated" });
                li.Add(new SelectListItem { Text = "Divorced", Value = "divorced" });
                li.Add(new SelectListItem { Text = "Widowed", Value = "widowed" });

                ViewBag.maritial = li;

                List<SelectListItem> li1 = new List<SelectListItem>();
                li1.Add(new SelectListItem { Text = "Male", Value = "male" });
                li1.Add(new SelectListItem { Text = "Female", Value = "female" });
                ViewBag.sex = li1;


                var employee = db.Employees.Find(id);
                if (employee.BirthDate != null)
                {
                    employee.DateOfBirth = employee.BirthDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                }
                //employee.BirthDate = DateTime.ParseExact(date, dateFormat, CultureInfo.CreateSpecificCulture(culture));
               
                return View(employee);
        
            }
        }


        [HttpPost]
        public ActionResult EditEmployee(Employee model)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var culture = Session["DateCulture"].ToString();
                var dateFormat = Session["DateFormat"].ToString();
                try
                {
                    var result = db.Employees.Find(model.Id);
                    model.ModifiedOn = DateTime.Now;
                    model.BirthDate = DateTime.ParseExact(model.DateOfBirth, dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    model.DateOfBirth = null;
                    db.Entry(result).CurrentValues.SetValues(model);

                    db.SaveChanges();
                    return RedirectToAction("ShowAllEmployee", new { Msg = "Data Updated Successfully...." });
                }
                catch {
                    return RedirectToAction("ShowAllEmployee", new { Msg = "Plese Try Again...." });
                }

            }
        }
    }
}
