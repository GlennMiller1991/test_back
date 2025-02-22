using Microsoft.EntityFrameworkCore;
using WebApplication1.models;

namespace WebApplication1.Services.GuestEntryService;

public class GuestEntryService
{
    private AppDbContext context;

    public GuestEntryService(AppDbContext _context)
    {
        context = _context;
        context.Database.EnsureCreated();
    }

    public async Task<string> CreateGuestEntry(string ip)
    {
        var guestEntry = new GuestEntry { IpId = ip };
        context.Add(guestEntry);
        await context.SaveChangesAsync();
        return guestEntry.IpId;
    }

    public async Task<GuestEntry?> GetGuestEntry(string ip)
    {
        Console.WriteLine(ip);
        return await context.GuestEntries.SingleOrDefaultAsync((guestEntry) => guestEntry.IpId == ip);
    }

    public async Task<GuestEntry?> UpdateGuestEntry(string ip, DateTime date)
    {
        var guestEntry = await GetGuestEntry(ip);
        if (guestEntry == null)
        {
            return null;
        }

        Console.WriteLine("Hello");

        guestEntry.Date = date;
        context.Update(guestEntry);
        await context.SaveChangesAsync();
        return guestEntry;
    }
}