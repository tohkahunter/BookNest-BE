﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace BookNest_Repositories.Models;

public partial class Genre
{
    public int GenreId { get; set; }

    public string GenreName { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}