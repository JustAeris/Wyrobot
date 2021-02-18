namespace Wyrobot.Core.Models
{
    public class AutoRole
    {
        public AutoRole(ulong roleId = default)
        {
            RoleId = roleId;
        }

        public ulong RoleId { get; }
    }
}