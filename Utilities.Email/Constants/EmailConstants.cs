namespace CodeBridgeSoftware.Infrastructure.Email
{
    public class EmailConstants
    {
        public enum enumEmailAddressType
        {
            To = 0,
            CC,
            BCC
        }

        public enum enumMailPriority
        {
            // Summary:
            //     The email has normal priority.
            Normal = 0,

            //
            // Summary:
            //     The email has low priority.
            Low = 1,

            //
            // Summary:
            //     The email has high priority.
            High = 2,
        }
    }
}