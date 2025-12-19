using System.Globalization;

namespace member.Dtos
{
    public static  class UserDto
    { 
        public record LoginRequest(string Email, string Password);
        public record LoginResponse(string Id, string FullName,string Birthday,string Address, string Email,int MemberId);

        public record CreateRequest( string Email, string Password, string FullName, string? Birthday, string? Address );

        public record CreateResponse(string Id, string FullName, string Birthday, string Address, string Email, int MemberId);

        public record UpdateRequest(
            string Id,
            string Password,
            int Status,
            string Address
        );

    }
}
