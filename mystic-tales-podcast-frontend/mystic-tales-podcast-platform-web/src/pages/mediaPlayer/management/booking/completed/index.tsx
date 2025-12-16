import { useGetCompletedBookingsQuery } from "@/core/services/booking/booking.service";
import type { CompletedBooking } from "@/core/types/booking";
import { useEffect, useState } from "react";
import BookingCard from "./components/BookingCard/BookingCard";

const CompletedBookingsPage = () => {
  // STATES
  const [filteredCompletedBookings, setFilteredCompletedBookings] = useState<
    CompletedBooking[]
  >([]);
  // HOOKS
  const { data: completedBookingsRaw, isFetching: isLoadingCompletedBookings } =
    useGetCompletedBookingsQuery(undefined, {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    });

  useEffect(() => {
    if (isLoadingCompletedBookings || !completedBookingsRaw) {
      return;
    }
    setFilteredCompletedBookings(completedBookingsRaw.BookingList);
  }, [completedBookingsRaw, isLoadingCompletedBookings]);

  return (
    <div className="w-full flex-1 flex flex-col gap-5">
      <p className="font-poppins text-5xl m-8 text-white font-bold">
        Completed Bookings
      </p>
      <div className="w-full mx-8 grid grid-cols-4 gap-5 ">
        {filteredCompletedBookings.map((booking) => (
          <BookingCard booking={booking} key={booking.Id} />
        ))}
      </div>
    </div>
  );
};
export default CompletedBookingsPage;
