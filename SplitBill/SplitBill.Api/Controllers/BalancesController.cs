using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitBill.Business.Dtos;
using SplitBill.Business.Interfaces;

namespace SplitBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:int}")]
public class BalancesController : BaseController
{
    private readonly IBalanceService _balanceService;

    public BalancesController(IBalanceService balanceService)
    {
        _balanceService = balanceService;
    }

    [HttpGet("balances")]
    public async Task<ActionResult<GroupBalancesDto>> GetBalances(int groupId)
    {
        return Ok(await _balanceService.GetBalancesAsync(CurrentUserId, groupId));
    }

    [HttpPost("payments")]
    public async Task<ActionResult<PaymentDto>> AddPayment(int groupId, CreatePaymentDto dto)
    {
        PaymentDto payment = await _balanceService.AddPaymentAsync(CurrentUserId, groupId, dto);
        return StatusCode(StatusCodes.Status201Created, payment);
    }
}
