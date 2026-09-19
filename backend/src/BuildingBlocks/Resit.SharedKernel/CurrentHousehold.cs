namespace Resit.SharedKernel;

public sealed record CurrentHousehold(Guid HouseholdId, Guid UserId, string DisplayName);
