using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnDog_Click(object sender, EventArgs e)
        {
            imgShow.ImageUrl = "~/Pic/Dog.png";
        }

        protected void btnCat_Click(object sender, EventArgs e)
        {
            imgShow.ImageUrl = "~/Pic/Cat.png";
        }

        protected void btnBird_Click(object sender, EventArgs e)
        {
            imgShow.ImageUrl = "https://t1.gstatic.com/licensed-image?q=tbn:ANd9GcRR8sOcXZ9x7xmLKXIaS_wSPebx0fXpXa7hZLtS_3zQkjD-8TCDc_wNwXJE2qQ14fijhwLaZoK_OkZZx2qnvQM";
        }
    }
}