﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BookNest_Repositories.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; }

    public string Description { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}