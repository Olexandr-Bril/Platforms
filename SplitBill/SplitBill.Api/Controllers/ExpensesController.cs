using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitBill.Business.Dtos;
using SplitBill.Business.Interfaces;

namespace SplitBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:int}/expenses")]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Add(int groupId, CreateExpenseDto dto)
    {
        ExpenseDto expense = await _expenseService.AddAsync(CurrentUserId, groupId, dto);
        return StatusCode(StatusCodes.Status201Created, expense);
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll(int groupId)
    {
        return Ok(await _expenseService.GetAllAsync(CurrentUserId, groupId));
    }
}
