const CategoryPage = () => {
  return (
    <div
      className="
      flex flex-col items-center gap-10 mb-20 p-8
    "
    >
      <div className="w-full flex flex-col items-start justify-center mb-10 gap-2">
        <p className="text-9xl pb-4 font-poppins font-bold text-transparent bg-clip-text bg-gradient-to-r from-[#abbaab] to-[#ffffff]">
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
          come back often and see what’s haunting the charts.
        </p>
      </div>
    </div>
  );
};

export default CategoryPage;
