using System;
using System.Collections.Generic;

namespace member.Models;

public partial class UserProfile
{
    public Guid UserId { get; set; }

    public string? FullName { get; set; }

    public DateOnly? Birthday { get; set; }

    public string? Address { get; set; }
}
