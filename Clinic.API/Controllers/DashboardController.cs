using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.API.Data;
using Clinic.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    [Authorize]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var today = DateTime.Today;

        var totalPatients = await _context.Patients.CountAsync();
        var totalDoctors = await _context.Staffs
            .Where(s => s.StaffType != null && s.StaffType.IsDoctor == 1)
            .CountAsync();

        var totalAppointmentsToday = await _context.Appointments
            .Where(a => a.ScheduleDate.Date == today)
            .CountAsync();

        var totalRevenue = await _context.Payments.SumAsync(p => p.PayingAmount);

        var recentAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.ScheduleDate)
            .ThenByDescending(a => a.ScheduleTime)
            .Take(5)
            .Select(a => new RecentAppointmentDto(
                a.Id,
                a.Patient != null ? a.Patient.FullName : "Unknown",
                a.Doctor != null ? a.Doctor.FullName : "Unknown",
                a.ScheduleDate,
                a.ScheduleTime
            ))
            .ToListAsync();

        // Sessions overview - last 10 days appointments count
        var last10Days = Enumerable.Range(0, 10)
            .Select(i => today.AddDays(-i))
            .Reverse()
            .ToList();

        var appointmentsByDate = await _context.Appointments
            .Where(a => a.ScheduleDate >= today.AddDays(-9))
            .GroupBy(a => a.ScheduleDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var sessionsOverview = last10Days
            .Select(d => new SessionDataDto(
                d.ToString("dd MMM"),
                appointmentsByDate.FirstOrDefault(a => a.Date == d)?.Count ?? 0
            ))
            .ToList();

        return Ok(new DashboardStatsDto(
            totalPatients,
            totalDoctors,
            totalAppointmentsToday,
            totalRevenue,
            recentAppointments,
            sessionsOverview
        ));
    }

    [HttpGet("top-sessions")]
    [Authorize]
    public ActionResult<IEnumerable<TopSessionDto>> GetTopSessions()
    {
        // Sample data mimicking the design
        var topSessions = new List<TopSessionDto>
        {
            new TopSessionDto("Chrome", "Top Browser", "2k+"),
            new TopSessionDto("Windows", "Top Platform", "1.9k+"),
            new TopSessionDto("Stack Overflow", "Top Sources", "1.6k+")
        };

        return Ok(topSessions);
    }

    [HttpGet("summary")]
    [Authorize]
    public async Task<ActionResult<object>> GetSummary()
    {
        var today = DateTime.Today;
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var summary = new
        {
            TotalSessions = await _context.Appointments.CountAsync(),
            TotalVisitors = await _context.Patients.CountAsync(),
            TimeSpentHr = 8.49m, // Demo value
            AvgRequestsReceived = 10.4m, // Demo value
            OnlineVisitors = 320, // Demo value
            AverageRevenue = await _context.Payments.AnyAsync() 
                ? await _context.Payments.AverageAsync(p => p.PayingAmount) 
                : 0
        };

        return Ok(summary);
    }
}
