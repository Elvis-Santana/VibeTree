using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Application.Result;


public record Result(bool isSuccess, List<Error>? error = default)
{
    public List<Error>? Error  => error;

    public bool IsSuccess   => isSuccess;
    public static Result Success()=> new Result(true, null);
    
    public static Result Failure(List<Error> error) => new Result(false,error);
    public static Result Failure(Error error) => new(false, [error]);
    public static implicit operator Result(List<Error> error) => Failure(error);
    public static implicit operator Result(Error error) => Failure(error);


}

public record Result<T>: Result
{
    public T Value { get; } = default!;

    private Result(T value ) : base(true,null) => this.Value = value;
    public Result(List<Error> error) : base(false, error) { }

    public static implicit operator Result<T>(T value) => new Result<T>(value);

    public static implicit operator Result<T>(List<Error> error) => new(error);

    public static implicit operator Result<T>(Error error) => new(error);




}

