﻿using System.Text.Json.Serialization;

namespace YuiiDev.Bitwarden.VaultItemTypes;

public class SecureNoteItem : BaseVaultItem
{
    [JsonPropertyName("notes")] public string Content { get; set; }
}