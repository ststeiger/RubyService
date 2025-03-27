
namespace StartRuby
{

    
    static class Program
    {


        [AdministratorPrincipalPermission(System.Security.Permissions.SecurityAction.Demand)]
        public static void Setup()
        {
            bool a = InstallHelper.IsUserLocalAdmin();
            bool b = InstallHelper.IsVistaUacAdmin();
            System.Console.WriteLine(a);
            System.Console.WriteLine(b);
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            ServicesToRun = new System.ServiceProcess.ServiceBase[]
            {
                new RubyService()
            };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        } // End Sub Main 


    } // End Class Program 


} // End Namespace StartRuby 
