using System.ComponentModel.DataAnnotations;
using Arch.Hexa.ModuMono.BuildingBlocks.Api.Helpers;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Exceptions;
using Arch.Hexa.ModuMono.BuildingBlocks.Application.Querying;
using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;
using Arch.Hexa.ModuMono.Customers.RestApi.Contracts.Requests.Customer;
using Arch.Hexa.ModuMono.Customers.RestApi.Security;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Commands;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Dtos;
using Arch.Hexa.ModuMono.Modules.Customers.Application.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Arch.Hexa.ModuMono.Customers.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(
    IMediator mediator, 
    ILogger logger, 
    IHttpContextAccessor httpContextAccessor,
    IValidator<CreateCustomerRequest> createCustomerRequestValidator) 
    : ControllerBase
{
    /// <summary>
    /// Get Customer By ID
    /// </summary>
    /// <param name="id">Search ID</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server error</response>
    [HttpGet]
    [Route("getCustomerByIdAsync/v1/{id:guid}", Name = "GetCustomerByIdAsync")]
    [Authorize(Policy = CustomersPolicies.Read)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> GetCustomerByIdAsync([Required] Guid id, CancellationToken cancellationToken)
    {
        logger.Verbose("Start {Operation}", nameof(GetCustomerByIdAsync));

        // With {id:guid} the empty Guid can still be passed, so validate it.
        if (id == Guid.Empty)
            throw new BadRequestException("Invalid id. The id parameter cannot be an empty Guid.");

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Execute query (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var customer = await mediator.Send(new GetCustomerByIdQuery(headersDic, id), cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(GetCustomerByIdAsync));
        logger.Verbose("End {Operation}", nameof(GetCustomerByIdAsync));

        return Ok(customer);
    }

    /// <summary>
    /// Get Customers with filtering, sorting and pagination
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <param name="includeTotalCount">Include or not Total Count optional</param>
    /// <param name="name">Filter by Name optional</param>
    /// <param name="email">Filter by Email optional</param>
    /// <param name="status">Filter by Status optional</param>
    /// <param name="createdFrom">Filter by CreatedFrom optional</param>
    /// <param name="createdTo">Filter by CreatedTo optional</param>
    /// <param name="sortBy">Sort by field optional</param>
    /// <param name="descending">Sort descending optional</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="500">Internal Server error</response>
    /// <returns>Paged result of customers</returns>
    [HttpGet]
    [Route("getCustomersAsync/v1", Name = "GetCustomersAsync")]
    [Authorize(Policy = CustomersPolicies.Read)]
    [ProducesResponseType(typeof(PagedResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomersAsync(
        [FromQuery, Required] int pageNumber,
        [FromQuery, Required] int pageSize,
        CancellationToken cancellationToken,
        [FromQuery] bool includeTotalCount = false,
        [FromQuery] string? name = null,
        [FromQuery] string? email = null,
        [FromQuery] CustomerStatus? status = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false
        )
    {
        logger.Verbose("Start {Operation}", nameof(GetCustomersAsync));

        // Get headers
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Optional: basic validation (otherwise let your validators throw)
        if (pageNumber <= 0 || pageSize < 0)
            throw new BadRequestException("Invalid pagination parameters. PageNumber must be greater than 0 and PageSize must be non-negative.");


        var query = new GetCustomersQuery(
            headersDic,
            pageNumber,
            pageSize,
            includeTotalCount,
            name,
            email,
            status,
            createdFrom,
            createdTo,
            sortBy,
            descending);

        // Execute query (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var page = await mediator.Send(query, cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(GetCustomersAsync));
        logger.Verbose("End {Operation}", nameof(GetCustomersAsync));

        return Ok(page);
    }

    /// <summary>
    /// Create a new Customer
    /// </summary>
    /// <param name="request">Customer data</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="201">Created</response>
    /// <response code="400">Bad Request</response>
    /// <response code="409">Conflict</response>
    /// <response code="500">Internal Server error</response>
    [HttpPost]
    [Route("createCustomerAsync/v2", Name = "CreateCustomerAsync")]
    [Authorize(Policy = CustomersPolicies.Write)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> CreateCustomerAsync([FromBody, Required] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        logger.Verbose("Start {Operation}", nameof(CreateCustomerAsync));

        //validate request
        var validationResult = await ValidationsHelper.Validations(createCustomerRequestValidator, request);
        if (validationResult != null) // 400 Bad Request
            throw new BadRequestException(validationResult);

        // Get headers 
        var headers = HttpHelper.GetHttpHeaders(httpContextAccessor);
        var headersDic = HeaderHelper.ToDictionaryAllValues(headers);

        // Execute command  (handler should throw for domain errors; ApiExceptionHandler maps to ProblemDetails)
        var customer = await mediator.Send(new CreateCustomerCommand(headersDic, request.Name, request.Email), cancellationToken);

        logger.Information("{Operation} executed successfully", nameof(CreateCustomerAsync));
        logger.Verbose("End {Operation}", nameof(CreateCustomerAsync));

        // Standard: 201 + Location header to the GET by id endpoint
        return CreatedAtRoute("GetCustomerByIdAsync", new { id = customer!.Id }, customer);

    }
}

