using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Sql.Interceptors;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Sql.Interceptors;

public sealed class CustomerReadSqlInterceptor() : SerilogSqlInterceptorBase("READ");