using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XenERP.Models;
namespace XenERP.Models
{
    public class MasterClasses
    {
        private InventoryEntities db = new InventoryEntities();
        public List<Category> getDdlCategories()
        {
            List<Category> list = new List<Category>();
            Category cat = new Category();
            cat.Id = 0;
            cat.Name = "-----Select------";
            list.Add(cat);
            string leadingspace = "\xA0\xA0\xA0\xA0\xA0";
            var categories=db.Categories.OrderBy(c=>c.Id).ThenBy(c=>c.Depth).ToList();
            foreach(var category in categories)
            {
                string addleadingspace = string.Empty;
                for (int i = 1; i <= category.Depth; i++)
                {
                    addleadingspace += leadingspace;
                }
                category.Name =addleadingspace + category.Name;
                list.Add(category);
               // category.Name = "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0" + category.Name;
            }

            return list;
        }

        public List<Group> getDdlGroups()
        {
            List<Group> list = new List<Group>();
            Group grp = new Group();
            grp.Id = 0;
            grp.Name = "-----Select------";
            list.Add(grp);
            string leadingspace = "\xA0\xA0\xA0\xA0\xA0";
            //var groups = db.Groups.OrderBy(c => c.Id).ThenBy(c => c.Depth).ToList();
            //foreach (var group in groups)
            //{
            //    string addleadingspace = string.Empty;
            //    //for (int i = 1; i <= group.Depth; i++)
            //    //{
            //    //    addleadingspace += leadingspace;
            //    //}
            //    group.Name = addleadingspace + group.Name;
            //    list.Add(group);
            //    // category.Name = "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0" + category.Name;
            //}

            return list;
        }

        public List<Warehouse> getDdlWarehouses(long companyid,long branchid)
        {
             List<Warehouse> list=new List<Warehouse>();
            if(branchid==0)
               list = db.Warehouses.Where(d => (d.Companyid == companyid)).ToList();
            else
               list = db.Warehouses.Where(d => (d.Companyid == companyid && d.Branchid == branchid)).ToList();
            return list;
        }
        public List<Tax> getDdlTaxes(long userid,long companyid, long branchid)
        {
            List<Tax> list = new List<Tax>();
            if(branchid==0)
             list = db.Taxes.Where(d => (d.CompanyId == companyid )).ToList();
            else
                list = db.Taxes.Where(d => (d.CompanyId == companyid && d.BranchId == branchid)).ToList();
            return list;
        }
        public List<Product> getDdlProducts(long companyid, long branchid)
        {
            List<Product> list = new List<Product>();
            if(branchid==0)
              list = db.Products.Where(d => (d.companyid == companyid)).ToList();
            else
              list = db.Products.Where(d => (d.companyid == companyid && d.Branchid == branchid)).ToList();
            return list;
        }

        public IEnumerable<Category> GetList(int? parentID = null)
        {
           
            var items=db.Categories.Where(x => x.GId == parentID).OrderBy(x => x.SortOrder).ToList();
            foreach (var item in items)
            {

                yield return item;

                foreach (var child in GetList(item.Id))
                {
                  
                    yield return child;
                }

            }
        }
    }
    
}