namespace ApiApplication.Results
{
    public class CommonResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static CommonResult<T> SuccessResult(T data, string message = "Operation successful") =>
            new() { Success = true, Message = message, Data = data };

        public static CommonResult<T> FailureResult(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}