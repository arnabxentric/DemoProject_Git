using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XenERP.Models;


namespace XenERP
{
    public class AuthorizeUserAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        // Custom property
        public string menuitem { get; set; }


        InventoryEntities db = new InventoryEntities();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            string username=HttpContext.Current.User.Identity.Name.ToString();
            var role = db.Users.Where(d => d.UserName == username).FirstOrDefault();
            var accesslevel = db.MenuaccessUsers.Where(m => m.RoleId == role.RoleId && m.CompanyId==role.CompanyId).Select(m=>m.Name).ToList();
            if (accesslevel.Contains(this.menuitem))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new
                            {
                                controller = "Home",
                                action = "Login"
                            })
                        );
        }
    }
}