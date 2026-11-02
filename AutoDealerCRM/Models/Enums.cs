namespace AutoDealerCRM.Models;

public enum DeliveryStatus
{
    AtAuction    = 0,   // На аукціоні
    InTransit    = 1,   // В дорозі
    AtCustoms    = 2,   // На митниці
    Available    = 3    // В наявності
}

public enum AuditEventType
{
    LoginSuccess,
    LoginFailed,
    Logout,
    UserCreated,
    UserBlocked,
    UserUnblocked,
    LicenseUploaded,
    LicenseUploadFailed,
    LicenseInvalid
}
