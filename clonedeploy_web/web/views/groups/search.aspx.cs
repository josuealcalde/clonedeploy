﻿/*  
    CrucibleWDS A Windows Deployment Solution
    Copyright (C) 2011  Jon Dolny

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Global;
using Models;


namespace views.groups
{
    public partial class GroupSearch : Page
    {
        private readonly BLL.Group _bllGroup = new BLL.Group();
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in gvGroups.Rows)
            {
                var cb = (CheckBox) row.FindControl("chkSelector");
                if (cb == null || !cb.Checked) continue;
                var dataKey = gvGroups.DataKeys[row.RowIndex];
                if (dataKey == null) continue;
                _bllGroup.DeleteGroup(Convert.ToInt32(dataKey.Value));
            }


            PopulateGrid();
            Master.Master.Msgbox(Utility.Message);
        }

        public string GetSortDirection(string sortExpression)
        {
            if (ViewState[sortExpression] == null)
                ViewState[sortExpression] = "Desc";
            else
                ViewState[sortExpression] = ViewState[sortExpression].ToString() == "Desc" ? "Asc" : "Desc";

            return ViewState[sortExpression].ToString();
        }

        protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            PopulateGrid();
            List<Group> listGroups = (List<Group>)gvGroups.DataSource;
            switch (e.SortExpression)
            {
                case "Name":
                    listGroups = GetSortDirection(e.SortExpression) == "Asc" ? listGroups.OrderBy(g => g.Name).ToList() : listGroups.OrderByDescending(g => g.Name).ToList();
                    break;
                case "Image":
                    listGroups = GetSortDirection(e.SortExpression) == "Asc" ? listGroups.OrderBy(g => g.Image).ToList() : listGroups.OrderByDescending(g => g.Image).ToList();
                    break;
                case "Type":
                    listGroups = GetSortDirection(e.SortExpression) == "Asc" ? listGroups.OrderBy(g => g.Type).ToList() : listGroups.OrderByDescending(g => g.Type).ToList();
                    break;
               
            } 
            gvGroups.DataSource = listGroups;
            gvGroups.DataBind();
            foreach (GridViewRow row in gvGroups.Rows)
            {
                var group = new Models.Group();
                var lbl = row.FindControl("lblCount") as Label;
                var dataKey = gvGroups.DataKeys[row.RowIndex];
                if (dataKey != null)
                    group = _bllGroup.GetGroup(Convert.ToInt32(dataKey.Value));
                if (row.Cells[4].Text == "smart")
                {
                   
                       
                
                    //FIX ME
                    //if (lbl != null) lbl.Text = @group.SearchSmartHosts(@group.Expression).Count.ToString();
                }
                else if (lbl != null)
                {
                    lbl.Text = new BLL.GroupMembership().GetGroupMemberCount(group.Id);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {        
            if (IsPostBack) return;
            Master.Master.Msgbox(Utility.Message); //Display message after redirect to this page.
            PopulateGrid();
        }

        protected void PopulateGrid()
        {
           

            gvGroups.DataSource = _bllGroup.SearchGroups(txtSearch.Text);

            gvGroups.DataBind();

            foreach (GridViewRow row in gvGroups.Rows)
            {
                var group = new Models.Group();
                var lbl = row.FindControl("lblCount") as Label;
                var dataKey = gvGroups.DataKeys[row.RowIndex];
                if (dataKey != null)
                    group = _bllGroup.GetGroup(Convert.ToInt32(dataKey.Value));
                
                if (row.Cells[4].Text == "smart")
                {
                   

                    //if (lbl != null) lbl.Text = @group.SearchSmartHosts(@group.Expression).Count.ToString();
                }
                else if (lbl != null)
                {

                    lbl.Text = new BLL.GroupMembership().GetGroupMemberCount(group.Id);
                }
            }


            lblTotal.Text = gvGroups.Rows.Count + " Result(s) / " + _bllGroup.TotalCount() + " Total Group(s)";
        }

        protected void search_Changed(object sender, EventArgs e)
        {
            PopulateGrid();
        }

        protected void SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            var hcb = (CheckBox) gvGroups.HeaderRow.FindControl("chkSelectAll");

            ToggleCheckState(hcb.Checked);
        }

        private void ToggleCheckState(bool checkState)
        {
            foreach (GridViewRow row in gvGroups.Rows)
            {
                var cb = (CheckBox) row.FindControl("chkSelector");
                if (cb != null)
                    cb.Checked = checkState;
            }
        }
    }
}