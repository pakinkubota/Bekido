using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FGL;

namespace WebDataHour
{
    public partial class Bekido_Old : System.Web.UI.Page
    {
        public int scheduleLineCode = 920608;
        public DataHour dh;

        public void Page_Load(object sender, EventArgs e)
        {
            dh = new DataHour(scheduleLineCode, "c");
            if (!this.IsPostBack)
            {
                LoadData();
                //lblLineName.Text = "&nbsp;" +  dh.GetLineName() + "&nbsp;TT&nbsp;:&nbsp;" + dh.GetTaktTime() + "&nbsp;sec&nbsp;" + dh.timeStart.ToString("HH:mm") + "&nbsp;-&nbsp;" + dh.timeEnd.ToString("HH:mm");
                lblLineName.Text = "&nbsp;" + dh.lineName + "&nbsp;TT&nbsp;:&nbsp;" + "-" + "&nbsp;sec&nbsp;";
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        public void LoadData()
        {
            dh.LoadDatafull(gvwDetail);
            lblActual.Text = dh.GetActual().ToString();
            lblTarget.Text = dh.GetTarget().ToString();
            dh.loadBekido(lblBekido);
            lblPlan.Text = dh.GetPlan().ToString();
            dh.UpdateLastUpdate(lblStatus);
        }
    }
}