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
    public partial class Bekido_Cell_2 : System.Web.UI.Page
    {
        public int scheduleLineCode = 920604;
        public DataHour dh;

        public void Page_Load(object sender, EventArgs e)
        {
            dh = new DataHour(scheduleLineCode, "c");
            if (!this.IsPostBack)
            {
                dh.DisplayData(this.Page.Master);
            }
        }
    }
}