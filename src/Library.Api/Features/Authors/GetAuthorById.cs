﻿using Library.Api.DTOs.Responses;

namespace Library.Api.Features.Authors
{
	public static class GetAuthorById
	{
		public record Query(string AuthorId) : IRequest<Result<AuthorResponse>>;

		internal sealed class Handler(IAuthorRepository repository) : IRequestHandler<Query, Result<AuthorResponse>>
		{
			private readonly IAuthorRepository _repository = repository;
			public async Task<Result<AuthorResponse>> Handle(Query request, CancellationToken cancellationToken)
			{
				var author = await _repository.GetById(request.AuthorId);

				if (author is null)
					return Result<AuthorResponse>.NotFound();

				var authorResponse = author.Adapt<AuthorResponse>();

				return Result.Success(authorResponse);
			}
		}
	}

	public class GetAuthorByIdEndpoint : ICarterModule
	{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapGet("api/authors/{authorId}", async (string authorId, ISender sender) =>
			{
				var result = await sender.Send(
					new GetAuthorById.Query(authorId));

				return result.HandleResult();

			}).Produces<AuthorResponse>()
			.WithTags("Authors")
			.WithSummary("Get author by id.")
			.WithOpenApi();
		}
	}
}
