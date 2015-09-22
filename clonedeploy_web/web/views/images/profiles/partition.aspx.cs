﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using Global;
using Models;

public partial class views_images_profiles_partition : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ddlPartitionMethod.Text = Master.ImageProfile.PartitionMethod;
            chkDownForceDynamic.Checked = Convert.ToBoolean(Master.ImageProfile.ForceDynamicPartitions);
          
            DisplayLayout();
        }
    }

    protected void btnUpdatePartitions_OnClick(object sender, EventArgs e)
    {
        var imageProfile = Master.ImageProfile;
        imageProfile.PartitionMethod = ddlPartitionMethod.Text;
        imageProfile.ForceDynamicPartitions = Convert.ToInt16(chkDownForceDynamic.Checked);

        if (ddlPartitionMethod.SelectedIndex == 2)
        {
            var fixedLineEnding = scriptEditorText.Value.Replace("\r\n", "\n");
            imageProfile.CustomPartitionScript = fixedLineEnding;
        }
        else if (ddlPartitionMethod.SelectedIndex == 3)
        {
            new ImageProfilePartition().DeleteAllForProfile(Master.ImageProfile.Id);
            imageProfile.CustomPartitionScript = "";
            foreach (GridViewRow row in gvLayout.Rows)
            {
                var cb = (CheckBox)row.FindControl("chkSelector");
                if (cb == null || !cb.Checked) continue;
                var dataKey = gvLayout.DataKeys[row.RowIndex];
                if (dataKey == null) continue;
                var profilePartitionLayout = new ImageProfilePartition()
                {
                    LayoutId = Convert.ToInt16(dataKey.Value),
                    ProfileId = Master.ImageProfile.Id            
                };
                profilePartitionLayout.Create();
            }
        }
        else
        {
            imageProfile.CustomPartitionScript = "";
        }
        new BLL.LinuxProfile().UpdateProfile(imageProfile);
        new Utility().Msgbox(Utility.Message);
    }

    protected void ddlPartitionMethod_OnSelectedIndexChanged(object sender, EventArgs e)
    {
      DisplayLayout();
    }

    protected void DisplayLayout()
    {
        if (ddlPartitionMethod.SelectedIndex == 2)
        {
            customScript.Visible = true;
            customLayout.Visible = false;
            scriptEditorText.Value = Master.ImageProfile.CustomPartitionScript;
        }
        else if (ddlPartitionMethod.SelectedIndex == 3)
        {
            customScript.Visible = false;
            customLayout.Visible = true;
            PopulateGrid();
            var profilePartitionLayouts = new ImageProfilePartition().Search(Master.ImageProfile.Id);
            foreach (GridViewRow row in gvLayout.Rows)
            {
                var cb = (CheckBox) row.FindControl("chkSelector");
                var dataKey = gvLayout.DataKeys[row.RowIndex];
                if (dataKey == null) continue;
                foreach (var profilePartitionLayout in profilePartitionLayouts)
                {
                    if (profilePartitionLayout.LayoutId == Convert.ToInt16(dataKey.Value))
                        cb.Checked = true;
                }
            }

        }
        else
        {
            customScript.Visible = false;
            customLayout.Visible = false;
        }
    }
    protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        var hcb = (CheckBox)gvLayout.HeaderRow.FindControl("chkSelectAll");

        ToggleCheckState(hcb.Checked);
    }

    public string GetSortDirection(string sortExpression)
    {
        if (ViewState[sortExpression] == null)
            ViewState[sortExpression] = "Desc";
        else
            ViewState[sortExpression] = ViewState[sortExpression].ToString() == "Desc" ? "Asc" : "Desc";

        return ViewState[sortExpression].ToString();
    }

    protected void PopulateGrid()
    {
        var layout = new PartitionLayout();
        gvLayout.DataSource = layout.Search("");
        gvLayout.DataBind();
    }

    protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        PopulateGrid();
        List<PartitionLayout> listLayouts = (List<PartitionLayout>)gvLayout.DataSource;
        switch (e.SortExpression)
        {
            case "Name":
                listLayouts = GetSortDirection(e.SortExpression) == "Asc" ? listLayouts.OrderBy(l => l.Name).ToList() : listLayouts.OrderByDescending(l => l.Name).ToList();
                break;
            case "Table":
                listLayouts = GetSortDirection(e.SortExpression) == "Asc" ? listLayouts.OrderBy(l => l.Table).ToList() : listLayouts.OrderByDescending(l => l.Table).ToList();
                break;
            case "ImageEnvironment":
                listLayouts = GetSortDirection(e.SortExpression) == "Asc" ? listLayouts.OrderBy(l => l.ImageEnvironment).ToList() : listLayouts.OrderByDescending(l => l.ImageEnvironment).ToList();
                break;

        }


        gvLayout.DataSource = listLayouts;
        gvLayout.DataBind();
    }
    private void ToggleCheckState(bool checkState)
    {
        foreach (GridViewRow row in gvLayout.Rows)
        {
            var cb = (CheckBox)row.FindControl("chkSelector");
            if (cb != null)
                cb.Checked = checkState;
        }
    }
}