using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics.Tracing;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FGL;

namespace FGL
{

    public class DataHour
    {
        public int scheduleLineCode;
        private string con_str = "Data Source=172.20.184.144;Persist Security Info=True;User ID=FGL_SQL;Password=Uzkg\">*I9WD))1fw";
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter adt;
        private string strSQL;
        private DataTable dt;
        public string lineName;
        public int taktTime;
        public int taktTimeActual;
        public TimeSpan timeStart;
        public TimeSpan timeEnd;
        public int workingSec;
        public int taktTimeRemain;
        private DataTable breakTime;
        public string line;
        public bool isWorkDay = true;
        public string pickingLampStaion;
        public DataHour(int schLine, string mode = "n", string pickingLampStation = "-")
        {
            line = mode;
            scheduleLineCode = schLine;
            pickingLampStaion = pickingLampStation;
            lineName = GetLineName().Trim();
            timeEnd = TimeSpan.Parse("20:00:00");
            timeStart = GetTimeStart();
            breakTime = GetBreakTime();
            if (timeStart == TimeSpan.Parse("00:00:00"))
            {
                timeEnd = TimeSpan.Parse("00:00:00");
                isWorkDay = false;
                return;
            }
            if (line != "c")
            {
                taktTimeRemain = GetRamainTime();
                taktTime = GetTaktTime();
                taktTimeActual = GetTaktTimeActual();
                timeEnd = GetTimeEnd();
                workingSec = GetWorkingSec();
                WorkingTimeAdjust();
            }
        }

        public string GetTimeStartString()
        {
            if (timeStart == TimeSpan.Parse("00:00:00"))
            {
                return "-";
            }
            else
            {
                return timeStart.ToString();
            }

        }

        public string GetTimeEndString()
        {
            if (timeEnd == TimeSpan.Parse("00:00:00"))
            {
                return "-";
            }
            else
            {
                return timeEnd.ToString();
            }

        }
        public string GetHeadLine()
        {
            return "&nbsp;" + lineName + "&nbsp;TT&nbsp;(PPC)&nbsp;:&nbsp;" + taktTimeActual + "(" + taktTime + ")" + "&nbsp;sec&nbsp;";
        }

        public void UpdateData()
        {
            if (!isWorkDay)
            {
                Console.WriteLine($"{scheduleLineCode} : Not Workday");
                using (con = new SqlConnection(con_str))
                {
                    con.Open();
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET TARGET = 0, ACTUAL = 0,STOP_MAX = '-',WORKING_SEC = 0,STOP_SEC = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }
                return;
            }

            UpdateStatus(0);

            List<Task> tasks = new List<Task>();


            if (line == "n")
            {
                tasks.Add(UpdateActual());
                tasks.Add(UpdateTarGet());
                tasks.Add(UpdateStopMax());
            }

            else if (line == "c")
            {
                tasks.Add(UpdateActualCell());
                tasks.Add(CheckTargetCell());
                tasks.Add(UpdatePlanCell());
            }

            tasks.Add(UpdateWorkingSec());
            tasks.Add(UpdateStopSec());
            Task.WaitAll(tasks.ToArray());
            UpdateStatus(1);
            Console.WriteLine($"{scheduleLineCode} : start {timeStart} , end {timeEnd}");

            //if (scheduleLineCode == 990000)
            //{
            //    foreach (DataRow row in breakTime.Rows)
            //    {
            //        Console.WriteLine($"{scheduleLineCode} : {row["BREAK_START"]} , {row["BREAK_FINISH"]}, {row["BREAK_SEC"]}");
            //    }
            //}
        }

        public async Task UpdatePlanCell()
        {
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();

                //Get Actual
                strSQL = $"select count(id_no) as amount\r\nfrom openquery(PRDACT,'select a.*,b.plan_prod_finish_date from stdadmin.prod_result a join stdadmin.prod_info b on a.ID_NO = b.ID_NO where a.station_no = ''{pickingLampStaion}'' and to_char(plan_prod_finish_date,''yyyymmdd'') = to_char(sysdate,''yyyymmdd'')')";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);

                int totalPlanCell = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalPlanCell = Convert.ToInt32(row["amount"]);
                }
                strSQL = $"UPDATE [FGL_DATA].[dbo].[PLAN_CELL] SET [PLAN] = {totalPlanCell} where [CELL] = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
        }


        public void DisplayData(MasterPage master)
        {
            LoadDatafull(((GridView)master.FindControl("gvwDetail")));
            ((Label)master.FindControl("lblWorkingPeriod")).Text = "Working : " + GetTimeStartString() + " - " + GetTimeEndString();
            ((Label)master.FindControl("lblActual")).Text = GetActual().ToString();
            ((Label)master.FindControl("lblTarget")).Text = GetTarget().ToString();
            loadBekido(((Label)master.FindControl("lblBekido")));
            ((Label)master.FindControl("lblPlan")).Text = GetPlan().ToString();
            UpdateLastUpdate(((Label)master.FindControl("lblStatus")));
            ((Label)master.FindControl("lblHeadline")).Text = GetHeadLine();
        }

        public DataTable LoadData()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                dt = new DataTable();
                strSQL = $"select left(CONVERT(VARCHAR, TIME_START, 108),5) + ' - ' + left(CONVERT(VARCHAR, TIME_FINISH, 108),5) as Time,[TARGET] as [Target],[ACTUAL] as [Actual],FORMAT(iiF([WORKING_SEC]=0,0,cast(([WORKING_SEC]-[STOP_SEC]) as float)/[WORKING_SEC]),'#,##0.0%') as Bekido,[STOP_MAX] as[ST(Stop Max)] from [FGL_DATA].[dbo].[BEKIDO_DATA] where SCHEDULED_LINE_CODE = {scheduleLineCode} order by id";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(dt);
                return dt;
            }
        }

        public int GetRamainTime()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"SELECT TOP (1) * FROM [FGL_DATA].[dbo].[TIME_REMAIN] where [SCHEDULED_LINE_CODE] = '{scheduleLineCode}' and CAST([DATE] as date) <> CAST(GETDATE() as date) order by [DATE] desc";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["TIME_REMAIN"]);
            }
        }
        public void LoadDatafull(GridView gvwDetail)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                DataTable da;
                while (true)
                {
                    da = new DataTable();
                    strSQL = $"SELECT [UPDATE_STATUS]  FROM[FGL_DATA].[dbo].[BEKIDO_UPDATE_STATUS]  where SCHEDULED_LINE_CODE = '{scheduleLineCode}'";
                    adt = new SqlDataAdapter(strSQL, con);
                    adt.Fill(da);
                    if (Convert.ToInt32(da.Rows[0]["UPDATE_STATUS"].ToString()) == 1)
                        break;
                }
                da = new DataTable();
                strSQL = $"select left(CONVERT(VARCHAR, TIME_START, 108),5) + ' - ' + left(CONVERT(VARCHAR, TIME_FINISH, 108),5) as Time,\r\n\tCAST([TARGET] AS varchar) as [Target],\r\n\tCAST([Actual] AS varchar) as [Actual],\r\n\tIIF([WORKING_SEC]=0,'-',\r\n\t\tIIF([STOP_SEC]<[WORKING_SEC],FORMAT(cast(([WORKING_SEC]-[STOP_SEC]) as float)/[WORKING_SEC],'#,##0.0%'),'0.0%')) as Bekido,\r\n\t[STOP_MAX] as[ST(Stop Max)]\r\nfrom [FGL_DATA].[dbo].[BEKIDO_DATA]\r\nwhere SCHEDULED_LINE_CODE = '{scheduleLineCode}'\r\norder by id"; // no running Total
                //strSQL = $"select left(CONVERT(VARCHAR, TIME_START, 108),5) + ' - ' + left(CONVERT(VARCHAR, TIME_FINISH, 108),5) as Time,\r\nCAST([TARGET] AS varchar) + ' / ' + CAST(sum([TARGET]) over(order by TIME_START rows unbounded preceding) AS varchar) as [Target],\r\n CAST([Actual] AS varchar) + ' / ' + CAST(sum([Actual]) over(order by TIME_START rows unbounded preceding) AS varchar) as [Actual],\r\nIIF([WORKING_SEC]=0,'-',\r\n\t\tIIF([STOP_SEC]<[WORKING_SEC],FORMAT(cast(([WORKING_SEC]-[STOP_SEC]) as float)/[WORKING_SEC],'#,##0.0%'),'0.0%')) as Bekido,\r\n[STOP_MAX] as[ST(Stop Max)]\r\nfrom [FGL_DATA].[dbo].[BEKIDO_DATA]\r\nwhere SCHEDULED_LINE_CODE = '{scheduleLineCode}'\r\norder by id"; // have running Total
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(da);
                gvwDetail.DataSource = da;
                gvwDetail.DataBind();
                for (int i = 0; i < da.Rows.Count; i++)
                {
                    GridViewRow gvRow = gvwDetail.Rows[i];
                    DataRow dataRow = da.Rows[i];
                    if (dataRow["Bekido"].ToString() != "-")
                    {
                        double bekido = Convert.ToDouble(dataRow["Bekido"].ToString().Remove(dataRow["Bekido"].ToString().Length - 1));
                        if (bekido <= 95)
                        {
                            gvRow.CssClass = "detail_data row_red";
                        }
                        if (bekido >= 95 && bekido <= 98)
                        {
                            gvRow.CssClass = "detail_data row_yellow";
                        }
                        if (bekido >= 98)
                        {
                            if (i % 2 == 1)
                                gvRow.CssClass = "detail_data row_odd";
                            else
                                gvRow.CssClass = "detail_data row_even";
                        }
                    }
                    else
                    {
                        if (i % 2 == 1)
                            gvRow.CssClass = "detail_data row_odd";
                        else
                            gvRow.CssClass = "detail_data row_even";
                    }

                    if (Convert.ToInt32(dataRow["Time"].ToString().Substring(0, 2)) > DateTime.Now.Hour || DateTime.Parse($"{Convert.ToInt32(dataRow["Time"].ToString().Substring(0, 2))}:00:00").TimeOfDay > timeEnd)
                    {
                        gvRow.Visible = false;
                    }

                    else
                        gvRow.Visible = true;
                }
            }
        }

        public void loadBekido(Label lblBekido)
        {
            string strBekido = GetBekido();
            lblBekido.Text = strBekido;
            if (strBekido == "-")
                lblBekido.CssClass = "bekido_label row_odd";
            else
            {
                double bekido = Convert.ToDouble(strBekido.Remove(strBekido.ToString().Length - 1));
                if (bekido <= 95)
                {
                    lblBekido.CssClass = "bekido_label row_red";
                }
                if (bekido >= 95 && bekido <= 98)
                {
                    lblBekido.CssClass = "bekido_label row_yellow";
                }
                if (bekido >= 98)
                {
                    lblBekido.CssClass = "bekido_label row_odd";
                }
            }
        }

        public void UpdateLastUpdate(Label lastUpdate)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                DataTable da = new DataTable();
                strSQL = $"SELECT [LAST_UPDATE]  FROM[FGL_DATA].[dbo].[BEKIDO_UPDATE_STATUS]  where SCHEDULED_LINE_CODE = '{scheduleLineCode}'";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(da);
                lastUpdate.Text = "Last Update : " + DateTime.Parse(da.Rows[0]["LAST_UPDATE"].ToString()).ToString("hh:mm:ss tt");
            }
        }

        public string GetLineName()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                dt = new DataTable();
                strSQL = $"SELECT [LINE_NAME] FROM [FGL_DATA].[dbo].[LINE_NAME] where [SCHEDULED_LINE_CODE] = {scheduleLineCode}";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(dt);
                return dt.Rows[0]["LINE_NAME"].ToString();
            }
        }

        public int GetTaktTime()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                dt = new DataTable();
                strSQL = $"select TAKT_TIME_PER_UNIT as takttime from OPENQUERY(PRDACT,'select * from STDADMIN.MS_SEQ_DLV_TAKT_TIME where TAKT_TIME_NAME = ''{lineName}''') where cast(SCOPE_FROM as date) <= cast(GETDATE() as date) and cast(SCOPE_TO as date) >= cast(getdate() as date)";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["takttime"]);
            }
        }

        public int GetTaktTimeActual()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                dt = new DataTable();
                strSQL = $"SELECT TOP(1)*\r\n  FROM [FGL_DATA].[dbo].[ACTUAL_TAKT_TIME]\r\n  where SCHEDULED_LINE_CODE = '{scheduleLineCode}'\r\n  order by UPDATE_DATE desc";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["TAKT_TIME"]);
            }
        }

        public string GetBekido()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                dt = new DataTable();
                strSQL = $"select iiF(SUM([WORKING_SEC]) = 0,'-',FORMAT(cast((SUM([WORKING_SEC])-SUM([STOP_SEC])) as float)/SUM([WORKING_SEC]),'#,##0.0%')) as Bekido from [FGL_DATA].[dbo].[BEKIDO_DATA] where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(dt);
                return dt.Rows[0]["Bekido"].ToString();
            }
        }

        public int GetActual()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"select sum([Actual]) as total_actual from [FGL_DATA].[dbo].[BEKIDO_DATA]  where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["total_actual"]);
            }
        }

        public int GetTarget()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"select sum([TARGET]) as total_target from [FGL_DATA].[dbo].[BEKIDO_DATA]  where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["total_target"]);
            }
        }

        public int GetPlan()
        {

            int plan = 0;
            if (!isWorkDay)
            {
                plan = 0;
                return plan;
            }

            if (line == "n")
            {
                double planDecimal = (Convert.ToDouble(workingSec) + taktTimeActual - taktTimeRemain) / taktTimeActual;
                plan = Convert.ToInt32(Math.Floor(planDecimal));
            }

            else if (line == "c")
            {
                using (con = new SqlConnection(con_str))
                {
                    con.Open();
                    strSQL = $"SELECT [PLAN] FROM [FGL_DATA].[dbo].[PLAN_CELL] where [CELL] = {scheduleLineCode}";
                    adt = new SqlDataAdapter(strSQL, con);
                    dt = new DataTable();
                    adt.Fill(dt);
                    plan = Convert.ToInt32(dt.Rows[0]["PLAN"]);
                }
            }
            return plan;
        }

        public void AddTarget(int hour)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                string strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Target = Target + 1 where DATEPART(HH,[TIME_START]) = {hour} and SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
        }

        public void SetTarget(int hour, int target)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                string strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Target = {target} where DATEPART(HH,[TIME_START]) = {hour} and SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddActual(int id)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                string strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Actual = Actual + 1 where ID = {id}  and SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
        }

        public TimeSpan GetTimeStart()
        {
            DateTime dateTime = new DateTime();
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                if (line == "n")
                    strSQL = $"SELECT [TIME_BAND_FROM],[TIME_BAND_TO] FROM [MFG_LineStop].[dbo].[IF_MS_PROD_PLAN_DAILY_WORK_BAND] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' and WORK_DATE = cast(GETDATE() as date)";
                else if (line == "c")
                    strSQL = $"SELECT [TIME_BAND_FROM],[TIME_BAND_TO] FROM [MFG_LineStop].[dbo].[IF_MS_PROD_PLAN_DAILY_WORK_BAND] where SCHEDULED_LINE_CODE = '990002' and WORK_DATE = cast(GETDATE() as date)";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    dateTime = dateTime.AddHours(Convert.ToInt32(row["TIME_BAND_FROM"].ToString().Substring(0, 2)));
                    dateTime = dateTime.AddMinutes(Convert.ToInt32(row["TIME_BAND_FROM"].ToString().Substring(2, 2)));
                }
                return dateTime.TimeOfDay;
            }
        }

        public TimeSpan GetTimeEnd()
        {
            DateTime dateTime = new DateTime();
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"SELECT [TIME_BAND_FROM],[TIME_BAND_TO] FROM [MFG_LineStop].[dbo].[IF_MS_PROD_PLAN_DAILY_WORK_BAND] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' and WORK_DATE = cast(GETDATE() as date)";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    dateTime = dateTime.AddHours(Convert.ToInt32(row["TIME_BAND_TO"].ToString().Substring(0, 2)));
                    dateTime = dateTime.AddMinutes(Convert.ToInt32(row["TIME_BAND_TO"].ToString().Substring(2, 2)));
                }
                return dateTime.TimeOfDay;
            }
        }

        public int GetWorkingSec()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"SELECT [ACTUAL_WORK_HOUR_SUM] FROM [MFG_LineStop].[dbo].[IF_MS_PROD_PLAN_DAILY_WORK_BAND] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' and WORK_DATE = cast(GETDATE() as date)";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return Convert.ToInt32(dt.Rows[0]["ACTUAL_WORK_HOUR_SUM"]);
            }
        }

        public void WorkingTimeAdjust()
        {
            using (con = new SqlConnection(con_str))
            {
                string eventChange;
                int changeSec;
                con.Open();
                strSQL = $"SELECT [ADJUST_WORK_HOUR],[EVENT_NAME],[REMARK] FROM [MFG_LineStop].[dbo].[MS_PROD_PLAN_DAILY_WORK_ADJUST]  where SCHEDULED_LINE_CODE = '{scheduleLineCode}' and WORK_DATE = cast(GETDATE() as date) and isActive = 1";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    eventChange = row["EVENT_NAME"].ToString().Trim();
                    changeSec = Convert.ToInt32(row["ADJUST_WORK_HOUR"]) * 60;
                    if (eventChange == "เปิด OT")
                        timeEnd += TimeSpan.FromSeconds(changeSec);
                    if (eventChange == "ยกเลิก OT")
                        timeEnd += TimeSpan.FromSeconds(changeSec);
                    if (eventChange == "เพิ่มกิจกรรม")
                        timeStart += TimeSpan.FromSeconds(-changeSec);
                    if (eventChange == "ยกเลิกกิจกรรม")
                        timeStart += TimeSpan.FromSeconds(-changeSec);
                    workingSec += changeSec;
                }

                strSQL = $"SELECT *  FROM [MFG_LineStop].[dbo].[IF_PROD_PLAN_DAILY_EVENT] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' and WORK_DATE = CAST(GETDATE() as date) and EVENT_NAME = 'Lunch60'";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                if (dt.Rows.Count > 0)
                    breakTime.Rows[1]["BREAK_FINISH"] = TimeSpan.Parse(breakTime.Rows[1]["BREAK_FINISH"].ToString()) + TimeSpan.FromMinutes(20);
            }

            TimeSpan calDatetime = timeStart;
            calDatetime += TimeSpan.FromSeconds(workingSec);
            foreach (DataRow row in breakTime.Rows)
            {

                if (calDatetime > DateTime.Parse(row["BREAK_START"].ToString()).TimeOfDay && timeStart < DateTime.Parse(row["BREAK_FINISH"].ToString()).TimeOfDay)
                {
                    calDatetime += TimeSpan.FromSeconds(Convert.ToInt32(row["BREAK_SEC"]));
                }
            }
            timeEnd = calDatetime;
        }

        public DataTable GetBreakTime()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                if (line == "n")
                    strSQL = $"SELECT *,DATEDIFF(SECOND,BREAK_START,BREAK_FINISH) as BREAK_SEC FROM [FGL_DATA].[dbo].[BREAK_TIME] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' order by BREAK_FINISH ";
                else if (line == "c")
                    strSQL = $"SELECT *,DATEDIFF(SECOND,BREAK_START,BREAK_FINISH) as BREAK_SEC FROM [FGL_DATA].[dbo].[BREAK_TIME] where SCHEDULED_LINE_CODE = '990002' order by BREAK_FINISH ";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return dt;
            }
        }

        public async Task UpdateTarGet()
        {
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Target = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
            int currentHour = 0;
            int currentCount = 0;
            TimeSpan oldCal;
            TimeSpan currentCal = timeStart;
            TimeSpan now = DateTime.Now.TimeOfDay;

            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"SELECT TOP (1) * FROM [FGL_DATA].[dbo].[TIME_REMAIN] where [SCHEDULED_LINE_CODE] = '{scheduleLineCode}' and CAST([DATE] as date) <> CAST(GETDATE() as date) order by [DATE] desc";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                currentCal += TimeSpan.FromSeconds(Convert.ToInt32(row["TIME_REMAIN"]));
                if (currentCal < now)
                {
                    AddTarget(currentCal.Hours);
                    currentCount++;
                    currentHour = currentCal.Hours;
                }


            }

            while (currentCal < now && currentCal <= timeEnd)
            {
                oldCal = currentCal;
                currentCal += TimeSpan.FromSeconds(taktTimeActual);
                foreach (DataRow row in breakTime.Rows)
                {
                    if (currentCal > DateTime.Parse(row["BREAK_START"].ToString()).TimeOfDay && oldCal <= DateTime.Parse(row["BREAK_START"].ToString()).TimeOfDay)
                    {
                        //Console.WriteLine("in break");
                        currentCal += TimeSpan.FromSeconds(Convert.ToInt32(row["BREAK_SEC"]));
                    }

                }
                if (currentCal <= now && currentCal <= timeEnd)
                {
                    if (currentCal.Hours != currentHour)
                    {
                        SetTarget(currentHour, currentCount);
                        currentCount = 0;
                        currentHour = currentCal.Hours;
                    }
                    //if (scheduleLineCode == 990000)
                    //{ 
                    //    Console.WriteLine(currentCal.ToString()); 
                    //} 
                    currentCount++;

                }
            }
            SetTarget(currentHour, currentCount);
        }

        public async Task UpdateActual()
        {
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();

                //Get Actual
                strSQL = $"select DATEPART(HOUR,UPDATE_DATE_10) as hour_start,count(ID_NO) as actual from OPENQUERY(PRDACT,'select ID_NO,UPDATE_DATE_10  from STDADMIN.PROD_INFO WHERE SCHEDULED_LINE_CODE = ''{scheduleLineCode}'' and UPDATE_DATE_10 is not null and UPDATE_DATE_10 < SYSDATE and UPDATE_DATE_10 > SYSDATE-1') where cast(UPDATE_DATE_10 as date) = cast(GETDATE() as date) group by DATEPART(HOUR, UPDATE_DATE_10) order by hour_start";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);

                // Reset Actual
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Actual = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();


                // Add Actual
                foreach (DataRow row in dt.Rows)
                {
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Actual = {row["actual"]} where DATEPART(HOUR,TIME_START) = {row["hour_start"]} and SCHEDULED_LINE_CODE = {scheduleLineCode}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task UpdateActualCell()
        {
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();

                //Get Actual
                strSQL = $"select DATEPART(HOUR,UPDATE_DATE_10) as hour_start,count(ID_NO) as actual from OPENQUERY(PRDACT,'select ID_NO,UPDATE_DATE_10  from STDADMIN.PROD_INFO WHERE UPDATE_BY_10 = ''{scheduleLineCode}'' and UPDATE_DATE_10 is not null and UPDATE_DATE_10 < SYSDATE and UPDATE_DATE_10 > SYSDATE-1') where cast(UPDATE_DATE_10 as date) = cast(GETDATE() as date) group by DATEPART(HOUR, UPDATE_DATE_10) order by hour_start";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);

                // Reset Actual
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Actual = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();

                // Add Actual
                foreach (DataRow row in dt.Rows)
                {
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Actual = {row["actual"]} where DATEPART(HOUR,TIME_START) = {row["hour_start"]} and SCHEDULED_LINE_CODE = {scheduleLineCode}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task UpdateStopMax()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"DECLARE @table as TABLE (TIME_STOP_STOP TIME(0),TIME_STOP_START  TIME(0),LINE_STOP_SEC int,SCHEDULED_LINE_CODE nvarchar(50),[type] int,NEXT_HOUR TIME(0),STOPPED_STATION nvarchar(50)) \r\nDECLARE @Groupsec AS int = 5\r\nDECLARE @USE_DATE AS datetime = getdate()\r\n\r\nINSERT INTO @table\r\nSELECT CONVERT(TIME(0), LINE_STOP_STOP) as TIME_STOP_STOP,CONVERT(TIME(0), LINE_STOP_START) as TIME_STOP_START,\r\n\tIIF(LINE_STOP_SEC is null,DATEDIFF(second,LINE_STOP_STOP,GETDATE()),LINE_STOP_SEC) LINE_STOP_SEC,SCHEDULED_LINE_CODE,\r\n\t(case when datepart(HOUR,LINE_STOP_STOP) = datepart(HOUR,LINE_STOP_START) then 0\r\n\t\twhen datepart(HOUR,LINE_STOP_STOP) != datepart(HOUR,LINE_STOP_START) then 1\r\n\t\tend ) as 'Type',\r\n\t\tCONVERT(TIME(0),dateadd(hour, datediff(hour, 0, LINE_STOP_STOP)+1 , 0)) as 'NEXT_HOUR',\r\n\t\tSTOPPED_STATION\r\nFROM(\r\n\tselect *,sum(IsNewGroup) OVER (ORDER BY LINE_STOP_STOP desc) as groupID\r\n\tfrom (\tselect *\r\n\t\t\t\t\t,(case when ABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP))  > @Groupsec\r\n\t\t\t\t\t\t\tthen 1 else 0 end) as IsNewGroup\r\n\t\t\t\t\t,ABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP)) as secdiff\r\n\t\t\t\tfrom [MFG_LineStop].[dbo].[LINESTOP_LOG]\r\n\t\t\t\twhere SCHEDULED_LINE_CODE LIKE '{scheduleLineCode}'\r\n\t\t\t\t\tand cast(LINE_STOP_STOP as date) = cast(LINE_STOP_START as date)\r\n\t\t\t\t\tand LINE_STOP_SEC > 0\r\n\t\t\t\t\tand cast(LINE_STOP_STOP as date) = cast(@USE_DATE as date)\r\n\t\t)a\r\n\t)c\r\n\r\nselect k.HOUR,k.STOPPED_STATION\r\nfrom (\r\n\tselect a.USE_HOUR as 'HOUR',SUM(a.STOP_SEC) as TOTAL_STOP,STOPPED_STATION,ROW_NUMBER() OVER(PARTITION BY a.USE_HOUR order by SUM(a.STOP_SEC) desc) as NO\r\n\tfrom(\r\n\t\tselect datepart(HOUR,TIME_STOP_STOP) as 'HOUR',TIME_STOP_STOP,TIME_STOP_START,LINE_STOP_SEC, NEXT_HOUR, LINE_STOP_SEC as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR',STOPPED_STATION\r\n\t\tfrom @table\r\n\t\twhere [type] = 0\r\n\t\tunion all\r\n\t\tselect datepart(HOUR,TIME_STOP_STOP) as 'HOUR',TIME_STOP_STOP,TIME_STOP_START,LINE_STOP_SEC,NEXT_HOUR,DATEDIFF(s,TIME_STOP_STOP,NEXT_HOUR) as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR',STOPPED_STATION\r\n\t\tfrom @table\r\n\t\twhere [type] = 1\r\n\t\tunion all\r\n\t\tselect datepart(HOUR,TIME_STOP_STOP) as 'HOUR',TIME_STOP_STOP,TIME_STOP_START,LINE_STOP_SEC,NEXT_HOUR,DATEDIFF(s,NEXT_HOUR,TIME_STOP_START) as STOP_SEC,datepart(HOUR,NEXT_HOUR) as 'USE_HOUR',STOPPED_STATION\r\n\t\tfrom @table\r\n\t\twhere [type] = 1\r\n\t)a\r\n\tgroup by USE_HOUR,STOPPED_STATION\r\n) k\r\nwhere k.NO = 1";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET STOP_MAX = '-' where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();

            }
            foreach (DataRow row in dt.Rows)
            {
                using (con = new SqlConnection(con_str))
                {
                    await con.OpenAsync();
                    int hour = Convert.ToInt32(row["HOUR"]);
                    string stMax = row["STOPPED_STATION"].ToString();
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET STOP_MAX = '{stMax}' where SCHEDULED_LINE_CODE = {scheduleLineCode} and DATEPART(HOUR,TIME_START) = {hour}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task UpdateStopSec()
        {
            DataTable dt = GetHourlyStop();
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET STOP_SEC = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();

                foreach (DataRow row in dt.Rows)
                {
                    int hour = Convert.ToInt32(row["HOUR"]);
                    int totalStop = Convert.ToInt32(row["TOTAL_STOP"]);
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET STOP_SEC = {totalStop} where SCHEDULED_LINE_CODE = {scheduleLineCode} and DATEPART(HOUR,TIME_START) = {hour}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task UpdateWorkingSec()
        {
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();
                strSQL = $"SELECT DATEPART(hour,[TIME_START]) as [TIME_START],DATEPART(hour,[TIME_FINISH]) as [TIME_FINISH] ,[WORKING_SEC]  FROM [FGL_DATA].[dbo].[BEKIDO_DATA] where SCHEDULED_LINE_CODE = '{scheduleLineCode}' order by TIME_FINISH";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);

                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET WORKING_SEC = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();

                foreach (DataRow row in dt.Rows)
                {

                    TimeSpan start = DateTime.Parse($"{Convert.ToInt32(row["TIME_START"].ToString())}:00:00").TimeOfDay;
                    TimeSpan finish = DateTime.Parse($"{Convert.ToInt32(row["TIME_FINISH"].ToString())}:00:00").TimeOfDay;

                    if (start > DateTime.Now.TimeOfDay || timeStart > DateTime.Now.TimeOfDay)
                        return;

                    if (finish > DateTime.Now.TimeOfDay && start < DateTime.Now.TimeOfDay)
                        finish = DateTime.Now.TimeOfDay;

                    int workingSec = Convert.ToInt32((finish - start).TotalSeconds);
                    if (timeStart > start && timeStart < finish)
                        workingSec -= Convert.ToInt32((timeStart - start).TotalSeconds);
                    if (timeEnd > start && timeEnd < finish)
                        workingSec -= Convert.ToInt32((finish - timeEnd).TotalSeconds);
                    foreach (DataRow rowb in breakTime.Rows)
                    {
                        TimeSpan breakStart = DateTime.Parse(rowb["BREAK_START"].ToString()).TimeOfDay;
                        TimeSpan breakFinish = DateTime.Parse(rowb["BREAK_FINISH"].ToString()).TimeOfDay;
                        if (breakFinish <= start)
                            ;
                        else if (breakFinish >= start && breakFinish <= finish)
                        {
                            if (breakStart < start)
                                workingSec -= Convert.ToInt32((breakFinish - start).TotalSeconds);
                            else if (breakStart >= start)
                                workingSec -= Convert.ToInt32((breakFinish - breakStart).TotalSeconds);
                        }
                        else if (breakFinish > finish)
                        {
                            if (breakStart >= finish)
                                ;
                            else if (breakStart <= start)
                                workingSec = 0;

                            else if (breakStart > finish)
                                workingSec -= Convert.ToInt32((breakStart - finish).TotalSeconds);
                        }
                    }
                    strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET WORKING_SEC = {workingSec} where SCHEDULED_LINE_CODE = {scheduleLineCode} and DATEPART(HOUR,TIME_START) = {start.Hours}";
                    cmd = new SqlCommand(strSQL, con);
                    cmd.ExecuteNonQuery();
                }

            }
        }

        public DataTable GetHourlyStop()
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                if (line == "n")
                    strSQL = $"DECLARE @table as TABLE (TIME_STOP_STOP TIME(0),TIME_STOP_START  TIME(0),LINE_STOP_SEC int,SCHEDULED_LINE_CODE nvarchar(50),[type] int,NEXT_HOUR_1 TIME(0),NEXT_HOUR_2 TIME(0)) \r\nDECLARE @Groupsec AS int = 5\r\nDECLARE @USE_DATE AS datetime = getdate()\r\n\r\nINSERT INTO @table\r\nSELECT CONVERT(TIME(0), LINE_STOP_STOP) as TIME_STOP_STOP,\r\n\tCONVERT(TIME(0), LINE_STOP_START) as TIME_STOP_START,\r\n\tLINE_STOP_SEC,SCHEDULED_LINE_CODE,\r\n\tdatepart(HOUR,LINE_STOP_START) - datepart(HOUR,LINE_STOP_STOP) as 'Type',\r\n\tCONVERT(TIME(0),dateadd(hour, datediff(hour, 0, LINE_STOP_STOP)+1 , 0)) as 'NEXT_HOUR_1',\r\n\tCONVERT(TIME(0),dateadd(hour, datediff(hour, 0, LINE_STOP_STOP)+2 , 0)) as 'NEXT_HOUR_2'\r\nFROM(\r\n\tselect *\r\n\t\t,ROW_NUMBER()OVER(\r\n\t\t\tPARTITION BY groupID\r\n\t\t\torder by LINE_STOP_START desc) as 'NO'\r\n\tfrom(\r\n\t\tselect *,sum(IsNewGroup) OVER (ORDER BY LINE_STOP_STOP desc) as groupID\r\n\t\tfrom (select LINE_STOP_STOP,\r\n\t\t\t\tSCHEDULED_LINE_CODE,\r\n\t\t\t\tIIF(LINE_STOP_SEC is null,DATEDIFF(second,LINE_STOP_STOP,GETDATE()),LINE_STOP_SEC) as LINE_STOP_SEC,\r\n\t\t\t\tIIF(LINE_STOP_START is null,GETDATE(),LINE_STOP_START) as LINE_STOP_START,\r\n\t\t\t\t(case when ABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP))  > @Groupsec\r\n\t\t\t\t\tthen 1 else 0 end) as IsNewGroup,\r\n\t\t\t\tABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP)) as secdiff\r\n\t\t\tfrom [MFG_LineStop].[dbo].[LINESTOP_LOG]\r\n\t\t\twhere SCHEDULED_LINE_CODE LIKE '{scheduleLineCode}'\r\n\t\t\t\tand (cast(LINE_STOP_STOP as date) = cast(LINE_STOP_START as date) or LINE_STOP_START is null)\r\n\t\t\t\tand (LINE_STOP_SEC > 0 or LINE_STOP_SEC is null)\r\n\t\t\t\tand cast(LINE_STOP_STOP as date) = cast(getdate() as date)\r\n\t\t )a\r\n\t)b\r\n)c\r\nwhere NO = '1'\r\n\r\nselect a.USE_HOUR as 'HOUR',SUM(a.STOP_SEC) as TOTAL_STOP\r\nfrom(\r\n\tselect LINE_STOP_SEC as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 0\r\n\tunion all\r\n\tselect DATEDIFF(s,TIME_STOP_STOP,NEXT_HOUR_1) as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 1 \r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_1,TIME_STOP_START) as STOP_SEC,datepart(HOUR,NEXT_HOUR_1) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 1\r\n\tunion all\r\n\tselect DATEDIFF(s,TIME_STOP_STOP,NEXT_HOUR_1) as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_1,NEXT_HOUR_2) as STOP_SEC,datepart(HOUR,NEXT_HOUR_1) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_2,TIME_STOP_START) as STOP_SEC,datepart(HOUR,NEXT_HOUR_2) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n)a\r\ngroup by USE_HOUR\r\norder by USE_HOUR";
                else if (line == "c")
                    strSQL = $"DECLARE @table as TABLE (TIME_STOP_STOP TIME(0),TIME_STOP_START  TIME(0),LINE_STOP_SEC int,SCHEDULED_LINE_CODE nvarchar(50),[type] int,NEXT_HOUR_1 TIME(0),NEXT_HOUR_2 TIME(0)) \r\nDECLARE @Groupsec AS int = 5\r\nDECLARE @USE_DATE AS datetime = GETDATE()\r\n\r\n\r\nINSERT INTO @table\r\nSELECT CONVERT(TIME(0), LINE_STOP_STOP) as TIME_STOP_STOP,\r\n\tCONVERT(TIME(0), LINE_STOP_START) as TIME_STOP_START,\r\n\tLINE_STOP_SEC,SCHEDULED_LINE_CODE,\r\n\tdatepart(HOUR,LINE_STOP_START) - datepart(HOUR,LINE_STOP_STOP) as 'Type',\r\n\tCONVERT(TIME(0),dateadd(hour, datediff(hour, 0, LINE_STOP_STOP)+1 , 0)) as 'NEXT_HOUR_1',\r\n\tCONVERT(TIME(0),dateadd(hour, datediff(hour, 0, LINE_STOP_STOP)+2 , 0)) as 'NEXT_HOUR_2'\r\nFROM(\r\n\tselect *\r\n\t\t,ROW_NUMBER()OVER(\r\n\t\t\tPARTITION BY groupID\r\n\t\t\torder by LINE_STOP_START desc) as 'NO'\r\n\tfrom(\r\n\t\tselect *,sum(IsNewGroup) OVER (ORDER BY LINE_STOP_STOP desc) as groupID\r\n\t\tfrom (\tselect LINE_STOP_STOP,\r\n\t\t\t\tSCHEDULED_LINE_CODE,\r\n\t\t\t\tIIF(LINE_STOP_SEC is null,DATEDIFF(second,LINE_STOP_STOP,GETDATE()),LINE_STOP_SEC) as LINE_STOP_SEC,\r\n\t\t\t\tIIF(LINE_STOP_START is null,GETDATE(),LINE_STOP_START) as LINE_STOP_START,\r\n\t\t\t\t\t\t(case when ABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP))  > @Groupsec\r\n\t\t\t\t\t\t\t\tthen 1 else 0 end) as IsNewGroup\r\n\t\t\t\t\t\t,ABS(DATEDIFF(s,lag(LINE_STOP_STOP) over (order by LINE_STOP_STOP desc),LINE_STOP_STOP)) as secdiff\r\n\t\t\t\t\tfrom [MFG_LineStop].[dbo].[LINESTOP_LOG]\r\n\t\t\t\t\twhere SCHEDULED_LINE_CODE LIKE '990002'\r\n\t\t\t\t\t\tand (cast(LINE_STOP_STOP as date) = cast(LINE_STOP_START as date) or LINE_STOP_START is null)\r\n\t\t\t\t\t\tand (LINE_STOP_SEC > 0 or LINE_STOP_SEC is null)\r\n\t\t\t\t\t\tand LINE = '{lineName}'\r\n\t\t\t\t\t\tand cast(LINE_STOP_STOP as date) = cast(@USE_DATE as date)\r\n\t\t )a\r\n\t)b\r\n)c\r\nwhere NO = '1'\r\n\r\nselect a.USE_HOUR as 'HOUR',SUM(a.STOP_SEC) as TOTAL_STOP\r\nfrom(\r\n\tselect LINE_STOP_SEC as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 0\r\n\tunion all\r\n\tselect DATEDIFF(s,TIME_STOP_STOP,NEXT_HOUR_1) as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 1 \r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_1,TIME_STOP_START) as STOP_SEC,datepart(HOUR,NEXT_HOUR_1) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 1\r\n\tunion all\r\n\tselect DATEDIFF(s,TIME_STOP_STOP,NEXT_HOUR_1) as STOP_SEC,datepart(HOUR,TIME_STOP_STOP) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_1,NEXT_HOUR_2) as STOP_SEC,datepart(HOUR,NEXT_HOUR_1) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n\tunion all\r\n\tselect DATEDIFF(s,NEXT_HOUR_2,TIME_STOP_START) as STOP_SEC,datepart(HOUR,NEXT_HOUR_2) as 'USE_HOUR'\r\n\tfrom @table\r\n\twhere [type] = 2\r\n)a\r\ngroup by USE_HOUR\r\norder by USE_HOUR";
                adt = new SqlDataAdapter(strSQL, con);
                dt = new DataTable();
                adt.Fill(dt);
                return dt;
            }
        }

        public async Task CheckTargetCell()
        {
            DataTable da;
            using (con = new SqlConnection(con_str))
            {
                await con.OpenAsync();
                da = new DataTable();
                strSQL = $"SELECT DATEPART(HOUR,TIME_START) as TIME_START,[TARGET] as [Target] from [FGL_DATA].[dbo].[BEKIDO_DATA] where SCHEDULED_LINE_CODE = {scheduleLineCode} order by id";
                adt = new SqlDataAdapter(strSQL, con);
                adt.Fill(da);
            }
            foreach (DataRow row in da.Rows)
            {
                int target = Convert.ToInt32(row["Target"]);
                int hour = Convert.ToInt32(row["TIME_START"].ToString());
                if (target > 0 && hour > DateTime.Now.Hour)
                {
                    using (con = new SqlConnection(con_str))
                    {
                        await con.OpenAsync();
                        strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_DATA] SET Target = 0 where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                        cmd = new SqlCommand(strSQL, con);
                        cmd.ExecuteNonQuery();
                    }
                    return;
                }
            }
        }

        public void UpdateStatus(int status)
        {
            using (con = new SqlConnection(con_str))
            {
                con.Open();
                strSQL = $"UPDATE [FGL_DATA].[dbo].[BEKIDO_UPDATE_STATUS] SET UPDATE_STATUS = {status},LAST_UPDATE = GETDATE() where SCHEDULED_LINE_CODE = {scheduleLineCode}";
                cmd = new SqlCommand(strSQL, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}