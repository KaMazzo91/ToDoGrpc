using System.Data;
using System.Runtime.CompilerServices;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbcontext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbcontext = dbContext;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if(request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var ToDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbcontext.AddAsync(ToDoItem);
        await _dbcontext.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = ToDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0)
           throw new RpcException(new Status(StatusCode.InvalidArgument, "resource index must be greater than 0"));
        
        var todoItem = await _dbcontext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(todoItem != null)
        {
            return await Task.FromResult(new ReadToDoResponse
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                ToDoStatus = todoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"no Task with id {request.Id}"));
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var todoItems = await _dbcontext.ToDoItems.ToListAsync();

        foreach(var toDo in todoItems)
        {
            response.ToDo.Add(new ReadToDoResponse{
                Id = toDo.Id,
                Title = toDo.Title,
                Description = toDo.Description,
                ToDoStatus = toDo.ToDoStatus
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
        
        var todoItem = await _dbcontext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(todoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"no Task with id {request.Id}"));

        todoItem.Title = request.Title;
        todoItem.Description = request.Description;
        todoItem.ToDoStatus = request.ToDoStatus;

        await _dbcontext.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse{
            Id = todoItem.Id
        });
    }
    
    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0)
           throw new RpcException(new Status(StatusCode.InvalidArgument, "resource index must be greater than 0"));
        
        var todoItem = await _dbcontext.ToDoItems.FirstOrDefaultAsync(t=>t.Id == request.Id);

        if(todoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"no Task with id {request.Id}"));

        _dbcontext.Remove(todoItem);
        await _dbcontext.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse {
            Id = todoItem.Id
        });
    }
}