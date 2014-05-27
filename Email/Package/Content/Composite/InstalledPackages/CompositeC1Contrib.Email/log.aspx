<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.Email.Web.UI.MailLogPage" %>
<%@ Import Namespace="CompositeC1Contrib.Email.Web.UI" %>
<%@ Register TagPrefix="aspui" Namespace="Composite.Core.WebClient.UiControlLib" Assembly="Composite" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Mail log</title>
        
        <control:styleloader runat="server" />
        <control:scriptloader type="sub" runat="server" />
        
        <script type="text/javascript">
            DocumentManager.isDocumentSelectable = true;
		</script>
        
        <style>
            #scrollbox > table {
                margin: 1em;
                border: none;
            }

             a {
                 text-decoration: underline;
                 cursor: pointer;
             }

            a:hover {
                text-decoration: none;
            }
        </style>
    </head>

    <body>
        <form runat="server">
            <ui:page id="mailLog" image="${icon:log-showlog}">
                <ui:toolbar id="toolbar">
			        <ui:toolbarbody>
				        <ui:toolbargroup>
					        <aspui:ToolbarButton AutoPostBack="true" Text="Refresh" ImageUrl="${icon:refresh}" runat="server" OnClick="OnRefresh" />
                            <aspui:ToolbarButton AutoPostBack="true" Text="Empty queue" ImageUrl="${icon:delete}" runat="server" OnClick="OnDeleteAll" Visible='<%# View == LogViewMode.Queued %>' />
                            <aspui:Selector runat="server" ID="ddlTemplates" AutoPostBack="True" OnSelectedIndexChanged="OnTemplateChanged" DataValueField="Key" DataTextField="Value"/>
				        </ui:toolbargroup>
			        </ui:toolbarbody>
		        </ui:toolbar>
                           
                <ui:scrollbox id="scrollbox">
                    <asp:Repeater ID="rptLog" ItemType="CompositeC1Contrib.Email.Web.UI.MailLogItem" runat="server">
                        <HeaderTemplate>
                            <table>
                                <tr>
                                    <th>Subject</th>
                                    <th>Timestamp</th>
                                    <th>Template</th>
                                    
                                    <th></th>
                                    <th></th>
                                </tr>
                            </HeaderTemplate>

                        <ItemTemplate>
                                <tr>
                                    <td><%# Item.Subject %></td>
                                    <td><%# Item.FormatTimeStamp() %></td>
                                    <td><%# Item.Template == null ? "No template" : Item.Template.Key %></td>

                                    <td>
                                        <a href="<%= BaseUrl.Replace("&", "&amp;") %>&amp;cmd=delete&amp;id=<%# Item.Id %>">Delete</a>
                                    </td>
                                    
                                    <td>
                                        <a href="view.aspx<%= BaseUrl.Replace("&", "&amp;") %>&amp;id=<%# Item.Id %>">View</a>
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
