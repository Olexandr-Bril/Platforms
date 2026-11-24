namespace SplitBill.Business.Dtos;

public record CreateGroupDto(string Name, List<string>? MemberEmails);

public record AddMemberDto(string Email);

public record GroupShortDto(int Id, string Name, int MembersCount);

public record GroupDto(int Id, string Name, int OwnerId, DateTime CreatedAt, List<UserDto> Members);
