using SafeBox.Interfaces;

namespace SafeBox.EventArguments
{
    public class EditingMemberFinishedEventArgs(bool hasChanges, IStorageMember sourceMember, IStorageMember editedMember)
    {
        public bool HasChanges { get; set; } = hasChanges;
        public IStorageMember SourceMember { get; set; } = sourceMember;
        public IStorageMember EditedMember { get; set; } = editedMember;
    }
}
