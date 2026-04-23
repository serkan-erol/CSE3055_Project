using System.Data;
using Dapper;

namespace Kismet.Repository.Helpers;

/// <summary>
/// Dapper type handler for DateOnly to SQL Server date type conversion
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly date)
    {
        // Convert DateOnly to DateTime at midnight for SQL Server date type
        parameter.Value = date.ToDateTime(new TimeOnly(0, 0));
        parameter.DbType = DbType.Date;
    }

    public override DateOnly Parse(object value)
    {
        // Convert DateTime from database back to DateOnly
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }
        
        if (value is DateOnly dateOnly)
        {
            return dateOnly;
        }
        
        throw new ArgumentException($"Cannot convert {value?.GetType()} to DateOnly", nameof(value));
    }
}

/// <summary>
/// Dapper type handler for nullable DateOnly to SQL Server date type conversion
/// </summary>
public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly? date)
    {
        if (date.HasValue)
        {
            // Convert DateOnly to DateTime at midnight for SQL Server date type
            parameter.Value = date.Value.ToDateTime(new TimeOnly(0, 0));
            parameter.DbType = DbType.Date;
        }
        else
        {
            parameter.Value = DBNull.Value;
        }
    }

    public override DateOnly? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return null;
        }
        
        // Convert DateTime from database back to DateOnly
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }
        
        if (value is DateOnly dateOnly)
        {
            return dateOnly;
        }
        
        throw new ArgumentException($"Cannot convert {value?.GetType()} to DateOnly?", nameof(value));
    }
}
