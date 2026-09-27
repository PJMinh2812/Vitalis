using Vitalis.Application.DTOs.Doctors;

namespace Vitalis.Application.Exceptions;

// VC-03: the slot the client showed was taken by someone else between page
// load and submit — carries a freshly recomputed slot list so the client can
// re-render the picker without a full reload. Still a 409 (inherits ConflictException).
public class SlotUnavailableException(IReadOnlyList<SlotDto> availableSlots)
    : ConflictException("Khung giờ này vừa được đặt, vui lòng chọn giờ khác")
{
    public IReadOnlyList<SlotDto> AvailableSlots { get; } = availableSlots;
}
