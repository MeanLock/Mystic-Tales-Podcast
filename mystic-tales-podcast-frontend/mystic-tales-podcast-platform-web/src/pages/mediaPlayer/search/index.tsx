import { useGetSearchResultsQuery } from "@/core/services/search/search.service";
import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

const SearchPage = () => {
  // STATES

  // HOOKS
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const keyword = searchParams.get("keyword");

  const { data: searchDataRaw, isLoading: isSearchDataLoading } =
    useGetSearchResultsQuery(
      { keyword: keyword || "" },
      {
        skip: !keyword || keyword.trim() === "",
      }
    );

  useEffect(() => {
    const resolveSearchData = async () => {
      if (!keyword || keyword.trim() === "") {
        navigate("/media-player/discovery");
      }
    };

    resolveSearchData();
  }, [keyword, searchDataRaw, navigate]);

  return (
    <div>
      <p>Keyword is: {keyword}</p>
    </div>
  );
};

export default SearchPage;
