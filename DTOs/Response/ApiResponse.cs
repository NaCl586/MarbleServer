namespace MarbleServer.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }

        public static ApiResponse<T> Success(
            T? data = default,
            string? message = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}