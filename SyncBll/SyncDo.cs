using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SyncBll
{
    public class SyncDo
    {

        public decimal GetOrgNewInfo(int ogrId)
        {
            //`HistoryID`, `OrgID`, `ActionType`, `DisCount`, `OrgMoney`, `OrgGift`, `OrgValue`, `AfterValue`, `BookSetID`, `StudentID`, `SchoolID`, `SchoolName`, `OperationUser`, `Remarks`, `MfgRemarks`, `CreateTime`

            string sql = "SELECT   IFNULL(SUM(OrgMoney),0) FROM `mfg_org_transaction` WHERE orgid=@OrgId LIMIT 0, 1 ";
          object obj=DBHelper.GetScalarFile(sql,new MySqlParameter("@OrgId", ogrId));

            decimal leftMoney = Convert.ToDecimal(obj);

            return leftMoney;



        }



    }
}
