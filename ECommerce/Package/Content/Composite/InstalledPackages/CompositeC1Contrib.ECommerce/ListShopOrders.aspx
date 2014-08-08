<?xml version="1.0" encoding="UTF-8" ?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.ECommerce.Web.UI.ListShopOrders" %>

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
                        <aspui:toolbarbutton autopostback="true" text="Refresh" imageurl="${icon:refresh}" runat="server" onclick="OnRefresh" />
                    </ui:toolbargroup>
                </ui:toolbarbody>
            </ui:toolbar>

            <ui:scrollbox id="scrollbox">
                <asp:Repeater runat="server" ID="rpt" ItemType="CompositeC1Contrib.ECommerce.Data.Types.IShopOrder">
                    <HeaderTemplate>
                        <table>
                            <tr>
                                <td>Order ID</td>
                                <td>Created On</td>
                                <td>Total</td>
                                <td></td>
                            </tr>
                    </HeaderTemplate>

                    <ItemTemplate>
                        <tr>
                            <td><%#: Item.Id %></td>
                            <td><%#: Item.CreatedOn %></td>
                            <td><%#: Item.OrderTotal %></td>
                            <td>
                                <a href="viewShopOrder.aspx?id=<%#: Item.Id %>">View</a>
                            </td>
                        </tr>
                    </ItemTemplate>

                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </ui:scrollbox>
        </ui:page>
    </form>
</body>
</html>
