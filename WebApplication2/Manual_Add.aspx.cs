using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using FGL;
namespace WebApplication2
{
    public partial class Manual_Add : System.Web.UI.Page
    {
        DataHour dh;
        public int scheduleLineCode = 990000;
        protected void Page_Load(object sender, EventArgs e)
        {
            dh = new DataHour(scheduleLineCode);
        }
    }
}