using System;
using System.Collections.Generic;

namespace member.Models;

public partial class UserRole
{
    public Guid UserId { get; set; }

    public int RoleId { get; set; }
}
