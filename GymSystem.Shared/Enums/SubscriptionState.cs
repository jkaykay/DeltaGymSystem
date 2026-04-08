namespace GymSystem.Shared.Enums;

// Represents the lifecycle states of a member subscription.
public enum SubscriptionState
{
    Pending,   // Created, awaiting payment
    Active,    // Paid, currently in effect
    Queued,    // Paid (prepay), waiting for start date
    Expired    // Was active, end date has passed
}
