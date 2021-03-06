﻿using System;
using System.Web.UI.WebControls;
using Helpers;

public partial class views_users_removemembers : BasePages.Users
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        PopulateGrid();
    }

    protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        ChkAll(gvUsers);
    }

    protected void PopulateGrid()
    {
        gvUsers.DataSource = BLL.UserGroup.GetGroupMembers(CloneDeployUserGroup.Id,txtSearch.Text);
        gvUsers.DataBind();
        lblTotal.Text = gvUsers.Rows.Count + " Result(s) / " + BLL.UserGroup.MemberCount(CloneDeployUserGroup.Id) + " Total User(s)";
    }

    protected void search_Changed(object sender, EventArgs e)
    {
        PopulateGrid();
    }

    protected void btnRemoveSelected_OnClick(object sender, EventArgs e)
    {
        RequiresAuthorization(Authorizations.Administrator);
       
        var successCount = 0;
        foreach (GridViewRow row in gvUsers.Rows)
        {
            var cb = (CheckBox)row.FindControl("chkSelector");
            if (cb == null || !cb.Checked) continue;
            var dataKey = gvUsers.DataKeys[row.RowIndex];
            if (dataKey != null)
            {
                var user = BLL.User.GetUser(Convert.ToInt32(dataKey.Value));
                user.UserGroupId = -1;

                if (BLL.User.UpdateUser(user).IsValid)
                    successCount++;
            }
        }
        EndUserMessage = "Successfully Removed " + successCount + " Users From The Group";
        PopulateGrid();
    }

}