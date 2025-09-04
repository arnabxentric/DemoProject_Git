using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XenERP.Models;

namespace XenERP.Models
{
    public class TransactionClasses
    {
        private InventoryEntities db = new InventoryEntities();

        public string GenerateCode(string pretext,int count)
        {
            int num=100000000+count+1;
            string numtext=Convert.ToString(num).Substring(1);
            string code=pretext+ "-" + numtext;
            return code;
        }

    }
}