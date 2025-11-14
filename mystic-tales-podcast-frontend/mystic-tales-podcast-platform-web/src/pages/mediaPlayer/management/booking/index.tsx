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

const BookingsPage = () => {
  const [currentPage, setCurrentPage] = useState(1);

  // 🟢 Gọi API thật
  const { data: bookings, isLoading, error } = useGetBookingsQuery();

  const navigate = useNavigate();
  // 🧮 Xử lý phân trang
  const totalItems = bookings?.BookingList.length ?? 0;
  const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const endIndex = startIndex + ITEMS_PER_PAGE;
  const currentBookings =
    bookings?.BookingList.slice(startIndex, endIndex) ?? [];

  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

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

      {/* ACTION BAR */}
      <div className="h-16 bg-white text-black flex items-center px-6 font-bold text-lg shadow-[5px_5px_10px_#0000005c]">
        <ShowOnHoverButton
          Icon={RiFileAddFill}
          onClick={() => handleCreateBooking()}
          text="Create New Booking"
          bgColor="#1b81cf"
        />
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
