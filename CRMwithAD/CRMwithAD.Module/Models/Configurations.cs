namespace CRMwithAD.Module.Models
{
    public class Configurations
    {
        public string Duration { get; set; }
    }
    public enum UserType
    {
        Admin = 1,
        Technical = 2,
        Sales = 3,
        CustomerAgent = 4

    }
    public enum Gender
    {
        Male,
        Female,
        NotConfirm
    }
    public enum PreferredContact
    {
        Email,
        SMS,
        Call,
        Whatsapp,
        Helpdesk
    }

    public enum OwnerShip
    {
        PublicSector,
        Govt,
        Private,
        Committee
    }
    public enum ProblemType
    {
        Technical,
        Question,
        Information
    }
    public enum ResponseSent
    {
        Yes, No
    }
    public enum CallDirection
    {
        Incoming, Outgoing
    }
    public enum SLAType
    {
        Basic, Standard, Premium
    }
    public enum Duration
    {
        SixMonths, OneYear, TwoYears, ThreeYears,
        FiveYears
    }
    public enum TicketStatus
    {
        Open, Resolved, Hold, Failed
    }
    public enum Priority
    {
        Severe, Critical, High, Moderate
    }
    public enum CampaignStatus
    {
        Created,
        Completed
    }
}
