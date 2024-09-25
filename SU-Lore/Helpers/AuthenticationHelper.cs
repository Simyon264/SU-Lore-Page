using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SU_Lore.Database;
using SU_Lore.Database.Models.Accounts;

namespace SU_Lore.Helpers;

public class AuthenticationHelper
{
    private readonly ApplicationDbContext _context;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationHelper(ApplicationDbContext context, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager)
    {
        _context = context;
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
    }

    public async Task<Account?> FetchAccount(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var guid = Guid.Parse(user.Claims
            .First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == guid);
    }

    public async Task<Account?> FetchAccount(bool redirectOnMissing = false)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();

        if (state.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var guid = Guid.Parse(state.User.Claims
            .First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

        var account = _context.Accounts
            .FirstOrDefault(a => a.Id == guid);

        // If at this point the account is null, redirect to account creation
        if (account == null && redirectOnMissing)
        {
            _navigationManager.NavigateTo("/account/redirect", true);
        }

        return account;
    }
}