namespace JwtTokenAuthRefImplementation.Web.Models
{
    public class UserInfo
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Role Role { get; set; }

        public bool HasAdminAccess => Role == Role.PLANNING_HEAD;
    }

    public enum Role
    {
        PLANNING_HEAD = 1,
        PLANNING_MANAGER = 2,
        PLANNER = 3
    }
}