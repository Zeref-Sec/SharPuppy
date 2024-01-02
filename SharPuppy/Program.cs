using System;
using System.DirectoryServices;

class Program
{
    static void Main()
    {
        // Dynamically detect the current user's domain
        string domainName = Environment.UserDomainName;

        // Construct the LDAP path based on the detected domain
        string ldapPath = $"LDAP://{domainName}";

        // Create a DirectoryEntry object and bind it to the LDAP path
        using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
        {
            // Create a DirectorySearcher object to search for user objects with administrator privileges and SPNs
            using (DirectorySearcher searcher = new DirectorySearcher(entry))
            {
                // Set the filter to find user objects with administrator privileges and SPNs
                searcher.Filter = "(&(objectClass=user)(objectCategory=user)(|(memberOf=CN=Domain Admins,CN=Users,DC=yourdomain,DC=com)(memberOf=CN=Enterprise Admins,CN=Users,DC=yourdomain,DC=com))(servicePrincipalName=*))";

                // Perform the search and iterate through the result collection
                SearchResultCollection results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    // Get the DirectoryEntry for each user
                    DirectoryEntry userEntry = result.GetDirectoryEntry();

                    // Display username and other relevant information
                    string username = userEntry.Properties["sAMAccountName"].Value.ToString();
                    string isAdmin = IsAdministrator(userEntry) ? "Yes" : "No";

                    Console.WriteLine($"Username: {username}, Administrator: {isAdmin}");

                    // Display SPN values
                    foreach (string spn in userEntry.Properties["servicePrincipalName"])
                    {
                        Console.WriteLine($"  SPN: {spn}");
                    }
                }
            }
        }
    }

    // Check if the user is a member of the "Domain Admins" or "Enterprise Admins" group
    static bool IsAdministrator(DirectoryEntry userEntry)
    {
        var isAdmin = false;

        foreach (var group in userEntry.Properties["memberOf"])
        {
            // Check for membership in administrator groups
            if (group.ToString().Contains("Domain Admins") || group.ToString().Contains("Enterprise Admins"))
            {
                isAdmin = true;
                break;
            }
        }

        return isAdmin;
    }
}
