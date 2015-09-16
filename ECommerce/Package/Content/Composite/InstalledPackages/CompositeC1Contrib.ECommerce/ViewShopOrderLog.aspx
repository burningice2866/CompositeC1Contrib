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
        <form runat="server">
            <ui:page id="shopOrderLog" image="${icon:log-showlog}">
                <ui:toolbar id="toolbar">
                    <ui:toolbarbody>
                        <ui:toolbargroup>
                            <aspui:toolbarbutton autopostback="true" text="Back" imageurl="${icon:back}" runat="server" onclick="OnBack" />
                        </ui:toolbargroup>
                    </ui:toolbarbody>
                </ui:toolbar>

                <ui:scrollbox id="scrollbox">
                    <asp:Repeater ID="rpt" ItemType="CompositeC1Contrib.ECommerce.Data.Types.IShopOrderLog" runat="server">
                        <HeaderTemplate>
                            <table>
                                <thead>
                                    <th>
                                        <td>Timestamp (UTC)</td>
                                        <td>Title</td>
                                        <td>Data</td>
                                    </th>
                                </thead>

                                <tbody>
                        </HeaderTemplate>

                        <ItemTemplate>
                                    <tr>
                                        <td><%# Item.Timestamp %></td>
                                        <td><%# HttpUtility.HtmlEncode(Item.Title) %></td>

                                        <td>
                                            <pre><%# HttpUtility.HtmlEncode(Item.Data ?? String.Empty) %></pre>
                                        </td>
                                    </tr>
                        </ItemTemplate>

                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </ui:scrollbox>
            </ui:page>
        </form>
    </body>
</html>
