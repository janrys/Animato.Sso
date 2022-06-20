namespace Animato.Sso.Domain.Entities;
using System;

public class Scope
{
    public ScopeId Id { get; set; }
    public string Name { get; set; }

    public static readonly Scope All = new() { Id = new ScopeId(new Guid("44461D08-A21F-4EEB-862A-182503C14333")), Name = nameof(All) };
    public static readonly Scope General = new() { Id = new ScopeId(new Guid("06B61D08-A21F-4EEB-862A-0E565840D138")), Name = nameof(General) };
    public static readonly Scope Mail = new() { Id = new ScopeId(new Guid("88248AE1-079F-4BFF-9CF6-182503C14578")), Name = nameof(Mail) };
    public static readonly Scope Online = new() { Id = new ScopeId(new Guid("C4A1B2FC-D031-449C-B25C-BDDA6848F798")), Name = nameof(Online) };
    public static readonly Scope Phone = new() { Id = new ScopeId(new Guid("E88415E3-EE79-43CD-A163-40AF1E7DBDA7")), Name = nameof(Phone) };
    public static readonly Scope Role = new() { Id = new ScopeId(new Guid("A2DBD865-7A90-44FA-B2F3-B43DD3D11199")), Name = nameof(Role) };
}

