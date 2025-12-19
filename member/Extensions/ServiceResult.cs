namespace member.Extensions
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "成功";
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data };
        }

        public static ServiceResult<T> Fail(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }


}
