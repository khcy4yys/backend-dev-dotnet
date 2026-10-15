using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private static readonly ConcurrentDictionary<int, User> Users = new();
	private static int nextId;

	[HttpGet]
	public ActionResult<PagedResult<User>> GetAll([FromQuery] UserListQuery query)
	{
		var filteredUsers = Users.Values.AsEnumerable();

		if (!string.IsNullOrWhiteSpace(query.Name))
		{
			filteredUsers = filteredUsers.Where(user => user.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));
		}

		if (!string.IsNullOrWhiteSpace(query.Email))
		{
			filteredUsers = filteredUsers.Where(user => user.Email.Contains(query.Email, StringComparison.OrdinalIgnoreCase));
		}

		if (!string.IsNullOrWhiteSpace(query.Department))
		{
			filteredUsers = filteredUsers.Where(user => user.Department.Contains(query.Department, StringComparison.OrdinalIgnoreCase));
		}

		var orderedUsers = filteredUsers.OrderBy(user => user.Id).ToArray();
		var skip = Math.Min((long)(query.Page - 1) * query.PageSize, int.MaxValue);
		var pageItems = orderedUsers.Skip((int)skip).Take(query.PageSize).ToArray();
		var result = new PagedResult<User>(query.Page, query.PageSize, orderedUsers.Length, pageItems);

		return Ok(result);
	}

	[HttpGet("{id:int}")]
	public ActionResult<User> GetById(int id)
	{
		return Users.TryGetValue(id, out var user) ? Ok(user) : NotFound();
	}

	[HttpPost]
	public ActionResult<User> Create(User? user)
	{
		if (user is null)
		{
			return BadRequest(new ProblemDetails
			{
				Status = StatusCodes.Status400BadRequest,
				Title = "Invalid request body",
				Detail = "A user payload is required."
			});
		}

		var createdUser = new User
		{
			Id = Interlocked.Increment(ref nextId),
			Name = user.Name,
			Email = user.Email,
			Department = user.Department
		};

		Users[createdUser.Id] = createdUser;
		return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
	}

	[HttpPut("{id:int}")]
	public IActionResult Update(int id, User? user)
	{
		if (user is null)
		{
			return BadRequest(new ProblemDetails
			{
				Status = StatusCodes.Status400BadRequest,
				Title = "Invalid request body",
				Detail = "A user payload is required."
			});
		}

		if (!Users.TryGetValue(id, out var existingUser))
		{
			return NotFound();
		}

		var updatedUser = new User
		{
			Id = id,
			Name = user.Name,
			Email = user.Email,
			Department = user.Department
		};

		return Users.TryUpdate(id, updatedUser, existingUser) ? NoContent() : Conflict();
	}

	[HttpDelete("{id:int}")]
	public IActionResult Delete(int id)
	{
		return Users.TryRemove(id, out _) ? NoContent() : NotFound();
	}
}
