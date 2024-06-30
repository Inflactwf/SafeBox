using SafeBox.Models;

namespace SafeBox.EventArguments
{
    public class CreatingMemberFinishedEventArgs(StorageMember member)
    {
        public StorageMember StorageMember { get; } = member;
    }
}
