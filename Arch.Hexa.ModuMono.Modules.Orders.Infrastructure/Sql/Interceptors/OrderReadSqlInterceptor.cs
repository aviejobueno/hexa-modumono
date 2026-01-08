using Arch.Hexa.ModuMono.BuildingBlocks.Infrastructure.Sql.Interceptors;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Sql.Interceptors;

public sealed class OrderReadSqlInterceptor() : SerilogSqlInterceptorBase("READ");