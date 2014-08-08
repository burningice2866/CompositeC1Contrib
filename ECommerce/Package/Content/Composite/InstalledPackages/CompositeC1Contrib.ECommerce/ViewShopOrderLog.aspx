<?xml version="1.0" encoding="UTF-8" ?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.ECommerce.Web.UI.ViewShopOrderLog" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
<control:httpheaders runat="server" />
<head runat="server">
    <title>Order list</title>

    <control:styleloader runat="server" />
    <control:scriptloader type="sub" runat="server" />

    <script type="text/javascript">
        DocumentManager.isDocumentSelectable = true;
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <ui:page id="mailLog" image="${icon:log-showlog}">
            <ui:toolbar id="toolbar">
                <ui:toolbarbody>
                    <ui:toolbargroup>
                        <aspui:toolbarbutton autopostback="true" text="Back" imageurl="${icon:back}" runat="server" onclick="OnBack" />
                    </ui:toolbargroup>
                </ui:toolbarbody>
            </ui:toolbar>

            <ui:scrollbox id="scrollbox">
                <asp:Literal runat="server" ID="lit" />
            </ui:scrollbox>
        </ui:page>
    </form>
</body>
</html>
