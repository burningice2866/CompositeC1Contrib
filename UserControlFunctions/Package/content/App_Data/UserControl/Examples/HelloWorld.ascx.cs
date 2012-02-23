using System;

using CompositeC1Contrib.FunctionProvider;

[FunctionDescription("This ASP.NET User Control simply prints 'Hello World' (where 'World' can be changed via a C1 Parameter).")]
public partial class App_Data_UserControl_Examples_HelloWorld : System.Web.UI.UserControl
{
    [FunctionParameter("Whom to greet?", "Write a string specifying who we should greet here. By default the world is greeted.", "World")]
    public string ToGreet { get; set; }

    protected void Page_Load(object sender, EventArgs e)
    {

    }
}