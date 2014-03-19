<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.Teasers.Web.UI.SortPageTeasersPage" %>

<%@ Import Namespace="Composite.Data" %>
<%@ Import Namespace="CompositeC1Contrib.Teasers.Data.Types" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Sort formfields</title>
        <link type="text/css" href="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.1/themes/base/jquery-ui.css" rel="stylesheet" />	
        
        <control:scriptloader type="sub" runat="server" />

        <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js"></script>
        <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js"></script>

        <script type="text/javascript">
            $(document).ready(function () {
                $('ul').sortable(
                {
                    placeholder: 'ui-state-highlight',
                    handle: '.handle',
                    update: function () {
                        var order = $('ul').sortable('serialize');

                        $.ajax(
                        {
                            type: 'POST',
                            url: 'SortPageTeasers.aspx/UpdateOrder',
                            contentType: 'application/json; charset=utf-8',
                            dataType: 'json',
                            data: "{ 'pageId':'<%= HttpUtility.UrlEncode(Request.QueryString["pageId"]) %>', 'position': '<%= Request.QueryString["position"] %>', 'consoleId': '<%= Request.QueryString["consoleId"] %>', 'entityToken': \"<%= HttpUtility.UrlEncode(Request.QueryString["EntityToken"]) %>\", 'serializedOrder': '" + order + "' }",
                            success: function () {
                                MessageQueue.update();
                            }
                        });
			        }
                });

                $('#sortable').disableSelection();
            });
        </script>

        <style>
            body 
            {
                padding: 20px 0 20px 20px;
                color: #2C2C28;
                font-family: Tahoma,Arial,sans-serif;
                font-size: 13px;
                line-height: 20px; 
            }
            
            ul li img.handle 
            {
	            margin-right: 20px;
	            cursor: move;
            }
            
            ul { list-style-type: none; margin: 0; padding: 0; width: 60%; }
	        ul li { margin: 0 5px 5px 5px; padding: 5px; font-size: 1.2em; height: 1.5em; }
	        html>body ul li { height: 1.5em; line-height: 1.2em; }
	        .ui-state-highlight { height: 1.5em; line-height: 1.2em; }
        </style>
    </head>
    <body>
        <ui:page id="sort-data">
            <h1>Reorder</h1>
            Use mouse drag &amp; drop to change the order 
            <br />
            <br />

            <ul>
                <asp:Repeater ID="rptFields" ItemType="CompositeC1Contrib.Teasers.Data.Types.IPageTeaser" runat="server">
                    <ItemTemplate>
                        <li id="instance_<%# hashId(Item) %>" class="ui-state-default">
                            <img src="/Composite/InstalledPackages/CompositeC1Contrib.Sorting/arrow.png" alt="Drag" class="handle" />
                            <%# Server.HtmlEncode(Item.GetLabel()) %>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>        
            </ul>
        </ui:page>
    </body>
</html>
