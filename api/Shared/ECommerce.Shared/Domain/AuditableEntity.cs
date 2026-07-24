namespace ECommerce.Shared.Domain;

public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    public void SetCreatedBy(string userId) => CreatedBy = userId;
    public void SetUpdatedBy(string userId)
    {
        UpdatedBy = userId;
        SetUpdatedAt();
    }
}
