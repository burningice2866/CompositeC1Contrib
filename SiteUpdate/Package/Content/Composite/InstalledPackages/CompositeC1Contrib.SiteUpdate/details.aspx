<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.SiteUpdate.Web.UI.Details" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Mail view</title>
        
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
            <ui:page id="mailView">
                <ui:scrollbox id="scrollbox">
                    Name: <%: Update.Name %> <br /><br />
                    Changelog <%= Update.ChangeLog.Replace("\n", "<br />") %>
                </ui:scrollbox>
            </ui:page>
        </form>
    </body>
</html>
