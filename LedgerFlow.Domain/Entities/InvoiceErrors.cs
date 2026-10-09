namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public static class InvoiceErrors
{
    public static readonly Error InvalidOrganizationId = new(
        "Invoice.InvalidOrganizationId",
        "Organization ID is invalid");

    public static readonly Error NumberRequired = new(
        "Invoice.NumberRequired",
        "Invoice number is required");

    public static readonly Error CustomerNameRequired = new(
        "Invoice.CustomerNameRequired",
        "Customer name is required");

    public static readonly Error CurrencyRequired = new(
        "Invoice.CurrencyRequired",
        "Currency is required");

    public static readonly Error NotEditable = new(
        "Invoice.NotEditable",
        "Invoice is not editable in its current status");

    public static readonly Error DescriptionRequired = new(
        "Invoice.DescriptionRequired",
        "Line description is required");

    public static readonly Error InvalidQuantity = new(
        "Invoice.InvalidQuantity",
        "Line quantity must be greater than zero");

    public static readonly Error InvalidUnitPrice = new(
        "Invoice.InvalidUnitPrice",
        "Unit price must be greater than zero");

    public static readonly Error EmptyLines = new(
        "Invoice.EmptyLines",
        "Invoice must have at least one line item");

    public static readonly Error InvalidStatusTransition = new(
        "Invoice.InvalidStatusTransition",
        "Invalid invoice status transition");

    public static readonly Error CurrencyMismatch = new(
        "Invoice.CurrencyMismatch",
        "Line price currency must match invoice currency");
}
