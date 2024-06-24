using MediatR;

using Microsoft.AspNetCore.Mvc;
namespace HotelDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    public BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

}
