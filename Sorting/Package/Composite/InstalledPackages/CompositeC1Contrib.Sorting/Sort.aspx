<?xml version="1.0" encoding="UTF-8"?>

<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.Sorting.Web.UI.SortData" %>
<%@ Import Namespace="Composite.Data" %>

<html xmlns="http://www.w3.org/1999/xhtml" xmlns:ui="http://www.w3.org/1999/xhtml" xmlns:control="http://www.composite.net/ns/uicontrol">
    <control:httpheaders runat="server" />

    <head runat="server">
        <title>Sort</title>
        <link type="text/css" href="css/ui-lightness/jquery-ui-1.8.16.custom.css" rel="Stylesheet" />	
        
        <control:scriptloader type="sub" runat="server" />

        <script type="text/javascript" src="js/jquery-1.6.2.min.js"></script>
        <script type="text/javascript" src="js/jquery-ui-1.8.16.custom.min.js"></script>

        <script type="text/javascript">
            $(document).ready(function ()
            {            
                $('ul').sortable(
                {
			        placeholder: 'ui-state-highlight',
			        handle: '.handle',
			        update: function ()
			        {
			            var order = $('ul').sortable('serialize');

			            $.ajax(
                        {
                            type: 'POST',
                            url: 'Sort.aspx/UpdateOrder',
                            contentType: 'application/json; charset=utf-8',
                            dataType: 'json',
                            data: "{ 'type':'<%= HttpUtility.UrlEncode(Request.QueryString["type"]) %>', 'consoleId': '<%= Request.QueryString["consoleId"] %>', 'entityToken': \"<%= HttpUtility.UrlEncode(Request.QueryString["EntityToken"]) %>\", 'serializedOrder': '" + order + "' }",
                            success: function() 
                            {
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
                <% foreach (var instance in getInstances()) { %>
                    <li id="instance_<%= hashId(instance) %>" class="ui-state-default">
                        <img src="arrow.png" alt="Drag" class="handle" />
                        <%= instance.GetLabel() %>
                    </li>
                <% } %>        
            </ul>
        </ui:page>
    </body>
</html>
