using System.ComponentModel.DataAnnotations;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Commands;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Orders.Application.Queries;
using Arch.Hexa.ModuMono.Orders.RestApi.Contracts.Requests.Order;
using Arch.Hexa.ModuMono.Orders.RestApi.Security;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Arch.Hexa.ModuMono.Orders.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    IMediator mediator, 
    ILogger logger, 
    IHttpContextAccessor httpContextAccessor,
    IValidator<CreateOrderRequest> createOrderRequestValidator) 
    : ControllerBase
{
    #region Order

    /// <summary>
    /// Get Order By ID
    /// </summary>
    /// <param name="id">Search ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>Response Data with OrderDto founded</returns>
    [HttpGet]
    [Route("getOrderByIdAsync/v1/{id:guid}", Name = "GetOrderByIdAsync")]
    [Authorize(Policy = OrdersPolicies.Read)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderByIdAsync([Required] Guid id, CancellationToken cancellationToken)
    {
        logger.Verbose("Start {Operation}", nameof(GetOrderByIdAsync));

        // With {id:guid} the empty Guid can still be passed, so validate it.
        if (id == Guid.Empty)
            throw new BadRequestException("Invalid id. The id parameter cannot be an empty Guid.");

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Execute query (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var order = await mediator.Send(new GetOrderByIdQuery(headersDic, id), cancellationToken);



        logger.Information("{Operation} executed successfully", nameof(GetOrderByIdAsync));
        logger.Verbose("End {Operation}", nameof(GetOrderByIdAsync));

        return Ok(order);
    }


    /// <summary>
    /// Get Orders with filtering, sorting and pagination
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <param name="includeTotalCount">Include or not Total Count optional</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="createdFrom">Created From Date</param>
    /// <param name="createdTo">Created To Date</param>
    /// <param name="sortBy">Sort By</param>
    /// <param name="descending">Descending Order</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>List of Orders</returns>
    [HttpGet]
    [Route("getOrdersAsync/v1", Name = "GetOrdersAsync")]
    [Authorize(Policy = OrdersPolicies.Read)]
    [ProducesResponseType(typeof(PagedResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersAsync(
        [FromQuery, Required] int pageNumber,
        [FromQuery, Required] int pageSize,
        CancellationToken cancellationToken,
        [FromQuery] bool includeTotalCount = false,
        [FromQuery] string? customerId = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false)
    {

        logger.Verbose("Start {Operation}", nameof(GetOrdersAsync));

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Optional: basic validation (otherwise let your validators throw)
        if (pageNumber <= 0 || pageSize < 0)
            throw new BadRequestException("Invalid pagination parameters. PageNumber must be greater than 0 and PageSize must be non-negative.");

        var query = new GetOrdersQuery(
            headersDic,
            pageNumber,
            pageSize,
            includeTotalCount,
            customerId,
            createdFrom,
            createdTo,
            sortBy,
            descending);

        // Execute query (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var page = await mediator.Send(query, cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(GetOrdersAsync));
        logger.Verbose("End {Operation}", nameof(GetOrdersAsync));

        return Ok(page);
    }

    /// <summary>
    /// Create a new Order
    /// </summary>
    /// <param name="request">Create Order Request</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="201">Created</response>
    /// <response code="400">Bad Request</response>
    /// <response code="409">Conflict</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>Response Data with OrderDto created</returns>
    [HttpPost]
    [Route("createOrderAsync/v1", Name = "CreateOrderAsync")]
    [Authorize(Policy = OrdersPolicies.Write)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrderAsync([FromBody, Required] CreateOrderRequest request, CancellationToken cancellationToken)
    {

        logger.Verbose($"Start {nameof(CreateOrderAsync)} operation");

        //validate request
        var validationResult = await ValidationsHelper.Validations(createOrderRequestValidator, request);
        if (validationResult != null) // 400 Bad Request
            throw new BadRequestException(validationResult);

        //get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // mapping from request to dto
        var orderLineDto = request.Lines.Select(line => new OrderLineDto
        {
            ProductName = line.ProductName,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice
        });

        // Execute command  (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var order = await mediator.Send(new CreateOrderCommand(headersDic, request.CustomerId, orderLineDto), cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(CreateOrderAsync));
        logger.Verbose("End {Operation}", nameof(CreateOrderAsync));

        // Standard: 201 + Location header to the GET by id endpoint
        return CreatedAtRoute("GetOrderByIdAsync", new { id = order!.Id }, order);

    }


    #endregion
}

