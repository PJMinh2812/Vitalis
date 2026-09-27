using FluentAssertions;
using Moq;
using Vitalis.Application.DTOs.Appointments;
using Vitalis.Application.DTOs.Doctors;
using Vitalis.Application.Exceptions;
using Vitalis.Application.Interfaces;
using Vitalis.Application.Services;
using Vitalis.Domain.Entities.Scheduling;
using Vitalis.Domain.Enums;

namespace Vitalis.Application.UnitTests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDoctorService> _doctorService = new();
    private readonly Mock<IRepository<Patient>> _patientRepository = new();
    private readonly Mock<IRepository<Doctor>> _doctorRepository = new();
    private readonly Mock<IRepository<Appointment>> _appointmentRepository = new();
    private readonly Mock<IRepository<AppointmentStatusHistory>> _historyRepository = new();
    private readonly Mock<IRepository<Notification>> _notificationRepository = new();
    private readonly AppointmentService _sut;

    // A fixed future instant, always tomorrow at 09:00 regardless of when the
    // test suite runs — avoids "GreaterThan(DateTime.UtcNow)"-style flakiness.
    private static readonly DateTime Tomorrow9Am = DateTime.Today.AddDays(1).AddHours(9);

    public AppointmentServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<Patient>()).Returns(_patientRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Doctor>()).Returns(_doctorRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Appointment>()).Returns(_appointmentRepository.Object);
        _unitOfWork.Setup(u => u.Repository<AppointmentStatusHistory>()).Returns(_historyRepository.Object);
        _unitOfWork.Setup(u => u.Repository<Notification>()).Returns(_notificationRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new AppointmentService(_unitOfWork.Object, _doctorService.Object);
    }

    private void SetUpPatientAndDoctor(bool doctorActive = true, decimal? fee = 150000)
    {
        _patientRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = 1, FullName = "Nguyen Van A" });
        _doctorRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Doctor { Id = 1, FullName = "Bac si B", IsActive = doctorActive, ConsultationFee = fee });
    }

    [Fact]
    public async Task BookAsync_WhenSlotIsAvailable_CreatesAppointmentAsPending()
    {
        SetUpPatientAndDoctor();
        var requestedStart = TimeOnly.FromDateTime(Tomorrow9Am);
        _doctorService
            .Setup(d => d.GetAvailableSlotsAsync(1, DateOnly.FromDateTime(Tomorrow9Am), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SlotDto(requestedStart, requestedStart.AddMinutes(30), true)]);
        _appointmentRepository
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => a.Id = 42) // simulates the IDENTITY value SQL Server would assign
            .Returns(Task.CompletedTask);

        var request = new CreateAppointmentRequest(1, 1, Tomorrow9Am, "Kham tong quat", AppointmentSource.Online);
        var result = await _sut.BookAsync(request, createdBy: 10);

        result.Status.Should().Be(AppointmentStatus.Pending);
        result.AppointmentCode.Should().Be("LH000042");
        result.FeeSnapshot.Should().Be(150000);
        _historyRepository.Verify(r => r.AddAsync(
            It.Is<AppointmentStatusHistory>(h => h.FromStatus == null && h.ToStatus == AppointmentStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // CLAUDE.md: "đặt lịch trùng giờ phải bị từ chối" — the slot exists on the
    // doctor's schedule but is already taken (IsAvailable = false).
    [Fact]
    public async Task BookAsync_WhenSlotIsAlreadyBooked_ThrowsSlotUnavailable()
    {
        SetUpPatientAndDoctor();
        var requestedStart = TimeOnly.FromDateTime(Tomorrow9Am);
        _doctorService
            .Setup(d => d.GetAvailableSlotsAsync(1, DateOnly.FromDateTime(Tomorrow9Am), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SlotDto(requestedStart, requestedStart.AddMinutes(30), false)]);

        var request = new CreateAppointmentRequest(1, 1, Tomorrow9Am, "Kham tong quat", AppointmentSource.Online);
        var act = () => _sut.BookAsync(request, createdBy: 10);

        await act.Should().ThrowAsync<SlotUnavailableException>();
        _appointmentRepository.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // CLAUDE.md: "đặt ngoài giờ làm việc phải lỗi" — the doctor's schedule simply
    // never generates a slot at the requested time (e.g. before opening hours).
    [Fact]
    public async Task BookAsync_WhenTimeIsOutsideWorkingHours_ThrowsSlotUnavailable()
    {
        SetUpPatientAndDoctor();
        _doctorService
            .Setup(d => d.GetAvailableSlotsAsync(1, DateOnly.FromDateTime(Tomorrow9Am), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SlotDto(new TimeOnly(14, 0), new TimeOnly(14, 30), true)]);

        var request = new CreateAppointmentRequest(1, 1, Tomorrow9Am, "Kham tong quat", AppointmentSource.Online);
        var act = () => _sut.BookAsync(request, createdBy: 10);

        await act.Should().ThrowAsync<SlotUnavailableException>();
        _appointmentRepository.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BookAsync_WhenDoctorIsInactive_ThrowsConflict()
    {
        SetUpPatientAndDoctor(doctorActive: false);

        var request = new CreateAppointmentRequest(1, 1, Tomorrow9Am, "Kham tong quat", AppointmentSource.Online);
        var act = () => _sut.BookAsync(request, createdBy: 10);

        await act.Should().ThrowAsync<ConflictException>();
        _doctorService.Verify(d => d.GetAvailableSlotsAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The pre-check passes, but the DB unique index (UX_appointments_doctor_slot)
    // rejects the insert because someone else grabbed the exact same slot first —
    // UnitOfWork.SaveChangesAsync translates that into ConflictException, and
    // BookAsync must turn it into a SlotUnavailableException with a fresh list.
    [Fact]
    public async Task BookAsync_WhenSaveChangesHitsUniqueIndexRace_ThrowsSlotUnavailableWithFreshSlots()
    {
        SetUpPatientAndDoctor();
        var requestedStart = TimeOnly.FromDateTime(Tomorrow9Am);
        var date = DateOnly.FromDateTime(Tomorrow9Am);

        _doctorService.SetupSequence(d => d.GetAvailableSlotsAsync(1, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SlotDto(requestedStart, requestedStart.AddMinutes(30), true)])   // pre-check: still free
            .ReturnsAsync([new SlotDto(requestedStart, requestedStart.AddMinutes(30), false)]); // refreshed after the race: taken

        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("unique index violation"));

        var request = new CreateAppointmentRequest(1, 1, Tomorrow9Am, "Kham tong quat", AppointmentSource.Online);
        var act = () => _sut.BookAsync(request, createdBy: 10);

        var thrown = await act.Should().ThrowAsync<SlotUnavailableException>();
        thrown.Which.AvailableSlots.Should().BeEmpty();
    }

    // CLAUDE.md: "hủy lịch đã hoàn thành phải lỗi"
    [Fact]
    public async Task CancelAsync_WhenAppointmentIsAlreadyCompleted_ThrowsConflict()
    {
        var appointment = new Appointment { Id = 1, Status = AppointmentStatus.Completed, StartTime = DateTime.Today.AddDays(-1) };
        _appointmentRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var act = () => _sut.CancelAsync(1, "Doi y", changedBy: 10);

        await act.Should().ThrowAsync<ConflictException>();
        _appointmentRepository.Verify(r => r.Update(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_WhenTooCloseToStartTime_ThrowsConflict()
    {
        // The service approximates "now" as UtcNow+7h; a StartTime only 1 hour
        // from that is within the 2-hour cutoff regardless of the host's own timezone.
        var nowLocal = DateTime.UtcNow.AddHours(7);
        var appointment = new Appointment { Id = 1, Status = AppointmentStatus.Pending, StartTime = nowLocal.AddHours(1) };
        _appointmentRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var act = () => _sut.CancelAsync(1, "Doi y", changedBy: 10);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelAsync_WhenPendingAndFarEnoughAhead_Succeeds()
    {
        var nowLocal = DateTime.UtcNow.AddHours(7);
        var appointment = new Appointment { Id = 1, PatientId = 1, DoctorId = 1, Status = AppointmentStatus.Pending, StartTime = nowLocal.AddDays(1) };
        _appointmentRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        SetUpPatientAndDoctor();

        var result = await _sut.CancelAsync(1, "Doi y", changedBy: 10);

        result.Status.Should().Be(AppointmentStatus.Cancelled);
        result.CancelReason.Should().Be("Doi y");
        _appointmentRepository.Verify(r => r.Update(It.Is<Appointment>(a => a.Status == AppointmentStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenAlreadyCheckedIn_ThrowsConflict()
    {
        var appointment = new Appointment { Id = 1, Status = AppointmentStatus.CheckedIn, StartTime = DateTime.Today.AddDays(1) };
        _appointmentRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var act = () => _sut.CheckInAsync(1, changedBy: 10);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CheckInAsync_AssignsNextQueueNumberForSameDoctorAndDay()
    {
        var today = DateTime.Today.AddDays(1);
        var appointment = new Appointment { Id = 3, PatientId = 1, DoctorId = 1, Status = AppointmentStatus.Pending, StartTime = today.AddHours(10) };
        _appointmentRepository.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);
        SetUpPatientAndDoctor();

        // Two other appointments for the same doctor/day already checked in with queue numbers 1 and 2.
        var existing = new List<Appointment>
        {
            new() { Id = 1, DoctorId = 1, StartTime = today.AddHours(8), QueueNumber = 1 },
            new() { Id = 2, DoctorId = 1, StartTime = today.AddHours(9), QueueNumber = 2 },
        };
        _appointmentRepository
            .Setup(r => r.Query())
            .Returns(existing.AsQueryable());
        _appointmentRepository
            .Setup(r => r.ToListAsync(It.IsAny<IQueryable<Appointment>>(), It.IsAny<CancellationToken>()))
            .Returns<IQueryable<Appointment>, CancellationToken>((q, _) => Task.FromResult(q.ToList()));

        var result = await _sut.CheckInAsync(3, changedBy: 10);

        result.QueueNumber.Should().Be(3);
        result.Status.Should().Be(AppointmentStatus.CheckedIn);
    }
}
