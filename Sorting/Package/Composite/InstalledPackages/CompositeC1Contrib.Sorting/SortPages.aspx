<%@ Page Language="C#" AutoEventWireup="true" Inherits="CompositeC1Contrib.Sorting.Web.UI.SortPages" %>
<%@ Import Namespace="Composite.Data" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Sort</title>
        <link type="text/css" href="css/ui-lightness/jquery-ui-1.8.16.custom.css" rel="Stylesheet" />	
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
                            url: 'SortPages.aspx/UpdateOrder',
                            contentType: 'application/json; charset=utf-8',
                            dataType: 'json',
                            data: "{ 'pageId':'<%= Request.QueryString["pageId"] %>', 'consoleId': '<%= Request.QueryString["consoleId"] %>', 'serializedOrder': '" + order + "' }"
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
        <h1>Reorder</h1>
        Use mouse drag & drop to change the order 
        <br />
        <br />

        <ul>        
            <% foreach (var instance in getPages()) { %>
                <li id="instance_<%= hashId(instance) %>" class="ui-state-default">
                    <img src="arrow.png" alt="Drag" class="handle" />
                    <%= instance.GetLabel() %>
                </li>
            <% } %>        
        </ul>

        <asp:ScriptManager EnablePageMethods="true" 
            EnableCdn="false" EnableHistory="false" EnablePartialRendering="false" EnableScriptGlobalization="false" EnableScriptLocalization="false" EnableSecureHistoryState="false"
            runat="server" />
    </body>
</html>
