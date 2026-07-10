using HexaERP.MVC.Models;
using HexaERP.Services;
using System;
using System.Linq;

namespace HexaERP.MVC.Repository
{
    public class AppicationUser : IDisposable
    {
        EncryptionDecryption Sec = new EncryptionDecryption();
        private readonly ERPdbEntities context = new ERPdbEntities();
        private readonly UserCredentials user;
        public AppicationUser()
        {
            user = new UserCredentials();
        }

        public UserCredentials ValidateUser(string username, string password, string location)
        {
            string _Password = Sec.Encrypt(password);
            var _user = context.AppUsers.FirstOrDefault(user =>
            user.AppUserName.Equals(username, StringComparison.OrdinalIgnoreCase)
            && user.Password == _Password);
            if (_user != null)
            {
                user.userId = Convert.ToString(_user.AppUserId) == null ? "0" : Convert.ToString(_user.AppUserId);
                user.Name = _user.AppUserName;
                var organization = context.OrgInfoes.Find(_user.OrgInfoId).OrgInfoName;
                user.organization = organization;
                user.WorkingLocation = _user.WorkingLocation;
                user.email = Convert.ToString(_user.EMail) == null ? Convert.ToString(_user.AppUserName) : Convert.ToString(_user.EMail);
                var _roles = context.AppRoles.Find(_user.AppRoleId);
                if (_roles != null)
                    user.role = new string[] { _roles.SortCode };
            }
            else
            {
                return null;
            }
            return user;
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}