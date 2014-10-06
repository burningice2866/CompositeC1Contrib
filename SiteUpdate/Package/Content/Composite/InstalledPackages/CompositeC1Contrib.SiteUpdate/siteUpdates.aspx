<?xml version="1.0" encoding="UTF-8" ?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.SiteUpdate.Web.UI.SiteUpdates" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Mail log</title>

        <control:styleloader runat="server" />
        <control:scriptloader type="sub" runat="server" />
        
        <link href="logs.css" rel="stylesheet" />
    </head>

    <body
        id="root"
        data-consoleid="<%= ConsoleId %>"
        data-entitytoken="<%= EntityToken %>"
        data-baseurl="<%= HttpUtility.HtmlAttributeEncode(BaseUrl) %>">
        <form runat="server" class="updateform updatezone">
            <ui:broadcasterset>
                <ui:broadcaster id="broadcasterHasSelection" isdisabled="true" />
            </ui:broadcasterset>

            <ui:popupset></ui:popupset>

            <ui:page id="mailLog" image="${icon:report}">
                <ui:flexbox id="flexbox">
                    <ui:scrollbox id="scrollbox">
                        <asp:Repeater ID="rptUpdate" ItemType="CompositeC1Contrib.SiteUpdate.SiteUpdateInformation" runat="server">
                            <HeaderTemplate>
                                <table width="100%" id="logtable">
                                    <thead>
                                        <tr>
                                            <th>
                                                <ui:text label="Update id" />
                                            </th>

                                            <th width="100px">
                                                <ui:text label="Release date" />
                                            </th>

                                            <th width="350px">
                                                <ui:text label="Installed" />
                                            </th>

                                            <th width="75px"></th>
                                        </tr>
                                    </thead>

                                    <tbody>
                            </HeaderTemplate>

                            <ItemTemplate>
                                        <tr>
                                            <td>
                                                <ui:text label="<%# Server.HtmlEncode(Item.Name) %>" />
                                            </td>

                                            <td>
                                                <ui:text label="<%# Item.ReleasedDate.ToString("G") %>" />
                                            </td>

                                            <td>
                                                <ui:text label="<%# InstalledInformation(Item) %>" />
                                            </td>
                                
                                            <td class="command">
                                                <ui:toolbar id="commands">
                                                    <ui:toolbarbody>
                                                        <ui:toolbargroup>
                                                            <aspui:toolbarbutton 
                                                                runat="server"
                                                                autopostback="true" 
                                                                text="Install"
                                                                client_tooltip="Install"
                                                                client_image="${icon:previous}"
                                                                client_image-disabled="${icon:previous-disabled}"
                                                                commandArgument="<%# Item.Id.ToString() %>"
                                                                onclick="btnInstall_Click" />
                                                        </ui:toolbargroup>
                                                    </ui:toolbarbody>
                                                </ui:toolbar>
                                            </td>
                                        </tr>
                            </ItemTemplate>

                            <FooterTemplate>
                                    </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </ui:scrollbox>
                    
                    <ui:cover id="cover" hidden="true" />
                </ui:flexbox>
            </ui:page>
        </form>
    </body>
</html>
