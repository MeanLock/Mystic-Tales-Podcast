// @ts-nocheck
import Loading from "@/components/loading";
import {
  useGetListenHistoryQuery,
  type ListenHistory,
} from "@/core/services/episode/episode.service";
import { useEffect, useState } from "react";
import HistoryRow from "./components/HistoryRow";

type ListenHistoryMapping = {
  DateString: string;
  Histories: ListenHistory[];
};

const RecentPage = () => {
  // STATES
  const [listenHistoryByDate, setListenHistoryByDate] = useState<
    ListenHistoryMapping[]
  >([]);

  // HOOKS
  const { data, error, isLoading, refetch } = useGetListenHistoryQuery();

  // Helper function to check if a date is today
  const isToday = (dateString: string): boolean => {
    const date = new Date(dateString);
    const today = new Date();
    return (
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()
    );
  };

  // Helper function to format date as DD/MM/YYYY
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  };

  // Helper function to get date key (YYYY-MM-DD for grouping)
  const getDateKey = (dateString: string): string => {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  // Process and group listen history by date
  useEffect(() => {
    if (
      !data ||
      !data.PodcastEpisodeListenHistory ||
      data.PodcastEpisodeListenHistory.length === 0
    ) {
      setListenHistoryByDate([]);
      return;
    }

    // Group histories by date
    const groupedByDate: Record<string, ListenHistory[]> = {};

    data.PodcastEpisodeListenHistory.forEach((history) => {
      const dateKey = getDateKey(history.CreatedAt);
      if (!groupedByDate[dateKey]) {
        groupedByDate[dateKey] = [];
      }
      groupedByDate[dateKey].push(history);
    });

    // Sort histories within each date group by time (latest first)
    Object.keys(groupedByDate).forEach((dateKey) => {
      groupedByDate[dateKey].sort((a, b) => {
        return (
          new Date(b.CreatedAt).getTime() - new Date(a.CreatedAt).getTime()
        );
      });
    });

    // Convert to array and sort by date (latest date first)
    const mappedHistories: ListenHistoryMapping[] = Object.keys(groupedByDate)
      .sort((a, b) => new Date(b).getTime() - new Date(a).getTime())
      .map((dateKey) => {
        const firstHistory = groupedByDate[dateKey][0];
        const dateString = isToday(firstHistory.CreatedAt)
          ? "Today"
          : formatDate(firstHistory.CreatedAt);

        return {
          DateString: dateString,
          Histories: groupedByDate[dateKey],
        };
      });

    setListenHistoryByDate(mappedHistories);
  }, [data]);

  if (isLoading) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <Loading />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          Loading Listen History...
        </p>
      </div>
    );
  }

  if (!data || data?.PodcastEpisodeListenHistory.length === 0) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center gap-5">
        <p className="font-poppins font-bold text-[#D9D9D9]">
          No recent listened episodes.
        </p>
      </div>
    );
  }

  return (
    <div className="scrollbar-hide w-full h-full flex flex-col overflow-y-auto">
      <p className="font-poppins text-white text-5xl m-8 font-bold">
        Listen History
      </p>

      <div className="flex flex-col gap-10 px-8 pb-8 mt-10">
        {listenHistoryByDate.map((dateGroup, idx) => (
          <div key={idx} className="flex flex-col gap-4">
            <h2 className="text-2xl font-bold text-white">
              {dateGroup.DateString}
            </h2>
            <div className="flex flex-col gap-3">
              {dateGroup.Histories.map((history) => (
                <HistoryRow
                  key={history.PodcastEpisode.Id + history.CreatedAt}
                  history={history}
                  onListen={async () => {await refetch();}}
                />
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default RecentPage;
