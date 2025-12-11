/* eslint-disable @typescript-eslint/no-unused-vars */
// @ts-nocheck

import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { Skeleton } from "@/components/ui/skeleton";
import { useEffect, useState } from "react";
import { useGetBookingsQuery } from "@/core/services/booking/booking.service";
import BookingCard from "./components/BookingCard";
import "./styles.css";
import ShowOnHoverButton from "@/components/button/ShowOnHoverButton";
import { MdNoteAdd } from "react-icons/md";
import { RiFileAddFill } from "react-icons/ri";
import { useNavigate } from "react-router-dom";

const ITEMS_PER_PAGE = 4;

const BookingStatusesForFilter = [
  { id: 1, name: "Quotation Request" },
  { id: 2, name: "Quotation Dealing" },
  { id: 3, name: "Quotation Rejected" },
  { id: 4, name: "Quotation Cancelled" },
  { id: 5, name: "Producing" },
  { id: 6, name: "Track Previewing" },
  { id: 7, name: "Producing Requested" },
  { id: 8, name: "Completed" },
  { id: 10, name: "Customer Cancel Request" },
  { id: 11, name: "Cancelled Automatically" },
  { id: 12, name: "Cancelled Manually" },
];
const BookingsPage = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<
    number | null
  >(null);

  // 🟢 Gọi API thật với refetch on mount và focus
  const {
    data: bookings,
    isFetching: isLoading,
    error,
  } = useGetBookingsQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

  const navigate = useNavigate();

  // Định nghĩa thứ tự ưu tiên cho status
  const statusPriority: Record<number, number> = {
    6: 0,
    2: 1,
    3: 2,
    10: 3,
    11: 4,
    8: 5,
    5: 6,
    4: 7,
    7: 8,
    12: 9,
    // Các status khác sẽ có priority thấp hơn
  };

  // Sắp xếp bookings theo thứ tự ưu tiên
  const sortedBookings =
    bookings?.BookingList.slice().sort((a, b) => {
      const priorityA = statusPriority[a.CurrentStatus.Id] ?? 999;
      const priorityB = statusPriority[b.CurrentStatus.Id] ?? 999;
      return priorityA - priorityB;
    }) ?? [];

  // Apply filters và search
  const filteredBookings = sortedBookings.filter((booking) => {
    // Filter by status
    if (
      selectedStatusFilter !== null &&
      booking.CurrentStatus.Id !== selectedStatusFilter
    ) {
      return false;
    }

    // Search by title (case insensitive)
    if (searchQuery.trim() !== "") {
      const query = searchQuery.toLowerCase();
      const titleMatch = booking.Title.toLowerCase().includes(query);
      const podcasterMatch =
        booking.PodcastBuddy?.FullName?.toLowerCase().includes(query);
      if (!titleMatch && !podcasterMatch) {
        return false;
      }
    }

    return true;
  });

  // 🧮 Xử lý phân trang
  const totalItems = filteredBookings.length;
  const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const currentBookings = filteredBookings.slice(startIndex, endIndex);

  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  // Reset về trang 1 khi filter hoặc search thay đổi
  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery, selectedStatusFilter]);

  const handleCreateBooking = () => {
    localStorage.removeItem("selectedPodcaster");
    navigate("/media-player/management/bookings/create");
  };
  // ⚙️ Render
  return (
    <div className="w-full h-full flex flex-col text-white overflow-hidden rounded-3xl">
      {/* HEADER */}
      <div className="h-24 flex items-center px-8 text-4xl font-bold rounded-t-3xl">
        Bookings
      </div>

      {/* ACTION BAR: FILTER HERE, SEARCH BY NAME HERE */}
      <div className="h-16 mb-5 bg-white//10 text-black flex items-center justify-between px-6 font-medium shadow-md">
        <div className="flex items-center gap-4 w-full">
          {/* Search Input */}
          <input
            type="text"
            placeholder="Search by title or podcaster..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="px-4 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 w-64 placeholder:text-white placeholder:font-light bg-transparent"
          />

          {/* Status Filter */}
          <select
            value={selectedStatusFilter ?? ""}
            onChange={(e) =>
              setSelectedStatusFilter(
                e.target.value ? Number(e.target.value) : null
              )
            }
            className="px-2 py-2 rounded-lg border border-gray-300 focus:outline-none focus:ring-[1px] focus:ring-white bg-transparent text-white placeholder:text-white placeholder:font-light"
          >
            <option value="" className="text-[#252525]">
              All Statuses
            </option>
            {BookingStatusesForFilter.map((status) => (
              <option
                className="text-[#252525]"
                key={status.id}
                value={status.id}
              >
                {status.name}
              </option>
            ))}
          </select>

          {/* Clear Filters Button */}
          {(searchQuery || selectedStatusFilter !== null) && (
            <button
              onClick={() => {
                setSearchQuery("");
                setSelectedStatusFilter(null);
              }}
              className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition"
            >
              Clear Filters
            </button>
          )}

          <div className="flex-1 flex items-center justify-end">
            <ShowOnHoverButton
              Icon={RiFileAddFill}
              onClick={() => handleCreateBooking()}
              text="Create New Booking"
              bgColor="#1b81cf"
            />
          </div>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col min-h-0">
        {/* TABLE HEADER */}
        <div className="grid grid-cols-12 items-center px-6 py-3 font-semibold text-[#d9d9d9] sticky top-0 z-10">
          <div className="col-span-1 pl-3">Id</div>
          <div className="col-span-3">Title</div>
          <div className="col-span-2">Podcaster Name</div>
          <div className="col-span-1">Price (VND)</div>
          <div className="col-span-2 text-center">Deadline</div>
          <div className="col-span-2 text-center">Status</div>
          <div className="col-span-1 text-center"></div>
        </div>

        {/* TABLE BODY */}
        <div
          id="scrollbar-hide"
          className="flex-1 overflow-y-auto px-3 py-4 space-y-7 min-h-0"
        >
          {isLoading ? (
            // ⏳ Hiển thị Skeleton khi đang load
            Array.from({ length: 4 }).map((_, i) => (
              <Skeleton
                key={i}
                className="w-full h-20 bg-white/10 rounded-lg"
              />
            ))
          ) : error ? (
            <div className="text-center text-red-400">
              Lỗi khi tải danh sách booking
            </div>
          ) : currentBookings.length === 0 ? (
            <div className="text-center text-gray-400">
              Không có booking nào.
            </div>
          ) : (
            currentBookings.map((b) => <BookingCard key={b.Id} booking={b} />)
          )}
        </div>

        {/* PAGINATION */}
        {totalPages > 1 && (
          <div className="h-20 flex items-center justify-center rounded-b-3xl text-white">
            <Pagination>
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious
                    href="#"
                    onClick={() => handlePageChange(currentPage - 1)}
                    className="pagination-link"
                  />
                </PaginationItem>

                {Array.from({ length: totalPages }).map((_, index) => (
                  <PaginationItem key={index}>
                    <PaginationLink
                      href="#"
                      isActive={currentPage === index + 1}
                      onClick={() => handlePageChange(index + 1)}
                      className={`pagination-link ${
                        currentPage === index + 1 ? "bg-white/20 font-bold" : ""
                      }`}
                    >
                      {index + 1}
                    </PaginationLink>
                  </PaginationItem>
                ))}

                <PaginationItem>
                  <PaginationNext
                    href="#"
                    onClick={() => handlePageChange(currentPage + 1)}
                    className="pagination-link"
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          </div>
        )}
      </div>
    </div>
  );
};

export default BookingsPage;
