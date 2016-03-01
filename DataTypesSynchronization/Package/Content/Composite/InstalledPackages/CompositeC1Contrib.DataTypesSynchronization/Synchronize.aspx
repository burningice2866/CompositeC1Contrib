<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.DataTypesSynchronization.Web.UI.SynchronizePage" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <control:scriptloader type="sub" runat="server" />

        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8/jquery.min.js"></script>
        <script src="Synchronize.js"></script>
    </head>

    <body data-jobid="<%= Request.QueryString["jobid"] %>">
        <ui:page id="navision-data">
            <form runat="server">
                <asp:Button runat="server" Text="Batch update data from Proxy" OnClick="UpdateData_Click" /> <br /> <br />
            </form>
            
            Job Id: <%= Request.QueryString["jobid"] %> <br />
            Running: <span class="status">no</span> <br /> <br />
            <div id="log" class="log" runat="server"></div>
        </ui:page>
    </body>
</html>
