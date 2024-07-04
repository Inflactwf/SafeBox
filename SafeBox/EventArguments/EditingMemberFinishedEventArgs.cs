using SafeBox.Models;

namespace SafeBox.EventArguments
{
    public class EditingMemberFinishedEventArgs(bool hasChanges, StorageMember sourceMember, StorageMember editedMember)
    {
        public bool HasChanges { get; set; } = hasChanges;
        public StorageMember SourceMember { get; set; } = sourceMember;
        public StorageMember EditedMember { get; set; } = editedMember;
    }
}
