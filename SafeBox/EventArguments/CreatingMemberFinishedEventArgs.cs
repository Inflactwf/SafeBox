using SafeBox.Interfaces;

namespace SafeBox.EventArguments
{
    public class CreatingMemberFinishedEventArgs(IStorageMember member)
    {
        public IStorageMember StorageMember { get; } = member;
    }
}
