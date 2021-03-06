﻿<%@ Page Title="" Language="C#" MasterPageFile="~/views/users/groupacls/groupacls.master" AutoEventWireup="true" CodeFile="groupmanagement.aspx.cs" Inherits="views_users_groupacls_groupmanagement" %>


<asp:Content ID="Content1" ContentPlaceHolderID="BreadcrumbSub2" Runat="Server">
    <li>Group Based Computer Management</li>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="SubHelp">
      <li role="separator" class="divider"></li>
    <li><a href="<%= ResolveUrl("~/views/help/users-groupmgmt.aspx") %>"   target="_blank">Help</a></li>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="ActionsRightSub">
     <asp:LinkButton ID="buttonUpdate" runat="server" OnClick="buttonUpdate_OnClick" Text="Update Group Management" CssClass="btn btn-default"  />
     <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
    <span class="caret"></span>
  </button>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="SubContent2" Runat="Server">
     <script type="text/javascript">
        $(document).ready(function() {
            $('#group').addClass("nav-current");
            $("[id*=gvGroups] td").hover(function () {
                $("td", $(this).closest("tr")).addClass("hover_row");
            }, function () {
                $("td", $(this).closest("tr")).removeClass("hover_row");
            });
        });
    </script>
     
    <asp:GridView ID="gvGroups" runat="server" AutoGenerateColumns="False" CssClass="Gridview" DataKeyNames="Id" AlternatingRowStyle-CssClass="alt">
        <Columns>
            <asp:TemplateField>
                <HeaderStyle CssClass="chkboxwidth"></HeaderStyle>
                <ItemStyle CssClass="chkboxwidth"></ItemStyle>
                <HeaderTemplate>
                    <asp:CheckBox ID="chkSelectAll" runat="server" AutoPostBack="True" OnCheckedChanged="SelectAll_CheckedChanged"/>
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="chkSelector" runat="server"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Id" HeaderText="groupID" InsertVisible="False" SortExpression="Id" Visible="False"/>
            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="width_200"/>
            
            <asp:BoundField DataField="Type" HeaderText="Type" SortExpression="Type" HeaderStyle-CssClass="mobi-hide-smallest" ItemStyle-CssClass="mobi-hide-smallest"/>
         
        </Columns>
        <EmptyDataTemplate>
            No Groups Have Been Created
        </EmptyDataTemplate>
    </asp:GridView>
    
 
</asp:Content>



