using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitBill.Business.Dtos;
using SplitBill.Business.Interfaces;

namespace SplitBill.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups")]
public class GroupsController : BaseController
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> Create(CreateGroupDto dto)
    {
        GroupDto group = await _groupService.CreateAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(Get), new { groupId = group.Id }, group);
    }

    [HttpGet]
    public async Task<ActionResult<List<GroupShortDto>>> GetMyGroups()
    {
        return Ok(await _groupService.GetUserGroupsAsync(CurrentUserId));
    }

    [HttpGet("{groupId:int}")]
    public async Task<ActionResult<GroupDto>> Get(int groupId)
    {
        return Ok(await _groupService.GetAsync(CurrentUserId, groupId));
    }

    [HttpPost("{groupId:int}/members")]
    public async Task<ActionResult<GroupDto>> AddMember(int groupId, AddMemberDto dto)
    {
        return Ok(await _groupService.AddMemberAsync(CurrentUserId, groupId, dto));
    }
}
