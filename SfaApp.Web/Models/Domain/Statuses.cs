namespace SfaApp.Web.Models.Domain;

public enum OrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Created = 4,
    Synced = 5,
    Accepted = 6,
    Dispatched = 7,
    Delivered = 8,
    Cancelled = 9
}

public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum DaySessionStatus
{
    Started = 0,
    Ended = 1
}

public enum VisitStatus
{
    Completed = 0,
    Skipped = 1
}

public enum SyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2
}

public enum UploadJobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}