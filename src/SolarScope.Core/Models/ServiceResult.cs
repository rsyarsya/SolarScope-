namespace SolarScope.Core.Models;

public class ServiceResult<T>{
    public bool IsSuccess {get; init;}
    public T? Data {get; init;}
    public string? ErrorMessage {get; init;}

    public static ServiceResult<T> Ok(T data) => new(){
        IsSuccess = true,
        Data = data
    };
    public static ServiceResult<T> Fail(string err) => new(){
        IsSuccess = false,
        ErrorMessage = err
    };
}