#nullable enable
using System.Collections.Generic;

public sealed record PromotionResult(string FromName, IReadOnlyList<PromotionOption> Options);