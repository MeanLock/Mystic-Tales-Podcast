const AboutPage = () => {
  return (
    <div className="w-full  scrollbar-hide overflow-y-auto flex flex-col items-center gap-20">
      <div className="flex items-end justify-between w-full px-20 gap-40">
        <div className="flex flex-col items-start font-poppins">
          <p className="text-xl font-medium text-white">IF YOU DARE ENTER</p>
          <p className="text-9xl font-bold text-white">MYSTIC</p>
          <p className="text-9xl font-bold text-white">TALES</p>
          <p className="text-9xl font-bold text-white">PODCAST</p>
          <div className="w-full flex items-center gap-10 mt-5">
            <div className="text-xl font-semibold bg-mystic-green px-7 py-2 rounded-full">
              <p>DISCOVERY OUR PODCAST</p>
            </div>
            <div className="text-xl text-white font-semibold bg-transparent border-2 border-white px-7 py-2 rounded-full">
              <p>LEARN MORE</p>
            </div>
          </div>
        </div>
        <div className="h-fit flex flex-col items-start justify-end">
          <p className="text-mystic-green text-lg">
            EXPERIENCE THE FRIGHT OF YOUR LIFE
          </p>
          <p className="text-lg text-white">
            Midnight Murmur is a storytelling podcast platform devoted entirely
            to the strange, the supernatural, and the deeply unsettling. From
            whispered urban legends to slow-burn psychological horror, every
            episode is designed to make you feel like you’ve left the safety of
            your room and stepped into the dark. Here, horror isn’t just a
            category buried under “Entertainment” – it’s the beating heart of
            the entire experience. If you’re tired of scrolling through endless
            comedy and talk shows just to find one scary story, you’ve found
            your home.
          </p>
        </div>
      </div>

      <div className="flex flex-col items-start justify-center font-poppins mt-40">
        <p className="text-9xl text-white font-bold">WHAT AWAITS YOU</p>
        <p className="text-lg text-white font-bold">
          ENTER THE REALM OF REAL NIGHTMARE COMES TO LIFE
        </p>
        <p className="text-lg text-white font-bold">OUR HAUNTED PODCAST</p>
      </div>

      <div className="w-full flex items-center justify-around">
        <div className="w-[200px] h-[450px]">
          <img
            src="/images/aboutUs/monster2.png"
            className="w-full h-full object-cover"
          />
        </div>
      </div>
    </div>
  );
};

export default AboutPage;
