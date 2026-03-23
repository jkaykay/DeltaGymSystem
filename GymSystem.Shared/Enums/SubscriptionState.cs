namespace GymSystem.Shared.Enums;

public enum SubscriptionState
{
    Pending,   // Created, awaiting payment
    Active,    // Paid, currently in effect
    Queued,    // Paid (prepay), waiting for start date
    Expired    // Was active, end date has passed
}