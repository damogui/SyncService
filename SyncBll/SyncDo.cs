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
            object obj = DBHelper.GetScalarFile(sql, new MySqlParameter("@OrgId", ogrId));

            decimal leftMoney = Convert.ToDecimal(obj);

            return leftMoney;

            #region 事务
            // MySqlConnection conn =
            //             new MySqlConnection(connectionString);
            //            conn.Open();
            //            MySqlTransaction tran = conn.BeginTransaction();
            //            object obj = MySqlHelper.ExecuteScalar(connectionString, CommandType.Text, "SELECT  age FROM Test   WHERE id=2");
            //            int age = Convert.ToInt32(obj) + 1;
            //            int num1 = MySqlHelper.ExecuteNonQuery(tran, CommandType.Text, "INSERT INTO `ourtool`.`Test` ( `Name`, `CreateTime`, `Age`) VALUES ('梁朝伟', NOW(), 11) ;");
            //            int num2 = MySqlHelper.ExecuteNonQuery(connectionString, CommandType.Text, "UPDATE  Test  SET  age=" + age + " WHERE id=2");
            //
            //            int num3 = MySqlHelper.ExecuteNonQuery(tran, CommandType.Text, "INSERT INTO `ourtool`.`Test` ( `Name`, `CreateTime`, `Age`) VALUES ('周润发', NOW(), 11) ;");
            //
            //
            //            try
            //            {
            //                tran.Commit();
            //                conn.Dispose();
            //
            //
            //            }
            //            catch (Exception ex)
            //            {
            //                tran.Rollback();
            //                string msg = ex.Message;
            //
            //            } 
            #endregion

        }





    }
}
