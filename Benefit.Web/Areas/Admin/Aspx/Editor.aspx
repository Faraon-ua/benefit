<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Editor.aspx.cs" Inherits="Benefit.Web.Areas.Aspx.Editor" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">
        </telerik:RadAjaxManager>
        <telerik:RadScriptManager ID="RadScriptManager1" runat="server">
            <Scripts>
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.Core.js"></asp:ScriptReference>
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQuery.js"></asp:ScriptReference>
                <asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQueryInclude.js"></asp:ScriptReference>
            </Scripts>
        </telerik:RadScriptManager>
        <div>
            <telerik:RadEditor ID="RadEditor1" runat="server" UseRadContextMenu="False">
                <Content>
                </Content>
                <ImageManager UploadPaths="~/Content/UserFiles" ViewPaths="~/Content/UserFiles" />

                <TrackChangesSettings CanAcceptTrackChanges="False"></TrackChangesSettings>
            </telerik:RadEditor>

            <script type="text/javascript">
                function GetHtml() {
                    var editor = $find("<%=RadEditor1.ClientID%>");
                    return editor.get_html();
                }
                function SetHtml(html) {
                    var editor = $find("<%=RadEditor1.ClientID%>");
                    return editor.set_html(html);
                }
            </script>
        </div>
    </form>
</body>
</html>
