using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace HexaERP.Services.ActiveDirectory
{
    public class ADUser
    {
        public const string SamAccountNameProperty = "sAMAccountName";

        /// <summary>
        /// Property name of canonical name.
        /// </summary>
        public const string CanonicalNameProperty = "CN";


        #region Properties

        /// <summary>
        /// Gets or sets the canonical name of the user.
        /// </summary>
        public string CN { get; set; }

        /// <summary>
        /// Gets or sets the sAM account name
        /// </summary>
        public string SamAcountName { get; set; }

        #endregion

        /// <summary>
        /// Gets all users of a given domain.
        /// </summary>
        /// <param name="domain">Domain to query. Should be given in the form ldap://domain.com/ </param>
        /// <returns>A list of users.</returns>
        public static List<ADUser> GetUsers(string domain)
        {
            List<ADUser> users = new List<ADUser>();
            try
            {
                using (DirectoryEntry searchRoot = new DirectoryEntry(domain, "demo", "blore@123"))
                using (DirectorySearcher directorySearcher = new DirectorySearcher(searchRoot))
                {
                    // Set the filter
                    directorySearcher.Filter = "(&(objectCategory=person)(objectClass=user))";

                    // Set the properties to load.
                    directorySearcher.PropertiesToLoad.Add(CanonicalNameProperty);
                    directorySearcher.PropertiesToLoad.Add(SamAccountNameProperty);

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())
                    {
                        foreach (SearchResult searchResult in searchResultCollection)
                        {
                            // Create new ADUser instance
                            var user = new ADUser();

                            // Set CN if available.
                            if (searchResult.Properties[CanonicalNameProperty].Count > 0)
                                user.CN = searchResult.Properties[CanonicalNameProperty][0].ToString();

                            // Set sAMAccountName if available
                            if (searchResult.Properties[SamAccountNameProperty].Count > 0)
                                user.SamAcountName = searchResult.Properties[SamAccountNameProperty][0].ToString();

                            // Add user to users list.
                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            // Return all found users.
            return users;
        }
    }
}




//namespace Tests
//{
//    /// <summary>
//    /// Tests the ActiveDirectory classes.
//    /// </summary>
//    [TestClass]
//    public class ActiveDirectoryTests
//    {
//        /// <summary>
//        /// Tests the <see cref="ADUser"/> class
//        /// </summary>
//        [TestMethod]
//        public void TestADUser()
//        {
//            var users = ADUser.GetUsers("LDAP://neitzel.local/DC=neitzel,DC=local");
//            Assert.IsNotNull(users);
//        }
//    }
//}
//}
