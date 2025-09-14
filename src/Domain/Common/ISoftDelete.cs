namespace N1coLoyalty.Domain.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; set; } 
}