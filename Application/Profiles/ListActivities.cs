using Application.Activities;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profiles
{
  public class ListActivities
  {
    public class Query : IRequest<Result<List<UserActivityDto>>>
    {
      public string Username { get; set; }
      public string Predicate { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<UserActivityDto>>>
    {
      private readonly IUserAccessor _userAccessor;
      private readonly IMapper _mapper;
      private readonly DataContext _context;
      public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
      {
        _context = context;
        _mapper = mapper;
        _userAccessor = userAccessor;
      }

      public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
      {
        var query = _context.ActivityAttendees
          .Where(u => u.AppUser.UserName == request.Username)
          .OrderBy(a => a.Activity.Date)
          .ProjectTo<UserActivityDto>(_mapper.ConfigurationProvider).AsQueryable();

        if (request.Predicate == "past")
        {
          query = query.Where(a => a.Date <= DateTime.UtcNow);
        }

        else if (request.Predicate == "hosting")
        {
          query = query.Where(a => a.HostUsername == request.Username);
        }
        else
        {
          query = query.Where(a => a.Date >= DateTime.UtcNow);
        }

        var activities = await query.ToListAsync(cancellationToken);

        return Result<List<UserActivityDto>>.Success(activities);
      }
    }
  }
}