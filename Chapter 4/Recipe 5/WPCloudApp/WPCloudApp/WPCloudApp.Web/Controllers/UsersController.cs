namespace WPCloudApp.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using WPCloudApp.Web.Infrastructure;
    using WPCloudApp.Web.Models;
    using WPCloudApp.Web.UserAccountWrappers;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public class UsersController : Controller
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        private readonly IUserRepository userRepository;

        public UsersController()
            : this(new UserTablesServiceContext(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public UsersController(IUserPrivilegesRepository userPrivilegesRepository, IUserRepository userRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
            this.userRepository = userRepository;
        }

        public ActionResult Index()
        {
            var users = this.userRepository.GetAllUsers()
                .Select(user => new UserPermissionsModel
                {
                    UserName = user.Name,
                    UserId = user.UserId,
                    TablesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.TablesUsagePrivilege),
                    BlobsUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.BlobContainersUsagePrivilege),
                    QueuesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.QueuesUsagePrivilege)
                });

            return this.View(users);
        }

        [HttpPost]
        public void SetUserPermissions(string userId, bool useTables, bool useBlobs, bool useQueues, bool useSql)
        {
            this.SetStorageItemUsagePrivilege(useTables, userId, PrivilegeConstants.TablesUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useBlobs, userId, PrivilegeConstants.BlobContainersUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useQueues, userId, PrivilegeConstants.QueuesUsagePrivilege);
        }

        private void SetStorageItemUsagePrivilege(bool allowAccess, string user, string privilege)
        {
            if (allowAccess)
            {
                this.userPrivilegesRepository.AddPrivilegeToUser(user, privilege);
            }
            else
            {
                this.userPrivilegesRepository.RemovePrivilegeFromUser(user, privilege);
            }
        }
    }
}
