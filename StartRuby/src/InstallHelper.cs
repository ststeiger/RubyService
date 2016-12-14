
namespace StartRuby
{


    internal class InstallHelper
    {


        public static bool IsUserLocalAdmin()
        {
            bool isAdmin;

            try
            {
                // Get the currently logged in user
                using (System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                    isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                } // End Using identity 

            }
            catch (System.UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (System.Exception ex)
            {
                isAdmin = false;
            }

            return isAdmin;
        } // End Function IsUserLocalAdmin 


        public static bool IsVistaUacAdmin()
        {
            bool isAllowed = false;

            System.Security.Principal.SecurityIdentifier identifier =
             new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinAdministratorsSid, null);

            // https://stackoverflow.com/questions/3212862/how-can-i-get-the-local-group-name-for-guests-administrators
            string adminGroupName = identifier.Translate(typeof(System.Security.Principal.NTAccount)).Value;



            using (System.DirectoryServices.AccountManagement.PrincipalContext pc =
                new System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Machine, null))
            {
                System.DirectoryServices.AccountManagement.UserPrincipal up = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
                System.DirectoryServices.AccountManagement.GroupPrincipal gp =
                    System.DirectoryServices.AccountManagement.GroupPrincipal.FindByIdentity(pc, adminGroupName);

                if (up.IsMemberOf(gp))
                    isAllowed = true;
            }

            return isAllowed;
        } // End Function IsVistaUacAdmin 


    } // End Class 


} // End Namespace 
