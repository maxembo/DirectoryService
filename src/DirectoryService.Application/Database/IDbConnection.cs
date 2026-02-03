using System.Data.Common;

namespace DirectoryService.Application.Database;

public interface IDbConnectionFactory
{
    DbConnection GetDbConnection();
}