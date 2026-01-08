using System.ComponentModel.DataAnnotations;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;
using Arch.Hexa.ModuMono.Orders.RestApi.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Arch.Hexa.ModuMono.Orders.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderLinesController(
    IMediator mediator,
    ILogger logger,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    #region OrderLine

    /// <summary>
    /// Get OrderLine By ID
    /// </summary>
    /// <param name="id">Search ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>Response Data with OrderLineDto founded</returns>
    [HttpGet]
    [Route("getOrderLineByIdAsync/v1/{id:guid}", Name = "GetOrderLineByIdAsync")]
    [Authorize(Policy = OrdersPolicies.Read)]
    [ProducesResponseType(typeof(OrderLineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderLineByIdAsync([Required] Guid id, CancellationToken cancellationToken)
    {
        logger.Verbose("Start {Operation}", nameof(GetOrderLineByIdAsync));

        // With {id:guid} the empty Guid can still be passed, so validate it.
        if (id == Guid.Empty) //400 Bad request
        {
            throw new BadRequestException("The id parameter can not be empty Guid");
        }

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Execute query (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var orderLine = await mediator.Send(new GetOrderLineByIdQuery(headersDic, id), cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(GetOrderLineByIdAsync));
        logger.Verbose("End {Operation}", nameof(GetOrderLineByIdAsync));

        return Ok(orderLine);
    }


    /// <summary>
    /// Get OrderLines with filtering, sorting and pagination
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <param name="includeTotalCount">Include or not Total Count optional</param>
    /// <param name="productName">Filter by productName optional</param>
    /// <param name="quantity">Filter by quantity optional</param>
    /// <param name="unitPrice">Filter by unitPrice optional</param>
    /// <param name="orderId">Filter by orderId optional</param>
    /// <param name="sortBy">Sort by field optional</param>
    /// <param name="descending">Sort descending optional</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>List of OrderLines</returns>
    [HttpGet]
    [Route("getOrderLinesAsync/v1", Name = "GetOrderLinesAsync")]
    [Authorize(Policy = OrdersPolicies.Read)]
    [ProducesResponseType(typeof(PagedResponse<OrderLineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderLinesAsync(
        [FromQuery, Required] int pageNumber,
        [FromQuery, Required] int pageSize,
        CancellationToken cancellationToken,
        [FromQuery] bool includeTotalCount = false,
        [FromQuery] string? productName = null,
        [FromQuery] int? quantity = null,
        [FromQuery] decimal? unitPrice = null,
        [FromQuery] Guid? orderId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false)
    {

        logger.Verbose("Start {Operation}", nameof(GetOrderLinesAsync));

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        var query = new GetOrderLinesQuery(
            headersDic,
            pageNumber,
            pageSize,
            includeTotalCount,
            productName,
            quantity,
            unitPrice,
            orderId,
            sortBy,
            descending);

        // execute query
        var page = await mediator.Send(query, cancellationToken);

        logger.Information("Operation: {Operation} executed successfully", nameof(GetOrderLinesAsync));
        logger.Verbose("End {Operation} operation", nameof(GetOrderLinesAsync));

        return Ok(page);
    }

    #endregion
}