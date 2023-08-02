﻿using System.ComponentModel.DataAnnotations;

namespace SimpleObjectStore.Models;

public class AllowedHost
{
    [Key, StringLength(2048)] public string Hostname { get; set; }
}