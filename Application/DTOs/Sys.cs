﻿using System.Text.Json.Serialization;

namespace Application.DTOs;

public class Sys
{
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("country")]
    public required string Country { get; set; }

    [JsonPropertyName("sunrise")]
    public int Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public int Sunset { get; set; }
}