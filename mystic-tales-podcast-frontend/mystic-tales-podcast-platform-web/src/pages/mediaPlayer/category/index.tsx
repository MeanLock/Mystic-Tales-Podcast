import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import Loading from "@/components/loading";
import { useGetCategoriesQuery } from "@/core/services/category/category.serivce";
import { useNavigate } from "react-router-dom";

const themes = [
  {
    bg: "bg-gradient-to-r from-[#500B28]/38 to-[#E21C34]/24",
    text: "text-[#F56A7B]",
  },
  {
    bg: "bg-gradient-to-r from-[#5305B5]/26 to-[#FFA9EC]/24",
    text: "text-[#FE9DE8]",
  },
  {
    bg: "bg-gradient-to-r from-[#aa4b6b]/26 via-[#6b6b83] to-[#3b8d99]/24",
    text: "text-[#8fafe3]",
  },
  {
    bg: "bg-gradient-to-r from-[#D06814]/26  to-[#FFCDB2]/13",
    text: "text-[#DF7B2A]",
  },
];

const CategoryPage = () => {
  // HOOKS
  const navigate = useNavigate();

  const { data: categoriesData, isFetching: isCategoriesLoading } =
    useGetCategoriesQuery(undefined, {
      refetchOnFocus: true,
      refetchOnReconnect: true,
      refetchOnMountOrArgChange: true,
    });

  if (isCategoriesLoading) {
    return (
      <div className="w-full h-full flex flex-col gap-5 items-center justify-center">
        <Loading />
        <p className="text-[#D9D9D9] font-poppins font-bold">
          Loading categories...
        </p>
      </div>
    );
  }
  
  return (
    <div
      className="
      flex flex-col items-center gap-10 mb-20 p-8
    "
    >
      <div className="w-full flex flex-col items-start justify-center mb-10 gap-2">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-linear-to-r from-[#abbaab] to-[#ffffff]">
          Categories
        </p>
        <p className="font-poppins text-white font-bold">
          Find your next nightmare by vibe, not just by name.
        </p>
        <p className="w-2/3 font-poppins text-[#d9d9d9]">
          From ghost stories and occult mysteries to true crime and cosmic
          horror, each category leads you deeper into the dark.
        </p>
        <p className="font-poppins text-[#d9d9d9]">
          <span className="font-bold text-white">Updated constantly</span> —
          come back often and see what's haunting the charts.
        </p>
      </div>

      {/* Categories Grid */}
      <div className="w-full grid grid-cols-1 md:grid-cols-1 lg:grid-cols-2 gap-20">
        {categoriesData?.PodcastCategoryList.map((category, index) => {
          const theme = themes[index % themes.length];
          return (
            <div
              key={category.Id}
              className="group relative h-75 transition-all duration-300 cursor-pointer ease-out hover:-translate-y-1"
            >
              <div className="absolute h-75 z-10 left-0 bottom-0">
                <AutoResolveImage
                  FileKey={category.MainImageFileKey}
                  type="PodcastPublicSource"
                  imgClassName="w-full h-75 object-cover rounded-bl-[28px]"
                />
              </div>
              <div
                className={`absolute h-2/3 bottom-0 left-0 right-0 ${theme.bg} rounded-[28px] flex items-center justify-end`}
              >
                <div className="w-full flex flex-col px-5 gap-1 items-end justify-between">
                  <h3
                    className={`text-5xl z-20 font-bold ${theme.text} mb-2 transition-colors`}
                  >
                    {category.Name}
                  </h3>
                  <p className="text-md text-white font-medium">
                    {category.PodcastSubCategoryList.length} subcategories
                  </p>
                  <div
                    onClick={() =>
                      navigate(`/media-player/categories/${category.Id}`)
                    }
                    className={`px-12.5 mt-8 bg-white rounded-full py-0.5 font-bold font-poppins ${theme.text} hover:bg-white/70`}
                  >
                    <p>See More</p>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default CategoryPage;
