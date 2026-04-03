namespace Web.Models;

public sealed record SelectOptionModel<TValue>(TValue Value, string Label);
