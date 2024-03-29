﻿using CoffeeShop.Data.Entities;
using CoffeeShop.Employees.Shared;
using CoffeeShop.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Employees.Pages;

public partial class EditEmployee
{
    [Inject]
    public IDbContextFactory<EmployeeManagerDbContext>? ContextFactory { get; set; }

    [Inject]
    public NavigationManager? NavigationManager { get; set; }

    [Inject]
    public StateContainer? StateContainer { get; set; }

    [Parameter]
    public Guid EmployeeId { get; set; }

    private Employee? Employee { get; set; }

    private Department[]? Departments { get; set; }

    private bool IsBusy { get; set; }

    private string? ErrorMessage { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        IsBusy = true;

        try
        {
            using var context = ContextFactory!.CreateDbContext();
            Departments = await context.Departments
                             .AsNoTracking()
                             .OrderBy(dept => dept.Name)
                             .ToArrayAsync();
            Employee = await context.Employees
                             .AsNoTracking()
                             .FirstOrDefaultAsync(emp => emp.Id == EmployeeId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task HandleSubmit(bool isValid)
    {
        if (Employee is null || IsBusy || !isValid)
        {
            ErrorMessage = null;
            return;
        }

        IsBusy = true;

        try
        {
            using var context = ContextFactory!.CreateDbContext();
            context.Update(Employee);
            await context.SaveChangesAsync();

            NavigateToEmployeesListPage();
        }
        catch (DbUpdateConcurrencyException)
        {
            ErrorMessage = "The employee was modified by another user. Please reload this page.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error while saving employee: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NavigateToEmployeesListPage()
    {
        NavigationManager?.NavigateTo($"/employees/list/{StateContainer!.ListEmployees}");
    }

}
