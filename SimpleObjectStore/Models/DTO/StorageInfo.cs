﻿namespace SimpleObjectStore.Models.DTO;

public class StorageInfo
{
    public float FreeGB { get; set; }
    public float SizeGB { get; set; }
    public string Name { get; set; }
    public float AvailablePercent { get; set; }
}