import type { ContentRealtimeResponse } from "@/core/types/search";
import { IoMdMicrophone } from "react-icons/io";
import { RiSlideshow4Line } from "react-icons/ri";
import {
  resolveFiles,
  type FileResolveConfig,
} from "@/core/utils/fileResolver.util";
import { useEffect, useState } from "react";

type SearchSuggestionProps = {
  keywords: string[];
  contents: ContentRealtimeResponse[];
  isLoading: boolean;
  onKeywordClick: (keyword: string) => void;
  onContentClick: (content: ContentRealtimeResponse) => void;
};

const FileConfig: FileResolveConfig[] = [
  {
    path: "MainImageFileKey",
    output: "ImageUrl",
    type: "PodcastPublic",
  },
];

const SearchSuggesstion = ({
  keywords,
  contents,
  isLoading,
  onKeywordClick,
  onContentClick,
}: SearchSuggestionProps) => {
  const [resolvedContents, setResolvedContents] = useState<
    ContentRealtimeResponse[]
  >([]);

  useEffect(() => {
    const resolveContent = async () => {
      if (contents.length > 0) {
        const resolved = await Promise.all(
          contents.map(async (content) => {
            if (content.Show) {
              const { resolvedData } = await resolveFiles(
                content.Show,
                FileConfig
              );
              return { ...content, Show: resolvedData as any };
            }
            if (content.Episode) {
              const { resolvedData } = await resolveFiles(
                content.Episode,
                FileConfig
              );
              return { ...content, Episode: resolvedData as any };
            }
            return content;
          })
        );
        setResolvedContents(resolved);
      } else {
        setResolvedContents([]);
      }
    };

    resolveContent();
  }, [contents]);

  if (isLoading) {
    return (
      <div className="w-full bg-white/10 backdrop-blur-lg rounded-lg p-3 shadow-xl border border-white/20">
        <p className="text-white text-sm">Searching...</p>
      </div>
    );
  }

  if (keywords.length === 0 && resolvedContents.length === 0) {
    return null;
  }

  return (
    <div className="w-full  backdrop-blur-lg rounded-lg shadow-xl border border-white/20 max-h-[400px] overflow-y-auto">
      {/* Keywords Section */}
      {keywords.length > 0 && (
        <div className="p-3 border-b border-white/10">
          <div className="flex flex-col gap-1">
            {keywords.map((keyword, index) => (
              <div
                key={index}
                onClick={() => onKeywordClick(keyword)}
                className="px-3 py-2 hover:bg-white/10 rounded-md cursor-pointer transition-colors"
              >
                <p className="text-[#252525] text-sm">{keyword}</p>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Contents Section */}
      {resolvedContents.length > 0 && (
        <div className="p-3">
          <p className="text-xs text-gray-400 mb-2 font-semibold">Results</p>
          <div className="flex flex-col gap-2">
            {resolvedContents.map((content, index) => {
              const item = content.Show || content.Episode;
              if (!item) return null;

              return (
                <div
                  key={index}
                  onClick={() => onContentClick(content)}
                  className="flex items-center gap-3 px-3 py-2 hover:bg-white/10 rounded-md cursor-pointer transition-colors"
                >
                  <img
                    src={
                      (item as any).ImageUrl || "/images/unknown/podcast.png"
                    }
                    alt={item.Name}
                    className="w-12 h-12 rounded-md object-cover"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      {content.Show ? (
                        <RiSlideshow4Line
                          size={12}
                          className="text-mystic-green flex-shrink-0"
                        />
                      ) : (
                        <IoMdMicrophone
                          size={12}
                          className="text-mystic-green flex-shrink-0"
                        />
                      )}
                      <p className="text-white text-sm font-semibold line-clamp-1">
                        {item.Name}
                      </p>
                    </div>
                    <p className="text-gray-400 text-xs line-clamp-1 mt-1">
                      {item.Description}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchSuggesstion;
