using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VibeTree.Shared.Common;



public record Result  
{
    [JsonConstructor]
    protected Result() { }

    public Result(bool IsSuccess, List<Error>? Errors = default)
    {
        this.Errors = Errors ?? new List<Error>();
        this.IsSuccess = IsSuccess;
    }
    public List<Error>? Errors { get; init; } = default;
    public bool IsSuccess { get; init; }

    public static Result Success() => new (true, null);

    public static Result Failure(List<Error> errors) => new Result(false, errors);

    public static Result Failure(Error error) => new Result(false, new List<Error>() { error });

    public static implicit operator Result(List<Error> error) => Failure(error);
    public static implicit operator Result(Error error) => Failure(error);
}

public record  Result<T> : Result
{
    public T Value { get; } = default!;

    [JsonConstructor]
    protected Result(T value) : base(true, null) => this.Value = value;

    public Result(List<Error> error) : base(false, error) { }

    public Result(Error error) : base(false, new List<Error> { error }) { }

    public static implicit operator Result<T>(T value) => new Result<T>(value);

    public static implicit operator Result<T>(List<Error> error) => new Result<T>(error);

    public static implicit operator Result<T>(Error error) => new Result<T>(error);




}

