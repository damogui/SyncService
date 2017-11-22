using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SyncService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            var num = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["timeinterval"]);

            System.Threading.Timer timer = new System.Threading.Timer(Timer1_Elapsed, null, 0, num);
        }

        private void Timer1_Elapsed(object o)
        {

            //linux 下 mono 运行windows服务 命令:
            //mono-service -l:/var/run/myserver.lock /usr/local/myserver/myserver.exe

            try
            {
                //TeacherDal.CheckAbnormalClassEndTimer();

                //TeacherDal.CheckAbnormalCoursePicTimer();

            }
            catch (Exception ex)
            {

            }


        }

        protected override void OnStop()
        {
        }
    }
}
