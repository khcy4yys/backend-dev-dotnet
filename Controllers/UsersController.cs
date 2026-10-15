using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private static readonly ConcurrentDictionary<int, User> Users = new();
	private static int nextId;

	[HttpGet]
	public ActionResult<IEnumerable<User>> GetAll()
	{
		return Ok(Users.Values.OrderBy(user => user.Id));
	}

	[HttpGet("{id:int}")]
	public ActionResult<User> GetById(int id)
	{
		return Users.TryGetValue(id, out var user) ? Ok(user) : NotFound();
	}

	[HttpPost]
	public ActionResult<User> Create(User user)
	{
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
	public IActionResult Update(int id, User user)
	{
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
