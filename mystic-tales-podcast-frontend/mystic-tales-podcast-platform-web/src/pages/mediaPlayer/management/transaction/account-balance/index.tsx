// @ts-nocheck
import { useGetAccountBalanceChangeHistoryQuery } from "@/core/services/transaction/transaction.service";
import type {
  BalanceChange,
  TransactionType,
  TransactionStatus,
} from "@/core/types/transaction";
import { useState, useMemo } from "react";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { ArrowUpDown, ArrowUp, ArrowDown, Filter, X } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";

type SortField = "amount" | "date" | "type";
type SortDirection = "asc" | "desc";

const AccountBalanceChangePage = () => {
  const { data: balanceChangeData, isLoading: isBalanceChangeLoading } =
    useGetAccountBalanceChangeHistoryQuery();

  // Sorting state
  const [sortField, setSortField] = useState<SortField>("date");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");

  // Filter state
  const [filterType, setFilterType] = useState<string>("all");
  const [filterStatus, setFilterStatus] = useState<string>("all");
  const [filterDirection, setFilterDirection] = useState<string>("all"); // received/sent/all

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const clearFilters = () => {
    setFilterType("all");
    setFilterStatus("all");
    setFilterDirection("all");
  };

  // Filtered and sorted data
  const processedData = useMemo(() => {
    if (!balanceChangeData?.BalanceChangeHistory) return [];

    let filtered = [...balanceChangeData.BalanceChangeHistory];

    // Apply filters
    if (filterType !== "all") {
      filtered = filtered.filter(
        (item) => item.TransactionType.Id.toString() === filterType
      );
    }

    if (filterStatus !== "all") {
      filtered = filtered.filter(
        (item) => item.TransactionStatus.Id.toString() === filterStatus
      );
    }

    if (filterDirection !== "all") {
      filtered = filtered.filter((item) =>
        filterDirection === "received" ? item.IsReceived : !item.IsReceived
      );
    }

    // Apply sorting
    filtered.sort((a, b) => {
      let compareValue = 0;

      switch (sortField) {
        case "amount":
          compareValue = a.Amount - b.Amount;
          break;
        case "date":
          compareValue =
            new Date(a.CompletedAt).getTime() -
            new Date(b.CompletedAt).getTime();
          break;
        case "type":
          compareValue = a.TransactionType.Name.localeCompare(
            b.TransactionType.Name
          );
          break;
      }

      return sortDirection === "asc" ? compareValue : -compareValue;
    });

    return filtered;
  }, [
    balanceChangeData,
    sortField,
    sortDirection,
    filterType,
    filterStatus,
    filterDirection,
  ]);

  // Paginated data
  const paginatedData = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return processedData.slice(startIndex, endIndex);
  }, [processedData, currentPage, itemsPerPage]);

  const totalPages = Math.ceil(processedData.length / itemsPerPage);

  // Reset to page 1 when filters change
  useMemo(() => {
    setCurrentPage(1);
  }, [filterType, filterStatus, filterDirection, itemsPerPage]);

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat("vi-VN").format(amount);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    }).format(date);
  };

  const getStatusColor = (status: TransactionStatus) => {
    switch (status.Id) {
      case 1:
        return "text-yellow-400";
      case 2:
        return "text-green-400";
      case 3:
        return "text-gray-400";
      case 4:
        return "text-red-400";
      default:
        return "text-white";
    }
  };

  const SortButton = ({
    field,
    label,
  }: {
    field: SortField;
    label: string;
  }) => (
    <button
      onClick={() => handleSort(field)}
      className="flex items-center gap-1 hover:text-mystic-green transition-colors"
    >
      {label}
      {sortField === field ? (
        sortDirection === "asc" ? (
          <ArrowUp className="w-4 h-4" />
        ) : (
          <ArrowDown className="w-4 h-4" />
        )
      ) : (
        <ArrowUpDown className="w-4 h-4 opacity-40" />
      )}
    </button>
  );

  return (
    <div className="w-full h-full min-h-screen p-8">
      <h1 className="font-poppins text-5xl text-white font-bold mb-8">
        Account Balance History
      </h1>

      {/* Filters */}
      <div className="mb-6 p-6 rounded-2xl bg-white/5 backdrop-blur-xl border border-white/10 shadow-xl">
        <div className="flex items-center gap-2 mb-4">
          <Filter className="w-5 h-5 text-mystic-green" />
          <h2 className="text-lg font-semibold text-white">Filters</h2>
          {(filterType !== "all" ||
            filterStatus !== "all" ||
            filterDirection !== "all") && (
            <Button
              onClick={clearFilters}
              variant="ghost"
              size="sm"
              className="ml-auto text-white/70 hover:text-white hover:bg-white/10"
            >
              <X className="w-4 h-4 mr-1" />
              Clear
            </Button>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Transaction Type Filter */}
          <div>
            <label className="text-sm text-white/70 mb-2 block">
              Transaction Type
            </label>
            <Select value={filterType} onValueChange={setFilterType}>
              <SelectTrigger className="bg-white/5 border-white/10 text-white">
                <SelectValue placeholder="All Types" />
              </SelectTrigger>
              <SelectContent className="bg-[#1a1d24] border-white/10 text-white">
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem value="1">Account Balance Deposit</SelectItem>
                <SelectItem value="2">Account Balance Withdrawal</SelectItem>
                <SelectItem value="3">Booking Deposit</SelectItem>
                <SelectItem value="4">Booking Deposit Refund</SelectItem>
                <SelectItem value="6">Booking Pay The Rest</SelectItem>
                <SelectItem value="8">
                  Customer Subscription Cycle Payment
                </SelectItem>
                <SelectItem value="9">
                  Customer Subscription Cycle Payment Refund
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Status Filter */}
          <div>
            <label className="text-sm text-white/70 mb-2 block">Status</label>
            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="bg-white/5 border-white/10 text-white">
                <SelectValue placeholder="All Statuses" />
              </SelectTrigger>
              <SelectContent className="bg-[#1a1d24] border-white/10 text-white">
                <SelectItem value="all">All Statuses</SelectItem>
                <SelectItem value="1">Processing</SelectItem>
                <SelectItem value="2">Success</SelectItem>
                <SelectItem value="3">Cancelled</SelectItem>
                <SelectItem value="4">Error</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Direction Filter */}
          <div>
            <label className="text-sm text-white/70 mb-2 block">
              Direction
            </label>
            <Select value={filterDirection} onValueChange={setFilterDirection}>
              <SelectTrigger className="bg-white/5 border-white/10 text-white">
                <SelectValue placeholder="All Directions" />
              </SelectTrigger>
              <SelectContent className="bg-[#1a1d24] border-white/10 text-white">
                <SelectItem value="all">All Directions</SelectItem>
                <SelectItem value="received">Received (+)</SelectItem>
                <SelectItem value="sent">Sent (-)</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="rounded-2xl bg-white/5 backdrop-blur-xl border border-white/10 shadow-xl overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-white/10 border-b border-white/10">
              <tr>
                <th className="px-6 py-4 text-left text-sm font-semibold text-white">
                  <SortButton field="date" label="Date" />
                </th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-white">
                  <SortButton field="type" label="Type" />
                </th>
                <th className="px-6 py-4 text-left text-sm font-semibold text-white">
                  Status
                </th>
                <th className="px-6 py-4 text-right text-sm font-semibold text-white">
                  <SortButton field="amount" label="Amount" />
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {isBalanceChangeLoading ? (
                // Loading skeleton
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i}>
                    <td className="px-6 py-4">
                      <Skeleton className="h-5 w-32 bg-white/10" />
                    </td>
                    <td className="px-6 py-4">
                      <Skeleton className="h-5 w-48 bg-white/10" />
                    </td>
                    <td className="px-6 py-4">
                      <Skeleton className="h-5 w-24 bg-white/10" />
                    </td>
                    <td className="px-6 py-4">
                      <Skeleton className="h-5 w-32 bg-white/10 ml-auto" />
                    </td>
                  </tr>
                ))
              ) : processedData.length === 0 ? (
                // Empty state
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center">
                    <div className="flex flex-col items-center gap-2">
                      <p className="text-white/70 text-lg">
                        No transactions found
                      </p>
                      <p className="text-white/50 text-sm">
                        Try adjusting your filters
                      </p>
                    </div>
                  </td>
                </tr>
              ) : (
                // Data rows
                paginatedData.map((item, index) => (
                  <tr
                    key={index}
                    className="hover:bg-white/5 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-white/90">
                      {formatDate(item.CompletedAt)}
                    </td>
                    <td className="px-6 py-4 text-sm text-white/90">
                      {item.TransactionType.Name}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`text-sm font-medium ${getStatusColor(
                          item.TransactionStatus
                        )}`}
                      >
                        {item.TransactionStatus.Name}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      <span
                        className={`text-sm font-bold ${
                          item.IsReceived ? "text-green-400" : "text-red-400"
                        }`}
                      >
                        {item.IsReceived ? "+" : "-"}
                        {formatAmount(item.Amount)} VND
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Summary Footer */}
        {!isBalanceChangeLoading && processedData.length > 0 && (
          <div className="px-6 py-4 bg-white/5 border-t border-white/10">
            {/* Pagination Controls */}
            <div className="flex items-center justify-between mb-4 pb-4 border-b border-white/10">
              <div className="flex items-center gap-4">
                <p className="text-sm text-white/70">
                  Showing {(currentPage - 1) * itemsPerPage + 1} to{" "}
                  {Math.min(currentPage * itemsPerPage, processedData.length)}{" "}
                  of {processedData.length} transaction
                  {processedData.length !== 1 ? "s" : ""}
                </p>
                <div className="flex items-center gap-2">
                  <label className="text-sm text-white/70">Per page:</label>
                  <Select
                    value={itemsPerPage.toString()}
                    onValueChange={(value) => setItemsPerPage(Number(value))}
                  >
                    <SelectTrigger className="w-20 h-8 bg-white/5 border-white/10 text-white">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent className="bg-[#1a1d24] border-white/10 text-white">
                      <SelectItem value="10">10</SelectItem>
                      <SelectItem value="25">25</SelectItem>
                      <SelectItem value="50">50</SelectItem>
                      <SelectItem value="100">100</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <Button
                  onClick={() => setCurrentPage(1)}
                  disabled={currentPage === 1}
                  variant="ghost"
                  size="sm"
                  className="text-white/70 hover:text-white hover:bg-white/10 disabled:opacity-30"
                >
                  First
                </Button>
                <Button
                  onClick={() =>
                    setCurrentPage((prev) => Math.max(1, prev - 1))
                  }
                  disabled={currentPage === 1}
                  variant="ghost"
                  size="sm"
                  className="text-white/70 hover:text-white hover:bg-white/10 disabled:opacity-30"
                >
                  Previous
                </Button>

                <div className="flex items-center gap-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (currentPage <= 3) {
                      pageNum = i + 1;
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = currentPage - 2 + i;
                    }

                    return (
                      <Button
                        key={pageNum}
                        onClick={() => setCurrentPage(pageNum)}
                        variant={currentPage === pageNum ? "default" : "ghost"}
                        size="sm"
                        className={
                          currentPage === pageNum
                            ? "bg-mystic-green text-black hover:bg-mystic-green/90"
                            : "text-white/70 hover:text-white hover:bg-white/10"
                        }
                      >
                        {pageNum}
                      </Button>
                    );
                  })}
                </div>

                <Button
                  onClick={() =>
                    setCurrentPage((prev) => Math.min(totalPages, prev + 1))
                  }
                  disabled={currentPage === totalPages}
                  variant="ghost"
                  size="sm"
                  className="text-white/70 hover:text-white hover:bg-white/10 disabled:opacity-30"
                >
                  Next
                </Button>
                <Button
                  onClick={() => setCurrentPage(totalPages)}
                  disabled={currentPage === totalPages}
                  variant="ghost"
                  size="sm"
                  className="text-white/70 hover:text-white hover:bg-white/10 disabled:opacity-30"
                >
                  Last
                </Button>
              </div>
            </div>

            {/* Summary Totals */}
            <div className="flex items-center justify-between">
              <p className="text-sm text-white/70">
                Total filtered: {processedData.length} transaction
                {processedData.length !== 1 ? "s" : ""}
              </p>
              <div className="flex items-center gap-6">
                <div className="text-sm">
                  <span className="text-white/70">Total In: </span>
                  <span className="text-green-400 font-bold">
                    +
                    {formatAmount(
                      processedData
                        .filter((t) => t.IsReceived)
                        .reduce((sum, t) => sum + t.Amount, 0)
                    )}{" "}
                    VND
                  </span>
                </div>
                <div className="text-sm">
                  <span className="text-white/70">Total Out: </span>
                  <span className="text-red-400 font-bold">
                    -
                    {formatAmount(
                      processedData
                        .filter((t) => !t.IsReceived)
                        .reduce((sum, t) => sum + t.Amount, 0)
                    )}{" "}
                    VND
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
export default AccountBalanceChangePage;
