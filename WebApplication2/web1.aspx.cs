using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Age", typeof(Int32));
                dt.Columns.Add("Positiob", typeof(string));
                DataRow NewRow;
                NewRow = dt.NewRow();
                NewRow[0] = "Test";
                NewRow[1] = 99;
                NewRow[2] = "Trainee";
                dt.Rows.Add(NewRow);
                gvw1.DataSource = dt;
                gvw1.DataBind();
            }
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            int age;
            if (int.TryParse(txbAge.Text, out age))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Age", typeof(Int32));
                dt.Columns.Add("Positiob", typeof(string));
                foreach (GridViewRow row in gvw1.Rows)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < row.Cells.Count; j++)
                    {
                        dr[j] = row.Cells[j].Text;
                    }

                    dt.Rows.Add(dr);
                }
                gvw1.DataSource = dt;
                gvw1.DataBind();
                DataRow NewRow;
                NewRow = dt.NewRow();
                NewRow[0] = txbName.Text;
                NewRow[1] = age;
                NewRow[2] = txbPosition.Text;
                dt.Rows.Add(NewRow);
                gvw1.DataSource = dt;
                gvw1.DataBind();
            }
            else
            {
                lblWarning.Text = "Age is not a integer";
            }

        }
    }
}