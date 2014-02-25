<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.Email.Web.UI.MailViewPage" %>
<%@ Import Namespace="CompositeC1Contrib.Email.Web.UI" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Mail view</title>
        <link type="text/css" href="css/ui-lightness/jquery-ui-1.8.16.custom.css" rel="Stylesheet" />	
        
        <control:styleloader runat="server" />
        <control:scriptloader type="sub" runat="server" />

        <script type="text/javascript" src="js/jquery-1.6.2.min.js"></script>
        <script type="text/javascript" src="js/jquery-ui-1.8.16.custom.min.js"></script>
        
        <script type="text/javascript">
            DocumentManager.isDocumentSelectable = true;
		</script>
        
        <style>
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
            <ui:page id="mailView">
                <ui:toolbar id="toolbar">
			        <ui:toolbarbody>
				        <ui:toolbargroup>
					        <aspui:ToolbarButton AutoPostBack="true" Text="Back" ImageUrl="${icon:back}" runat="server" OnClick="OnBack" />
                            <aspui:ToolbarButton AutoPostBack="true" Text="Delete" ImageUrl="${icon:delete}" runat="server" OnClick="OnDelete" />
				        </ui:toolbargroup>
			        </ui:toolbarbody>
		        </ui:toolbar>

                <ui:scrollbox id="scrollbox">
                    Sent: <% = TimeStamp.ToString("dddd, dd MMMM yyyy, HH:mm:ss") %> <br /><br />
                    From: <%= Message.From.Address %> <br />
                    To: <%= String.Join(", ", Message.To.Select(o => o.Address)) %> <br />
                    Cc: <%= String.Join(", ", Message.CC.Select(o => o.Address)) %> <br />
                    Bcc: <%= String.Join(", ", Message.Bcc.Select(o => o.Address)) %> <br /><br />
                    <b><%= Message.Subject %></b> <br /> <br />
                    <br />
                    <%= Message.Body %>
                </ui:scrollbox>
            </ui:page>
        </form>
    </body>
</html>
