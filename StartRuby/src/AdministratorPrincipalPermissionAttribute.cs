
using System;


namespace StartRuby
{


    // https://stackoverflow.com/questions/820289/how-to-set-a-multilanguage-principalpermission-role-name
    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false), System.Runtime.InteropServices.ComVisible(true)]
    public sealed class AdministratorPrincipalPermissionAttribute : System.Security.Permissions.CodeAccessSecurityAttribute
    {

        public AdministratorPrincipalPermissionAttribute(System.Security.Permissions.SecurityAction action) : base(action)
        { }


        public override System.Security.IPermission CreatePermission()
        {
            System.Security.Principal.SecurityIdentifier identifier =
                new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinAdministratorsSid, null);

            // https://stackoverflow.com/questions/3212862/how-can-i-get-the-local-group-name-for-guests-administrators
            string adminGroupName = identifier.Translate(typeof(System.Security.Principal.NTAccount)).Value;
            return new System.Security.Permissions.PrincipalPermission(null, adminGroupName);
        } // End Sub CreatePermission 


    } // End Class AdministratorPrincipalPermissionAttribute 


} // End Namespace StartRuby
