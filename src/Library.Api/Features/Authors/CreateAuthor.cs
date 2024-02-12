﻿using Library.Api.DTOs.Requests;

namespace Library.Api.Features.Authors
{
	public static class CreateAuthor
	{
		public record class Command(string FirstName, string LastName, DateTime DateOfBirth) : IRequest<Result<string>>;
		public class Validator : AbstractValidator<Command>
		{
			public Validator()
			{
				RuleFor(p => p.FirstName).NotEmpty().WithError(AuthorErrors.FirstNameIsRequired);
				RuleFor(p => p.LastName).NotEmpty().WithError(AuthorErrors.LastNameIsRequired);
				RuleFor(p => p.DateOfBirth).NotNull().WithError(AuthorErrors.DateOfBirthIsRequired);

				//.WithError(AuthorErrors.PropertyIsRequired(nameof(Author.DateOfBirth)));
			}
		}

		internal sealed class Handler(IDatabaseConnectionFactory connectionFactory) : IRequestHandler<Command, Result<string>>
		{
			private readonly IDatabaseConnectionFactory _connectionFactory = connectionFactory;
			public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
			{
				try
				{
					string id = Guid.NewGuid().ToString();

					var sql = """
					INSERT INTO Author (Id, FirstName, LastName, DateOfBirth)
					VALUES(@Id, @FirstName, @Lastname, @DateOfBirth)
					""";

					await _connectionFactory.Connection.ExecuteAsync(sql, new
					{
						id,
						request.FirstName,
						request.LastName,
						request.DateOfBirth
					});

					return Result.Success(id);

				}
				catch (SqlException error)
				{
					return Result<string>.Failure(Errors.DatabaseError(
						typeof(Author),
						error.Message));
				}
			}
		}
	}

	public class CreateAuthorEndpoint : ICarterModule
	{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapPost("api/authors", async (CreateAuthorRequest request, ISender sender) =>
			{
				var command = request.Adapt<CreateAuthor.Command>();

				var result = await sender.Send(command);

				return result.HandleResult();

			}).WithTags("Authors");
		}
	}
}
